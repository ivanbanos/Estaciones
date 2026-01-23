using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using EnviadorInformacionService.Contabilidad;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Convertidor;
using Newtonsoft.Json;
using NLog;

namespace DataicoToSiesa
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string CSV_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ArchivoASubir.csv");
        
        // Control de rate limiting: máximo de requests por minuto (configurable)
        private static int MAX_REQUESTS_PER_MINUTE;
        private static readonly Queue<DateTime> requestTimestamps = new Queue<DateTime>();
        private static readonly object rateLimitLock = new object();
        
        // Database connection
        private static readonly string _connectionStringFacturacion = ConfigurationManager.ConnectionStrings["Facturacion"].ConnectionString;
        private static readonly Convertidor _convertidor = new Convertidor();
        
        static void Main(string[] args)
        {
            try
            {
                // Leer configuración de rate limiting
                string maxRequestsConfig = ConfigurationManager.AppSettings["dataico_max_requests_per_minute"];
                MAX_REQUESTS_PER_MINUTE = !string.IsNullOrEmpty(maxRequestsConfig) && int.TryParse(maxRequestsConfig, out int maxReq) 
                    ? maxReq 
                    : 100; // Default: 25 requests por minuto
                
                Logger.Info("=== Iniciando programa DataicoToSiesa ===");
                Logger.Info($"Rate limit configurado: {MAX_REQUESTS_PER_MINUTE} requests/minuto");
                
                // Validar que el archivo existe
                if (!File.Exists(CSV_FILE_PATH))
                {
                    Logger.Error($"Archivo CSV no encontrado: {CSV_FILE_PATH}");
                    Console.WriteLine("Presione cualquier tecla para salir...");
                    Console.ReadKey();
                    return;
                }

                ProcessInvoices();

                Logger.Info("=== Proceso completado ===");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error fatal en el programa");
            }

            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }

        /// <summary>
        /// Controla el rate limiting para no exceder el límite de requests por minuto.
        /// Espera si es necesario antes de permitir continuar.
        /// </summary>
        static void WaitForRateLimit()
        {
            lock (rateLimitLock)
            {
                DateTime now = DateTime.Now;
                DateTime oneMinuteAgo = now.AddMinutes(-1);

                // Eliminar timestamps antiguos (fuera de la ventana de 1 minuto)
                while (requestTimestamps.Count > 0 && requestTimestamps.Peek() < oneMinuteAgo)
                {
                    requestTimestamps.Dequeue();
                }

                // Si ya alcanzamos el límite, esperar hasta que el request más antiguo salga de la ventana
                if (requestTimestamps.Count >= MAX_REQUESTS_PER_MINUTE)
                {
                    DateTime oldestRequest = requestTimestamps.Peek();
                    TimeSpan waitTime = oldestRequest.AddMinutes(1).AddSeconds(1) - now;
                    
                    if (waitTime.TotalSeconds > 0)
                    {
                        int waitSeconds = (int)Math.Ceiling(waitTime.TotalSeconds);
                        Logger.Warn($"⏳ Límite de {MAX_REQUESTS_PER_MINUTE} requests/minuto alcanzado. Esperando {waitSeconds} segundos...");
                        Thread.Sleep(waitTime);
                        
                        // Limpiar timestamps antiguos después de esperar
                        now = DateTime.Now;
                        oneMinuteAgo = now.AddMinutes(-1);
                        while (requestTimestamps.Count > 0 && requestTimestamps.Peek() < oneMinuteAgo)
                        {
                            requestTimestamps.Dequeue();
                        }
                    }
                }

                // Registrar el nuevo request
                requestTimestamps.Enqueue(now);
                Logger.Debug($"Requests en última minuto: {requestTimestamps.Count}/{MAX_REQUESTS_PER_MINUTE}");
            }
        }

        static void ProcessInvoices()
        {
            // Leer el archivo CSV
            var lines = File.ReadAllLines(CSV_FILE_PATH, Encoding.UTF8).ToList();
            
            if (lines.Count < 2)
            {
                Logger.Warn("El archivo CSV está vacío o solo contiene encabezados");
                return;
            }

            var header = lines[0];
            var dataLines = lines.Skip(1).ToList();

            Logger.Info($"Total de facturas en el archivo: {dataLines.Count}");

            var apiSiesa = new ApiSiesa();
            int procesadas = 0;
            int exitosas = 0;
            int fallidas = 0;
            int yaSubidas = 0;

            for (int i = 0; i < dataLines.Count; i++)
            {
                var line = dataLines[i];
                var columns = ParseCsvLine(line);

                if (columns.Count < 8)
                {
                    Logger.Warn($"Línea {i + 2} no tiene suficientes columnas, saltando...");
                    continue;
                }

                // Columnas: 0=FECHA, 1=FACTURA DATAICO, 2=ORDEN DATAICO, 3=ORDEN SIGES, 4=VALOR SIGES, 5=ESTADO, 6=MOTIVO, 7=VERIFICACION
                var fechaFactura = columns[0];
                var facturaDataico = columns[1];
                var ordenDataico = columns[2];
                var estado = columns[5];
                var verificacion = columns[7];

                // Saltar si ya está subida
                if (estado.Trim().ToUpper() == "SUBIDA")
                {
                    yaSubidas++;
                    continue;
                }


                procesadas++;
                Logger.Info($"\n[{procesadas}] Procesando factura {facturaDataico}...");

                try
                {
                    // 1. Obtener información de Dataico
                    WaitForRateLimit(); // Control de rate limiting
                    var dataicoInfo = GetInvoiceFromDataico(facturaDataico);
                    
                    if (dataicoInfo == null)
                    {
                        Logger.Warn($"No se pudo obtener información de Dataico para {facturaDataico}");
                        fallidas++;
                        UpdateCsvLine(lines, i + 1, 5, "NO SUBIDA");
                        UpdateCsvLine(lines, i + 1, 6, "Error: No se encontró en Dataico");
                        SaveCsv(lines);
                        continue;
                    }

                    // 2. Crear factura para enviar a Siesa
                    var facturaParaSiesa = CreateFacturaFromDataico(dataicoInfo, facturaDataico);

                    // 3. Enviar a Siesa usando ApiSiesa
                    bool enviado = SendToSiesa(facturaParaSiesa, apiSiesa);

                    if (enviado)
                    {
                        exitosas++;
                        Logger.Info($"✓ Factura {facturaDataico} enviada exitosamente a Siesa");
                        
                        // Actualizar CSV
                        UpdateCsvLine(lines, i + 1, 5, "SUBIDA");
                        UpdateCsvLine(lines, i + 1, 6, $"Enviada exitosamente - {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                        SaveCsv(lines);
                    }
                    else
                    {
                        fallidas++;
                        Logger.Warn($"✗ Fallo al enviar factura {facturaDataico} a Siesa");
                        UpdateCsvLine(lines, i + 1, 5, "NO SUBIDA");
                        UpdateCsvLine(lines, i + 1, 6, "Error al enviar a Siesa");
                        SaveCsv(lines);
                    }

                    // Pequeña pausa entre facturas
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    fallidas++;
                    Logger.Error(ex, $"Error procesando factura {facturaDataico}");
                    UpdateCsvLine(lines, i + 1, 5, "NO SUBIDA");
                    UpdateCsvLine(lines, i + 1, 6, $"Error: {ex.Message}");
                    SaveCsv(lines);
                }

                // Mostrar progreso cada 10 facturas
                if (procesadas % 10 == 0)
                {
                    Logger.Info($"Progreso: {procesadas} procesadas | {exitosas} exitosas | {fallidas} fallidas | {yaSubidas} ya subidas");
                }
            }

            Logger.Info($"\n=== RESUMEN FINAL ===");
            Logger.Info($"Total procesadas: {procesadas}");
            Logger.Info($"Exitosas: {exitosas}");
            Logger.Info($"Fallidas: {fallidas}");
            Logger.Info($"Ya subidas (saltadas): {yaSubidas}");
        }

        static dynamic GetInvoiceFromDataico(string numeroFactura)
        {
            try
            {
                string dataicoToken = ConfigurationManager.AppSettings["dataico_token"];
                string url = $"https://api.dataico.com/dataico_api/v2/invoices?number={numeroFactura}";
                
                int maxRetries = 3;
                int retryCount = 0;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(30);
                            var request = new HttpRequestMessage(HttpMethod.Get, url);
                            request.Headers.Add("auth-token", dataicoToken);
                            
                            var response = client.SendAsync(request).Result;
                            
                            if (response.IsSuccessStatusCode)
                            {
                                var json = response.Content.ReadAsStringAsync().Result;
                                
                                if (string.IsNullOrWhiteSpace(json))
                                {
                                    Logger.Error($"Respuesta vacía de Dataico para {numeroFactura}");
                                    return null;
                                }

                                dynamic obj = JsonConvert.DeserializeObject(json);
                                
                                if (obj == null || obj.invoice == null)
                                {
                                    Logger.Error($"Objeto JSON inválido de Dataico para {numeroFactura}");
                                    return null;
                                }

                                Logger.Info($"Información obtenida de Dataico para {numeroFactura}");
                                return obj;
                            }
                            else
                            {
                                Logger.Warn($"Intento {retryCount + 1}: Error {response.StatusCode} al consultar Dataico");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Intento {retryCount + 1}: Error al llamar a Dataico: {ex.Message}");
                    }

                    retryCount++;
                    if (retryCount < maxRetries)
                    {
                        Thread.Sleep(2000); // Esperar 2 segundos antes de reintentar
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error obteniendo factura {numeroFactura} de Dataico");
                return null;
            }
        }

        static dynamic CreateFacturaFromDataico(dynamic dataicoInfo, string numeroFactura)
        {
            try
            {
                // Extraer información de la factura
                string fechaFacturacion = dataicoInfo.invoice.issue_date;
                DateTime fecha = DateTime.ParseExact(fechaFacturacion, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                // Extraer totales
                decimal total = 0, subtotal = 0, descuento = 0, iva = 0;

                // Procesar items
                var items = new List<dynamic>();
                string tipoCombustible = null; // Para detectar si es combustible
                
                if (dataicoInfo.invoice.items != null && dataicoInfo.invoice.items.HasValues)
                {
                    foreach (var item in dataicoInfo.invoice.items)
                    {
                        decimal itemPrice = 0;
                        decimal itemQty = 0;
                        int itemIva = 0;

                        try { itemPrice = (decimal)item.price; } catch { }
                        try { itemQty = (decimal)item.quantity; } catch { }
                        try { itemIva = (int)item.tax_percentage; } catch { }

                        decimal itemTotal = itemPrice * itemQty;
                        decimal itemIvaAmount = itemTotal * (itemIva / 100m);

                        // Normalizar descripción: trim y mayúsculas
                        string descripcion = ((string)item.description ?? "Producto").Trim().ToUpper();
                        
                        // Detectar si es combustible (EXTRA, CORRIENTE, DIESEL, ACPM, GNV, etc.)
                        if (tipoCombustible == null && (
                            descripcion.Contains("EXTRA") || 
                            descripcion.Contains("CORRIENTE") || 
                            descripcion.Contains("DIESEL") || 
                            descripcion.Contains("ACPM") || 
                            descripcion.Contains("GNV")))
                        {
                            tipoCombustible = descripcion;
                            Logger.Info($"Detectado combustible: {tipoCombustible}");
                        }

                        items.Add(new
                        {
                            Descripcion = descripcion,
                            Cantidad = itemQty,
                            Precio = itemPrice,
                            Subtotal = itemTotal,
                            IvaPorcentaje = itemIva,
                            IvaValor = itemIvaAmount,
                            Total = itemTotal + itemIvaAmount
                        });

                        subtotal += itemTotal;
                        iva += itemIvaAmount;
                    }
                }

                // Leer del QR code si existe
                string qrcode = dataicoInfo.invoice.qrcode;
                if (!string.IsNullOrWhiteSpace(qrcode))
                {
                    var lines = qrcode.ToString().Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("ValTolFac="))
                        {
                            var val = line.Substring("ValTolFac=".Length);
                            decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out total);
                        }
                        if (line.StartsWith("ValFac="))
                        {
                            var val = line.Substring("ValFac=".Length);
                            decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out subtotal);
                        }
                        if (line.StartsWith("ValOtroIm="))
                        {
                            var val = line.Substring("ValOtroIm=".Length);
                            decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out descuento);
                        }
                    }
                }

                if (total == 0 && subtotal > 0)
                    total = subtotal + iva;

                // Extraer información del tercero desde Dataico
                string terceroNitDataico = dataicoInfo.invoice.customer?.party_identification?.ToString()?.Trim() ?? "222222222222";
                string terceroNombreDataico = dataicoInfo.invoice.customer?.company_name?.ToString()?.Trim() 
                    ?? dataicoInfo.invoice.customer?.name?.ToString()?.Trim() 
                    ?? "CONSUMIDOR FINAL";
                string terceroEmailDataico = dataicoInfo.invoice.customer?.email?.ToString()?.Trim() ?? "";
                string terceroDireccionDataico = dataicoInfo.invoice.customer?.address_line?.ToString()?.Trim() ?? "";
                string terceroCiudadDataico = dataicoInfo.invoice.customer?.city?.ToString()?.Trim() ?? "";
                string terceroDepartamentoDataico = dataicoInfo.invoice.customer?.department?.ToString()?.Trim() ?? "";
                string terceroTelefonoDataico = "";
                
                // Buscar tercero en base de datos
                Tercero terceroDB = ObtenerTerceroPorIdentificacion(terceroNitDataico);
                
                // Usar datos de la base de datos si existe, sino usar datos de Dataico
                string terceroNit = terceroNitDataico;
                string terceroNombre = terceroDB?.Nombre ?? terceroNombreDataico;
                string terceroEmail = terceroDB?.Correo ?? terceroEmailDataico;
                string terceroDireccion = terceroDB?.Direccion ?? terceroDireccionDataico;
                string terceroTelefono = terceroDB?.Telefono ?? terceroTelefonoDataico;
                
                if (terceroDB != null)
                {
                    Logger.Info($"Tercero encontrado en BD: NIT={terceroNit}, Nombre={terceroNombre}");
                }
                else
                {
                    Logger.Warn($"Tercero NO encontrado en BD: NIT={terceroNit}, usando datos de Dataico: {terceroNombre}");
                }

                // Extraer forma de pago (si existe)
                string paymentMethod = "EFECTIVO";
                try
                {
                    if (dataicoInfo.invoice.payment_method != null)
                    {
                        paymentMethod = dataicoInfo.invoice.payment_method.ToString().ToUpper();
                    }
                }
                catch { }

                var factura = new
                {
                    NumeroFactura = numeroFactura,
                    Fecha = fecha,
                    Subtotal = subtotal,
                    Descuento = descuento,
                    Iva = iva,
                    Total = total,
                    Items = items,
                    Tercero = new
                    {
                        Identificacion = terceroNit,
                        Nombre = terceroNombre
                    },
                    FormaPago = paymentMethod,
                    TieneIva = iva > 0,
                    EsCombustible = !string.IsNullOrEmpty(tipoCombustible),
                    TipoCombustible = tipoCombustible
                };

                Logger.Info($"Factura creada: {numeroFactura} - Total: {total}, IVA: {iva}, Items: {items.Count}, Combustible: {tipoCombustible ?? "N/A"}");
                return factura;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error creando factura desde Dataico para {numeroFactura}");
                throw;
            }
        }

        static bool SendToSiesa(dynamic factura, ApiSiesa apiSiesa)
        {
            string contentString = "";
            try
            {
                // Determinar si es combustible o canastilla
                bool esCombustible = factura.EsCombustible;
                string tipoCombustible = esCombustible ? factura.TipoCombustible : null;
                
                // Determinar configuración según IVA
                bool tieneIva = factura.TieneIva;
                string sufijo = tieneIva ? "canastillaconiva" : "canastillasiniva";

                var config = new
                {
                    ConsecutivoAutoregulado = ConfigurationManager.AppSettings[$"consecutivoautoregulado{sufijo}"],
                    CentroOperacionesDocumento = ConfigurationManager.AppSettings[$"centrooperacionesdocuemnto{sufijo}"],
                    Idfe = ConfigurationManager.AppSettings[$"idfe{sufijo}"],
                    Sucursal = ConfigurationManager.AppSettings[$"sucursal{sufijo}"]
                };

                // Determinar auxiliares según si es combustible o canastilla
                string auxiliarContable;
                string auxiliarCruce;
                string formaPago = factura.FormaPago.ToString().ToUpper().Trim();
                
                if (esCombustible)
                {
                    // Para combustible, usar configuración específica por tipo de combustible
                    // Normalizar nombre de combustible para búsqueda en configuración
                    string combustibleKey = tipoCombustible.ToLower().Trim();
                    
                    // Intentar obtener auxiliares específicos para este combustible
                    auxiliarContable = ConfigurationManager.AppSettings[$"auxiliar{combustibleKey}"] 
                        ?? ConfigurationManager.AppSettings["auxiliarcombustible"];
                    
                    Logger.Info($"Combustible detectado: {tipoCombustible}, usando auxiliar: {auxiliarContable}");
                }
                else
                {
                    // Para canastilla, usar la configuración existente
                    auxiliarContable = ConfigurationManager.AppSettings[tieneIva ? "auxiliarcanastillaconiva" : "auxiliarcanastillasiniva"];
                }

                // Obtener cuenta de egreso (débito) - usar auxiliares dinámicos según forma de pago
                auxiliarCruce = ConfigurationManager.AppSettings["auxiliarefectivo"]; // Default
                
                if (formaPago.Contains("DATAFONO") || formaPago.Contains("TARJETA") || formaPago.Contains("DEBITO") || formaPago.Contains("CREDITO"))
                {
                    auxiliarCruce = ConfigurationManager.AppSettings["auxiliardatafono"];
                }
                else if (formaPago.Contains("TRANSFERENCIA") || formaPago.Contains("BANCARIA"))
                {
                    auxiliarCruce = ConfigurationManager.AppSettings["auxiliartransferencia"];
                }
                else
                {
                    auxiliarCruce = ConfigurationManager.AppSettings["auxiliarefectivo"];
                }

                // Determinar si es pago en efectivo
                bool esEfectivo = formaPago.Contains("EFECTIVO");

                object requestContent;

                if (esEfectivo)
                {
                    // Formato con Caja para pago en efectivo
                    var movimientosContables = new List<object>();

                    // Movimiento contable principal
                    movimientosContables.Add(new
                    {
                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"],
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                        F350_CONSEC_DOCTO = factura.NumeroFactura.ToString().Substring(4),
                        F351_BASE_GRAVABLE = "",
                        F351_NOTAS = $"Factura {(factura.EsCombustible ? "combustible " + factura.TipoCombustible : "canastilla")} {factura.NumeroFactura}",
                        F351_DOCTO_BANCO = "",
                        F351_ID_TERCERO = factura.Tercero.Identificacion.ToString(),
                        F351_ID_AUXILIAR = auxiliarContable,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"],
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"],
                        F351_ID_FE = factura.NumeroFactura.ToString().Substring(4),
                        F351_NRO_DOCTO_BANCO = "",
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"],
                        F351_VALOR_CR = ((decimal)factura.Subtotal).ToString("0.00", CultureInfo.InvariantCulture),
                        F351_VALOR_DB = "0"
                    });

                    // Movimiento IVA si aplica
                    if (tieneIva && (decimal)factura.Iva > 0)
                    {
                        movimientosContables.Add(new
                        {
                            F_CIA = "1",
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = factura.NumeroFactura.ToString().Substring(4),
                            F351_ID_AUXILIAR = "240801",
                            F351_ID_TERCERO = factura.Tercero.Identificacion.ToString(),
                            F351_ID_CO_MOV = factura.EsCombustible ? $"Venta combustible {factura.TipoCombustible}" : config.CentroOperacionesDocumento,
                            F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"],
                            F351_ID_CCOSTO = "",
                            F351_ID_FE = "",
                            F351_VALOR_DB = "0",
                            F351_VALOR_CR = ((decimal)factura.Iva).ToString("0.00", CultureInfo.InvariantCulture),
                            F351_BASE_GRAVABLE = ((decimal)factura.Subtotal).ToString("0.00", CultureInfo.InvariantCulture),
                            F351_DOCTO_BANCO = "",
                            F351_NRO_DOCTO_BANCO = "",
                            F351_NOTAS = $"IVA factura {(factura.EsCombustible ? "combustible" : "canastilla")} {factura.NumeroFactura}"
                        });
                    }

                    requestContent = new
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
                                F350_CONSEC_DOCTO = factura.NumeroFactura.ToString().Substring(4),
                                F351_NOTAS = factura.EsCombustible ? $"Venta combustible {factura.TipoCombustible}" : "Venta canastilla",
                                F351_ID_AUXILIAR = auxiliarCruce,
                                F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocaja"],
                                F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocaja"],
                                F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocaja"],
                                F351_VALOR_CR = "0",
                                F351_VALOR_DB = ((decimal)factura.Total).ToString("0.00", CultureInfo.InvariantCulture),
                                F351_ID_FE = ConfigurationManager.AppSettings["idfe"],
                                F358_COD_SEGURIDAD = "",
                                F358_FECHA_VCTO = ((DateTime)factura.Fecha).ToString("yyyyMMdd"),
                                F358_ID_CAJA = ConfigurationManager.AppSettings["caja"],
                                F358_ID_MEDIOS_PAGO = "EFE",
                                F358_NOTAS = $"Factura canastilla {factura.NumeroFactura}",
                                F358_NRO_AUTORIZACION = "",
                                F358_NRO_CUENTA = auxiliarCruce,
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
                                F350_CONSEC_DOCTO = factura.NumeroFactura.ToString().Substring(4),
                                F350_FECHA = ((DateTime)factura.Fecha).ToString("yyyyMMdd"),
                                F350_ID_TERCERO = factura.Tercero.Identificacion.ToString(),
                                F350_IND_ESTADO = "1",
                                F350_NOTAS = $"Factura canastilla {factura.NumeroFactura}"
                            }
                        },
                        Movimientocontable = movimientosContables
                    };
                }
                else
                {
                    // Formato normal para otros métodos de pago
                    var movimientosContables = new List<object>();

                    // Primer movimiento: INGRESO (CRÉDITO) - cuenta 42109003
                    movimientosContables.Add(new
                    {
                        F_CIA = "1",
                        F350_ID_CO = config.CentroOperacionesDocumento,
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                        F350_CONSEC_DOCTO = factura.NumeroFactura.ToString().Substring(4),
                        F351_ID_AUXILIAR = auxiliarContable,
                        F351_ID_TERCERO = factura.Tercero.Identificacion.ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocontableotros"],
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocontableotros"],
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocontableotros"],
                        F351_ID_FE = "",
                        F351_VALOR_DB = "0",
                        F351_VALOR_CR = ((decimal)factura.Subtotal).ToString("0.00", CultureInfo.InvariantCulture),
                        F351_BASE_GRAVABLE = "",
                        F351_DOCTO_BANCO = "",
                        F351_NRO_DOCTO_BANCO = "",
                        F351_NOTAS = $"Factura {(factura.EsCombustible ? "combustible " + factura.TipoCombustible : "canastilla")} {factura.NumeroFactura}"
                    });

                    // Segundo movimiento: EGRESO (DÉBITO)
                    movimientosContables.Add(new
                    {
                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesotros"],
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                        F350_CONSEC_DOCTO = factura.NumeroFactura.ToString().Substring(4),
                        F351_ID_AUXILIAR = auxiliarCruce,
                        F351_ID_TERCERO = "",
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientootros"],
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociootros"],
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostootros"],
                        F351_ID_FE = ConfigurationManager.AppSettings["idfeotros"],
                        F351_VALOR_DB = ((decimal)factura.Total).ToString("0.00", CultureInfo.InvariantCulture),
                        F351_VALOR_CR = "0",
                        F351_BASE_GRAVABLE = "",
                        F351_DOCTO_BANCO = "CG",
                        F351_NRO_DOCTO_BANCO = ((DateTime)factura.Fecha).ToString("yyyyMMdd"),
                        F351_NOTAS = $"Factura {(factura.EsCombustible ? "combustible " + factura.TipoCombustible : "canastilla")} {factura.NumeroFactura}"
                    });

                    // Tercer movimiento: IVA si aplica
                    if (tieneIva && (decimal)factura.Iva > 0)
                    {
                        movimientosContables.Add(new
                        {
                            F_CIA = "1",
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = factura.NumeroFactura.ToString().Substring(4),
                            F351_ID_AUXILIAR = "240801",
                            F351_ID_TERCERO = factura.Tercero.Identificacion.ToString(),
                            F351_ID_CO_MOV = config.CentroOperacionesDocumento,
                            F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"],
                            F351_ID_CCOSTO = "",
                            F351_ID_FE = "",
                            F351_VALOR_DB = "0",
                            F351_VALOR_CR = ((decimal)factura.Iva).ToString("0.00", CultureInfo.InvariantCulture),
                            F351_BASE_GRAVABLE = ((decimal)factura.Subtotal).ToString("0.00", CultureInfo.InvariantCulture),
                            F351_DOCTO_BANCO = "",
                            F351_NRO_DOCTO_BANCO = "",
                            F351_NOTAS = $"IVA factura {(factura.EsCombustible ? "combustible" : "canastilla")} {factura.NumeroFactura}"
                        });
                    }

                    requestContent = new
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
                                F350_CONSEC_DOCTO = factura.NumeroFactura.ToString().Substring(4),
                                F350_FECHA = ((DateTime)factura.Fecha).ToString("yyyyMMdd"),
                                F350_ID_TERCERO = factura.Tercero.Identificacion.ToString(),
                                F350_IND_ESTADO = "1",
                                F350_NOTAS = $"Factura {(factura.EsCombustible ? "combustible" : "canastilla")} {factura.NumeroFactura}"
                            }
                        },
                        Movimientocontable = movimientosContables
                    };
                }

                contentString = JsonConvert.SerializeObject(requestContent, Formatting.Indented);

                Logger.Info($"Enviando factura {factura.NumeroFactura} a Siesa. JSON:\n{contentString}");

                // Enviar a Siesa
                var urlSiesa = ConfigurationManager.AppSettings["urlsiesa"];
                var idSistema = ConfigurationManager.AppSettings["idsistema"];
                var idCompania = ConfigurationManager.AppSettings["idcompania"];
                var idDocumento = ConfigurationManager.AppSettings["iddocumento"];

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idSistema}&idDocumento={idDocumento}&nombreDocumento=Documento_Contablev2"
                    );

                    request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"]);
                    request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"]);

                    request.Content = new StringContent(contentString, null, "application/json");

                    var response = client.SendAsync(request).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        if (responseString.Contains("El documento ya existe"))
                        {
                            Logger.Info($"Factura {factura.NumeroFactura} ya existe en Siesa (marcada como exitosa)");
                            return true;
                        }
                        else
                        {
                            Logger.Error($"Error Bad Request enviando {factura.NumeroFactura}. Respuesta: {responseString}");
                            return false;
                        }
                    }

                    response.EnsureSuccessStatusCode();
                    Logger.Info($"Factura {factura.NumeroFactura} enviada exitosamente a Siesa. Respuesta: {responseString}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura {factura.NumeroFactura} a Siesa. JSON enviado:\n{contentString}");
                return false;
            }
        }

        static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString().Trim());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString().Trim());
            return result;
        }

        static void UpdateCsvLine(List<string> lines, int lineIndex, int columnIndex, string newValue)
        {
            if (lineIndex >= lines.Count) return;

            var columns = ParseCsvLine(lines[lineIndex]);
            if (columnIndex >= columns.Count) return;

            columns[columnIndex] = newValue;
            lines[lineIndex] = string.Join(",", columns.Select(c => c.Contains(",") ? $"\"{c}\"" : c));
        }

        static void SaveCsv(List<string> lines)
        {
            try
            {
                File.WriteAllLines(CSV_FILE_PATH, lines, Encoding.UTF8);
                Logger.Debug("CSV guardado exitosamente");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error guardando CSV");
            }
        }

        /// <summary>
        /// Obtiene un tercero de la base de datos por su identificación
        /// </summary>
        /// <param name="identificacion">NIT o identificación del tercero</param>
        /// <returns>Tercero si existe, null si no se encuentra</returns>
        static Tercero ObtenerTerceroPorIdentificacion(string identificacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    return null;
                }

                using (var connection = new SqlConnection(_connectionStringFacturacion))
                {
                    connection.Open();
                    
                    using (var command = new SqlCommand("ObtenerTercero", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@identificacion", identificacion.Trim());
                        
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            
                            if (dataTable.Rows.Count > 0)
                            {
                                var terceros = _convertidor.ConvertirTercero(dataTable);
                                return terceros?.FirstOrDefault();
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error obteniendo tercero con identificación {identificacion}");
                return null;
            }
        }
    }
}
