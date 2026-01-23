using EnviadorInformacionService.Contabilidad;
using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturadorEstacionesRepositorio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public class ProtocoloSiesaCanastilla
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConexionEstacionRemota _conexionEstacionRemota = new ConexionEstacionRemota();
        
        // Control de rate limiting: máximo 25 facturas por minuto
        private static readonly int MAX_FACTURAS_POR_MINUTO = 25;
        private static readonly Queue<DateTime> facturasTimestamps = new Queue<DateTime>();
        private static readonly object rateLimitLock = new object();

        public void Ejecutar()
        {
            var _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            var _apiContabilidad = new ApiSiesa();

            var estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            Logger.Info("Iniciando interfaz Siesa para Canastilla");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");

            while (true)
            {
                try
                {
                    var facturasCanastilla = _estacionesRepositorio.BuscarFacturasCanastillaNoEnviadasSiesa();

                    // Procesar terceros primero
                    var terceros = facturasCanastilla.Select(x => x.terceroId)
                        .Where(x => x != null)
                        .GroupBy(t => t.terceroId)
                        .Select(g => g.First())
                        .ToList();

                    if (terceros.Any(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                    {
                        var tercerosEnviados = new List<int>();
                        var tercerosFallidos = new List<string>();

                        foreach (var t in terceros.Where(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                        {
                            Logger.Info($"Canastilla - Iniciando envío de tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            if (_apiContabilidad.EnviarTercero(t))
                            {
                                tercerosEnviados.Add(t.terceroId);
                                Logger.Info($"Canastilla - Tercero enviado exitosamente - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            }
                            else
                            {
                                tercerosFallidos.Add($"ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                Logger.Warn($"Canastilla - Fallo al enviar tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            }
                        }

                        if (tercerosEnviados.Any())
                        {
                            _estacionesRepositorio.MarcarTercerosEnviadosASiesa(tercerosEnviados);
                            Logger.Info($"Canastilla - Total terceros enviados exitosamente: {tercerosEnviados.Count} - IDs: {string.Join(", ", tercerosEnviados)}");
                        }

                        if (tercerosFallidos.Any())
                        {
                            Logger.Warn($"Canastilla - Total terceros que fallaron al enviar: {tercerosFallidos.Count} - {string.Join(" | ", tercerosFallidos)}");
                        }
                    }

                    // Procesar facturas canastilla
                    var facturasEnviadas = new List<int>();
                    var facturasFallidas = new List<string>();

                    foreach (var facturaCanastilla in facturasCanastilla)
                    {
                        try
                        {
                            Logger.Info($"Procesando factura canastilla {facturaCanastilla.FacturasCanastillaId} con forma de pago {facturaCanastilla.codigoFormaPago}");

                            // Obtener detalle de la factura canastilla
                            var detalle = _estacionesRepositorio.getFacturaCanatillaDetalle(facturaCanastilla.FacturasCanastillaId);

                            if (detalle == null || !detalle.Any())
                            {
                                Logger.Warn($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} no tiene detalle");
                                continue;
                            }

                            // Determinar si tiene IVA (al menos un item con IVA > 0)
                            bool tieneIva = detalle.Any(d => d.Canastilla != null && d.Canastilla.iva > 0);

                            // Obtener configuración según si tiene IVA o no
                            string sufijo = tieneIva ? "canastillaconiva" : "canastillasiniva";

                            var documentoCruce = ConfigurationManager.AppSettings[$"documentocruce{sufijo}"];
                            var consecutivoAutoregulado = ConfigurationManager.AppSettings[$"consecutivoautoregulado{sufijo}"];
                            var centroOperacionesDocumento = ConfigurationManager.AppSettings[$"centrooperacionesdocuemnto{sufijo}"];
                            var idfe = ConfigurationManager.AppSettings[$"idfe{sufijo}"];
                            var sucursal = ConfigurationManager.AppSettings[$"sucursal{sufijo}"];
                            var idDocumento = ConfigurationManager.AppSettings[$"iddocumento{sufijo}"];
                            var idDocumentoCliente = ConfigurationManager.AppSettings[$"iddocumentocliente{sufijo}"];

                            // Configuración común
                            var urlSiesa = ConfigurationManager.AppSettings["urlsiesa"];
                            var idSistema = ConfigurationManager.AppSettings["idsistema"];
                            var idCompania = ConfigurationManager.AppSettings["idcompania"];
                            var vendedor = ConfigurationManager.AppSettings["vendedor"];

                            Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} - Usando configuración: {(tieneIva ? "CON IVA" : "SIN IVA")}");

                            // Calcular totales
                            decimal subtotal = 0;
                            decimal totalIva = 0;
                            decimal total = 0;

                            foreach (var item in detalle)
                            {
                                if (item.Canastilla != null)
                                {
                                    decimal valorItem = (decimal)(item.cantidad * item.Canastilla.precio);
                                    decimal ivaItem = 0;

                                    if (item.Canastilla.iva > 0)
                                    {
                                        ivaItem = valorItem * (item.Canastilla.iva / 100m);
                                    }

                                    subtotal += valorItem;
                                    totalIva += ivaItem;
                                }
                            }

                            total = subtotal + totalIva;

                            // Crear objeto de factura para Siesa
                            var facturaParaSiesa = new
                            {
                                facturaCanastilla.FacturasCanastillaId,
                                facturaCanastilla.consecutivo,
                                facturaCanastilla.fecha,
                                Tercero = facturaCanastilla.terceroId,
                                facturaCanastilla.codigoFormaPago,
                                Detalle = detalle,
                                Subtotal = subtotal,
                                TotalIva = totalIva,
                                Total = total,
                                TieneIva = tieneIva,
                                Configuracion = new
                                {
                                    DocumentoCruce = documentoCruce,
                                    ConsecutivoAutoregulado = consecutivoAutoregulado,
                                    CentroOperacionesDocumento = centroOperacionesDocumento,
                                    Idfe = idfe,
                                    Sucursal = sucursal,
                                    IdDocumento = idDocumento,
                                    IdDocumentoCliente = idDocumentoCliente,
                                    UrlSiesa = urlSiesa,
                                    IdSistema = idSistema,
                                    IdCompania = idCompania,
                                    Vendedor = vendedor
                                }
                            };

                            Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} - Subtotal: {subtotal}, IVA: {totalIva}, Total: {total}");

                            // Control de rate limiting
                            WaitForRateLimit();
                            
                            // Aquí se haría el envío a Siesa
                            // Por ahora lo dejamos como placeholder - debes implementar el envío real basado en tu ApiSiesa
                            bool enviado = EnviarFacturaCanastillaASiesa(facturaParaSiesa, _apiContabilidad);

                            if (enviado)
                            {
                                facturasEnviadas.Add(facturaCanastilla.FacturasCanastillaId);
                                Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} enviada exitosamente a Siesa");
                            }
                            else
                            {
                                facturasFallidas.Add($"ID: {facturaCanastilla.FacturasCanastillaId}, Total: {total}");
                                Logger.Warn($"Fallo al enviar factura canastilla {facturaCanastilla.FacturasCanastillaId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            facturasFallidas.Add($"ID: {facturaCanastilla.FacturasCanastillaId}, Error: {ex.Message}");
                            Logger.Error(ex, $"Error procesando factura canastilla {facturaCanastilla.FacturasCanastillaId}");
                        }
                    }

                    // Actualizar facturas enviadas
                    if (facturasEnviadas.Any())
                    {
                        _estacionesRepositorio.ActualizarFacturasCanastillaEnviadasSiesa(facturasEnviadas);
                        Logger.Info($"Total facturas canastilla enviadas exitosamente: {facturasEnviadas.Count} - IDs: {string.Join(", ", facturasEnviadas)}");
                    }

                    if (facturasFallidas.Any())
                    {
                        Logger.Warn($"Total facturas canastilla que fallaron: {facturasFallidas.Count} - {string.Join(" | ", facturasFallidas)}");
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error en el proceso de Siesa Canastilla");
                    Thread.Sleep(5000);
                }
            }
        }

        /// <summary>
        /// Envía una factura canastilla a Siesa
        /// </summary>
        /// <param name="facturaCanastilla">Objeto con la información de la factura</param>
        /// <param name="apiContabilidad">API de contabilidad Siesa</param>
        /// <returns>True si se envió exitosamente</returns>
        private bool EnviarFacturaCanastillaASiesa(dynamic facturaCanastilla, ApiSiesa apiContabilidad)
        {
            try
            {
                Logger.Info($"Enviando factura canastilla {facturaCanastilla.FacturasCanastillaId} a Siesa con configuración {(facturaCanastilla.TieneIva ? "CON IVA" : "SIN IVA")}");
                
                var consecutivo = facturaCanastilla.consecutivo.ToString();
                var config = facturaCanastilla.Configuracion;
                
                // Construir el objeto de movimiento contable
                var movimientosContables = new List<object>();
                
                // Agregar movimientos por cada item de la canastilla
                foreach (dynamic item in facturaCanastilla.Detalle)
                {
                    if (item.Canastilla != null)
                    {
                        decimal valorItem = (decimal)(item.cantidad * item.Canastilla.precio);
                        
                        // Obtener auxiliar según tiene IVA o no
                        string auxiliarItem = ConfigurationManager.AppSettings[facturaCanastilla.TieneIva ? "auxiliarcanastillaconiva" : "auxiliarcanastillasiniva"];
                        
                        // Movimiento del producto/servicio
                        movimientosContables.Add(new
                        {
                            F_CIA = "1",
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F351_ID_AUXILIAR = auxiliarItem,
                            F351_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                            F351_ID_CO_MOV = config.CentroOperacionesDocumento,
                            F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"],
                            F351_ID_CCOSTO = "",
                            F351_ID_FE = "",
                            F351_VALOR_DB = "0",
                            F351_VALOR_CR = valorItem.ToString("0.00", CultureInfo.InvariantCulture),
                            F351_BASE_GRAVABLE = "",
                            F351_DOCTO_BANCO = "",
                            F351_NRO_DOCTO_BANCO = "",
                            F351_NOTAS = $"Canastilla {item.Canastilla.descripcion} - Cant: {item.cantidad}"
                        });
                        
                        // Si tiene IVA, agregar movimiento de IVA
                        if (item.Canastilla.iva > 0)
                        {
                            decimal ivaItem = valorItem * (item.Canastilla.iva / 100m);
                            movimientosContables.Add(new
                            {
                                F_CIA = "1",
                                F350_ID_CO = config.CentroOperacionesDocumento,
                                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                                F350_CONSEC_DOCTO = consecutivo,
                                F351_ID_AUXILIAR = "240801", // Auxiliar para IVA generado
                                F351_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                                F351_ID_CO_MOV = config.CentroOperacionesDocumento,
                                F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"],
                                F351_ID_CCOSTO = "",
                                F351_ID_FE = "",
                                F351_VALOR_DB = "0",
                                F351_VALOR_CR = ivaItem.ToString("0.00", CultureInfo.InvariantCulture),
                                F351_BASE_GRAVABLE = valorItem.ToString("0.00", CultureInfo.InvariantCulture),
                                F351_DOCTO_BANCO = "",
                                F351_NRO_DOCTO_BANCO = "",
                                F351_NOTAS = $"IVA {item.Canastilla.iva}% - {item.Canastilla.descripcion}"
                            });
                        }
                    }
                }
                
                // Movimiento de caja/CXC (según forma de pago)
                if (facturaCanastilla.codigoFormaPago.FormaPagoId == 4) // Efectivo
                {
                    return EnviarFacturaCanastillaEfectivo(facturaCanastilla, consecutivo, config, movimientosContables);
                }
                else
                {
                    return EnviarFacturaCanastillaCredito(facturaCanastilla, consecutivo, config, movimientosContables);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura canastilla {facturaCanastilla.FacturasCanastillaId} a Siesa");
                return false;
            }
        }

        /// <summary>
        /// Envía factura canastilla con forma de pago efectivo (usando módulo Caja)
        /// </summary>
        private bool EnviarFacturaCanastillaEfectivo(dynamic facturaCanastilla, string consecutivo, dynamic config, List<object> movimientosContables)
        {
            try
            {
                var requestContent = new
                {
                    Inicial = new List<object> { new { F_CIA = "1" } },
                    Final = new List<object> { new { F_CIA = "1" } },
                    Caja = new List<object>
                    {
                        new
                        {
                            F_CIA = "1",
                            F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescaja"],
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F351_NOTAS = "Venta canastilla",
                            F351_ID_AUXILIAR = ConfigurationManager.AppSettings["cajaotros"] ?? "110501",
                            F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocaja"] ?? "",
                            F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocaja"] ?? "",
                            F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocaja"],
                            F351_VALOR_CR = "0",
                            F351_VALOR_DB = facturaCanastilla.Total.ToString("0.00", CultureInfo.InvariantCulture),
                            F351_ID_FE = config.Idfe,
                            F358_COD_SEGURIDAD = "",
                            F358_FECHA_VCTO = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                            F358_ID_CAJA = ConfigurationManager.AppSettings["caja"] ?? "003",
                            F358_ID_MEDIOS_PAGO = "EFE",
                            F358_NOTAS = $"Factura canastilla {consecutivo}",
                            F358_NRO_AUTORIZACION = "",
                            F358_NRO_CUENTA = "",
                            F358_REFERENCIA_OTROS = ""
                        }
                    },
                    Documentocontable = new List<object>
                    {
                        new
                        {
                            F_CIA = "1",
                            F_CONSEC_AUTO_REG = config.ConsecutivoAutoregulado,
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F350_FECHA = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                            F350_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                            F350_IND_ESTADO = "1",
                            F350_NOTAS = $"Factura canastilla {consecutivo}"
                        }
                    },
                    Movimientocontable = movimientosContables
                };

                return EnviarASiesaAPI(requestContent, consecutivo, "Canastilla-Efectivo");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura canastilla efectivo {consecutivo}");
                return false;
            }
        }

        /// <summary>
        /// Envía factura canastilla con otras formas de pago (usando CXC)
        /// </summary>
        private bool EnviarFacturaCanastillaCredito(dynamic facturaCanastilla, string consecutivo, dynamic config, List<object> movimientosContables)
        {
            try
            {
                // Agregar movimiento de CXC
                movimientosContables.Add(new
                {
                    F_CIA = "1",
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesotros"],
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = ConfigurationManager.AppSettings["cajaotros"] ?? "110501",
                    F351_ID_TERCERO = "",
                    F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientootros"],
                    F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociootros"],
                    F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostootros"] ?? "",
                    F351_ID_FE = ConfigurationManager.AppSettings["idfeotros"],
                    F351_VALOR_DB = facturaCanastilla.Total.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "0",
                    F351_BASE_GRAVABLE = "",
                    F351_DOCTO_BANCO = "CG",
                    F351_NRO_DOCTO_BANCO = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                    F351_NOTAS = $"Factura canastilla {consecutivo}"
                });

                var requestContent = new
                {
                    Inicial = new List<object> { new { F_CIA = "1" } },
                    Final = new List<object> { new { F_CIA = "1" } },
                    Documentocontable = new List<object>
                    {
                        new
                        {
                            F_CIA = "1",
                            F_CONSEC_AUTO_REG = config.ConsecutivoAutoregulado,
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F350_FECHA = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                            F350_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                            F350_IND_ESTADO = "1",
                            F350_NOTAS = $"Factura canastilla {consecutivo}"
                        }
                    },
                    Movimientocontable = movimientosContables
                };

                return EnviarASiesaAPI(requestContent, consecutivo, "Canastilla-Credito");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura canastilla crédito {consecutivo}");
                return false;
            }
        }

        /// <summary>
        /// Realiza el envío HTTP a la API de Siesa
        /// </summary>
        private bool EnviarASiesaAPI(object requestContent, string consecutivo, string tipoFactura)
        {
            var responseString = "";
            var contentString = JsonConvert.SerializeObject(requestContent);
            
            try
            {
                var urlSiesa = ConfigurationManager.AppSettings["urlsiesa"];
                var idSistema = ConfigurationManager.AppSettings["idsistema"];
                var idCompania = ConfigurationManager.AppSettings["idcompania"];
                var idDocumento = ConfigurationManager.AppSettings["iddocumento"];
                
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                
                var request = new HttpRequestMessage(
                    HttpMethod.Post, 
                    $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idSistema}&idDocumento={idDocumento}&nombreDocumento=Documento_Contablev2"
                );
                
                request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"]);
                request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"]);
                
                var content = new StringContent(contentString, null, "application/json");
                request.Content = content;
                
                var response = client.SendAsync(request).Result;
                responseString = response.Content.ReadAsStringAsync().Result;
                
                // Si es Bad Request, verificar si el documento ya existe
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    if (responseString.Contains("El documento ya existe"))
                    {
                        Logger.Info($"{tipoFactura} - Factura ya existe en Siesa (marcada como exitosa) - Consecutivo: {consecutivo}. Respuesta: {responseString}");
                        return true; // Tratarla como exitosa
                    }
                    else
                    {
                        Logger.Warn($"{tipoFactura} - Factura no enviada (Bad Request) - Consecutivo: {consecutivo}. Respuesta: {responseString}");
                        return false;
                    }
                }
                
                response.EnsureSuccessStatusCode();
                Logger.Info($"{tipoFactura} - Factura enviada exitosamente - Consecutivo: {consecutivo}. Respuesta: {responseString}");
                return true;
            }
            catch (HttpRequestException ex)
            {
                Logger.Warn($"{tipoFactura} - Error HttpRequest enviando factura {consecutivo}. Respuesta: {responseString}. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                if (responseString.Contains("El documento ya existe"))
                {
                    Logger.Info($"{tipoFactura} - Factura ya existe en Siesa (marcada como exitosa) - Consecutivo: {consecutivo}. Respuesta: {responseString}");
                    return true;
                }
                
                Logger.Error(ex, $"{tipoFactura} - Error enviando factura {consecutivo}. Respuesta: {responseString}");
                return false;
            }
        }

        /// <summary>
        /// Controla el rate limiting para no exceder 25 facturas por minuto
        /// </summary>
        private void WaitForRateLimit()
        {
            lock (rateLimitLock)
            {
                DateTime now = DateTime.Now;
                DateTime oneMinuteAgo = now.AddMinutes(-1);

                // Eliminar timestamps antiguos (fuera de la ventana de 1 minuto)
                while (facturasTimestamps.Count > 0 && facturasTimestamps.Peek() < oneMinuteAgo)
                {
                    facturasTimestamps.Dequeue();
                }

                // Si ya alcanzamos el límite, esperar hasta que la factura más antigua salga de la ventana
                if (facturasTimestamps.Count >= MAX_FACTURAS_POR_MINUTO)
                {
                    DateTime oldestTimestamp = facturasTimestamps.Peek();
                    TimeSpan waitTime = oldestTimestamp.AddMinutes(1).AddSeconds(1) - now;
                    
                    if (waitTime.TotalSeconds > 0)
                    {
                        int waitSeconds = (int)Math.Ceiling(waitTime.TotalSeconds);
                        Logger.Warn($"⏳ Límite de {MAX_FACTURAS_POR_MINUTO} facturas/minuto alcanzado. Esperando {waitSeconds} segundos...");
                        Thread.Sleep(waitTime);
                        
                        // Limpiar timestamps antiguos después de esperar
                        now = DateTime.Now;
                        oneMinuteAgo = now.AddMinutes(-1);
                        while (facturasTimestamps.Count > 0 && facturasTimestamps.Peek() < oneMinuteAgo)
                        {
                            facturasTimestamps.Dequeue();
                        }
                    }
                }

                // Registrar el nuevo timestamp
                facturasTimestamps.Enqueue(now);
            }
        }
    }
}
