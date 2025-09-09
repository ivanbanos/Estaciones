using FactoradorEstacionesModelo;
using FactoradorEstacionesModelo.Fidelizacion;
using FacturadorEstacionesPOSWinForm;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
using Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using FactoradorEstacionesModelo.Siges;
using FactoradorEstacionesModelo.Objetos;
using SigesServicio;
using System.Runtime.CompilerServices;

namespace EnviadorInformacionService.Contabilidad
{
    public class SiesaWorker : BackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly InfoEstacion _infoEstacion;
        private readonly Siesa _siesa;
        private readonly InformacionCuenta _informacionCuenta;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly IFidelizacion _fidelizacon;


        public override void Dispose()
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        public SiesaWorker(IEstacionesRepositorio estacionesRepositorio, IOptions<InformacionCuenta> informacionCuenta, IOptions<InfoEstacion> infoEstacion, IOptions<Siesa> siesa, IConexionEstacionRemota conexionEstacionRemota, IFidelizacion fidelizacon)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion.Value;
            _informacionCuenta = informacionCuenta.Value;
            _conexionEstacionRemota = conexionEstacionRemota;
            _fidelizacon = fidelizacon;
            _siesa = siesa.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                Logger.Info("Iniciando interfaz Siesa");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {

                        var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadasSiesa();

                        var terceros = facturas.Select(x => x.Tercero).GroupBy(t => t.terceroId).Select(g => g.First()).ToList();

                        if (terceros.Any(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                        {
                            var tercerosEnviados = new List<int>();
                            var tercerosFallidos = new List<string>();

                            foreach (var t in terceros.Where(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                            {
                                if (await EnviarTercero(t))
                                {
                                    tercerosEnviados.Add(t.terceroId);
                                    Logger.Info($"Tercero enviado exitosamente - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                }
                                else
                                {
                                    tercerosFallidos.Add($"ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                    Logger.Info($"Fallo al enviar tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                }
                            }

                            if (tercerosEnviados.Any())
                            {
                                _estacionesRepositorio.MarcarTercerosEnviadosASiesa(tercerosEnviados);
                                Logger.Info($"Total terceros enviados exitosamente: {tercerosEnviados.Count} - IDs: {string.Join(", ", tercerosEnviados)}");
                            }

                            if (tercerosFallidos.Any())
                            {
                                Logger.Info($"Total terceros que fallaron al enviar: {tercerosFallidos.Count} - {string.Join(" | ", tercerosFallidos)}");
                            }
                        }
                        var facturasEnviadas = new List<int>();
                        var facturasFallidas = new List<string>();

                        foreach (var factura in facturas)
                        {
                            try
                            {
                                var infoTemp = "";
                                var facelec = "";

                                if (factura.codigoFormaPago != 6)
                                {
                                    try
                                    {
                                        var intentos = 0;
                                        do
                                        {
                                            try
                                            {
                                                infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(factura.ventaId, Guid.Parse(_infoEstacion.EstacionFuente), _conexionEstacionRemota.getToken());
                                            }
                                            catch (Exception ex)
                                            {
                                                infoTemp = "";
                                                Logger.Error($"Error al obtener información de la factura electrónica para la factura {factura.ventaId}: {ex.Message}");
                                            }
                                            Thread.Sleep(100);
                                        } while (infoTemp == null || intentos++ < 3);

                                        if (!string.IsNullOrEmpty(infoTemp))
                                        {
                                            infoTemp = infoTemp.Replace("\n\r", " ");

                                            var facturaElectronica = infoTemp.Split(' ');

                                            Match match = Regex.Match(facturaElectronica[2], @"^([A-Za-z]+)(\d+)$");
                                            facelec = facturaElectronica[4];
                                            if (match.Success)
                                            {
                                                string letras = match.Groups[1].Value;
                                                string numeros = match.Groups[2].Value;

                                                string auxiliarContable = _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Combustible, true, true).Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                                                string auxiliarCruce = _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Combustible, true, false).Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                                                if (auxiliarContable == null)
                                                {

                                                    Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Combustible} no se envió no exite auxiliar contrable creado");
                                                }
                                                if (auxiliarCruce == null)
                                                {

                                                    Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Combustible} no se envió no exite auxiliar cruce creado");
                                                }
                                                await EnviarFactura(factura, facturaElectronica[2], numeros, auxiliarContable, auxiliarCruce);
                                                //_apiContabilidad.EnviarRecibo(factura, facturaElectronica[2], numeros, _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, true), _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, false));
                                                facturasEnviadas.Add(factura.ventaId);
                                                Logger.Info($"Factura enviada exitosamente - ID: {factura.ventaId}, Total: {factura.Total}, Forma Pago: {factura.codigoFormaPago}, Combustible: {factura.Combustible}");
                                            }
                                        }
                                        else
                                        {
                                            facturasEnviadas.Add(factura.ventaId);

                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Total}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                        Logger.Warn($"Fallo al enviar factura - ID: {factura.ventaId}, Total: {factura.Total}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                    }
                                }
                                else
                                {
                                    facturasEnviadas.Add(factura.ventaId);
                                }
                            }
                            catch (Exception ex)
                            {
                                facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Total}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                Logger.Warn($"Fallo al procesar factura - ID: {factura.ventaId}, Total: {factura.Total}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                            }
                        }
                        if (facturasEnviadas.Any())
                        {
                            Logger.Info($"Total facturas enviadas exitosamente: {facturasEnviadas.Count} - IDs: {string.Join(", ", facturasEnviadas)}");
                            _estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(facturasEnviadas);
                        }

                        if (facturasFallidas.Any())
                        {
                            Logger.Warn($"Total facturas que fallaron al enviar: {facturasFallidas.Count} - {string.Join(" | ", facturasFallidas)}");
                        }

                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {

                        Logger.Info("Ex" + ex.Message);
                        Thread.Sleep(5000);
                    }
                }
            });
        }

        private async Task EnviarFactura(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce)
        {
            var contentString = "";
            var responseString = "";

            if (factura.codigoFormaPago == 1)
            {
                // Pago en efectivo - usar formato con Caja
                var requestContent = ConvertirAMovimientoSiesaCaja(factura, facturaelectronica, consecutivo, auxiliarContable, auxiliarCruce);
                contentString = JsonConvert.SerializeObject(requestContent);
            }
            else
            {
                // Otros métodos de pago - usar formato sin Caja
                var requestContent = ConvertirAMovimientoSiesa(factura, facturaelectronica, consecutivo, auxiliarContable, auxiliarCruce);
                contentString = JsonConvert.SerializeObject(requestContent);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_siesa.UrlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={_siesa.IdCompania}&idSistema={_siesa.Idsistema}&idDocumento={_siesa.IdDocumento}&nombreDocumento=Documento_Contablev2");
                    request.Headers.Add("ConniKey", _siesa.KeySiesa);
                    request.Headers.Add("ConniToken", _siesa.Tokensiesa);
                    request.Content = new StringContent(contentString, Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);
                    responseString = await response.Content.ReadAsStringAsync();

                    // Si es Bad Request, verificar si el documento ya existe
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        if (responseString.Contains("El documento ya existe"))
                        {
                            Logger.Info($"Factura ya existe en Siesa (marcada como exitosa) - {contentString}. Respuesta: {responseString}");
                            return; // Salir sin lanzar excepción, se considera exitosa
                        }
                        else
                        {
                            Logger.Warn($"Factura no enviada (Bad Request) - {contentString}. Respuesta: {responseString}");
                            throw new HttpRequestException($"Bad Request: {responseString}");
                        }
                    }

                    response.EnsureSuccessStatusCode();
                    Logger.Info($"Factura enviada {contentString}. Respuesta {responseString}");
                }
            }
            catch (Exception ex)
            {
                if (responseString.Contains("El documento ya existe"))
                {
                    Logger.Info($"Factura ya existe en Siesa (marcada como exitosa) - {contentString}. Respuesta: {responseString}");
                    return; // Salir sin lanzar excepción, se considera exitosa
                }
                else
                {
                    Logger.Info($"Factura no enviada {contentString}. Respuesta {responseString}. Error: {ex.Message}");
                    throw;
                }
            }
        }

        private object ConvertirAMovimientoSiesa(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce)
        {
            var movimientos = new List<object>();
            // Primer movimiento contable
            movimientos.Add(new
            {
                F_CIA = "1",
                F350_ID_CO = _siesa.CentroOperacionesContableOtros ?? "",
                F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                F350_CONSEC_DOCTO = consecutivo,
                F351_ID_AUXILIAR = auxiliarContable ?? "",
                F351_ID_TERCERO = factura.Tercero.identificacion?.ToString() ?? "",
                F351_ID_CO_MOV = _siesa.MovimientoContableOtros ?? "",
                F351_ID_UN = _siesa.UnidadNegocioContableOtros ?? "",
                F351_ID_CCOSTO = _siesa.CentroCostoContableOtros ?? "",
                F351_ID_FE = "",
                F351_VALOR_DB = "0",
                F351_VALOR_CR = factura.Total.ToString("0", CultureInfo.InvariantCulture),
                F351_BASE_GRAVABLE = "",
                F351_DOCTO_BANCO = "",
                F351_NRO_DOCTO_BANCO = "",
                F351_NOTAS = $"Factura combustible {factura.Combustible.Trim()} id local {consecutivo}"
            });
            // Segundo movimiento contable
            movimientos.Add(new
            {
                F_CIA = "1",
                F350_ID_CO = _siesa.CentroOperacionesOtros ?? "",
                F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                F350_CONSEC_DOCTO = consecutivo,
                F351_ID_AUXILIAR = auxiliarCruce ?? "",
                F351_ID_TERCERO = "",
                F351_ID_CO_MOV = _siesa.MovimientoOtros ?? "",
                F351_ID_UN = _siesa.UnidadNegocioOtros ?? "",
                F351_ID_CCOSTO = _siesa.CentroCostoOtros ?? "",
                F351_ID_FE = _siesa.IdFeOtros ?? "",
                F351_VALOR_DB = factura.Total.ToString("0", CultureInfo.InvariantCulture),
                F351_VALOR_CR = "0",
                F351_BASE_GRAVABLE = "",
                F351_DOCTO_BANCO = "CG",
                F351_NRO_DOCTO_BANCO = factura.fecha.ToString("yyyyMMdd"),
                F351_NOTAS = $"Factura combustible {factura.Combustible.Trim()} id local {consecutivo}"
            });
            // Movimiento de descuento si aplica
            if (factura.Descuento != null && factura.Descuento > 0)
            {
                movimientos.Add(new
                {
                    F_CIA = "1",
                    F350_ID_CO = _siesa.CentroOperacionesContableDescuento ?? "101",
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = _siesa.AuxiliarDescuento ?? "58904001",
                    F351_ID_TERCERO = factura.Tercero.identificacion?.ToString() ?? "",
                    F351_ID_CO_MOV = _siesa.MovimientoContableDescuento ?? "101",
                    F351_ID_UN = _siesa.UnidadNegocioDescuento ?? "03",
                    F351_ID_CCOSTO = _siesa.CentroCostoDescuento ?? "0203",
                    F351_ID_FE = _siesa.IdFeDescuento ?? "1",
                    F351_VALOR_DB = factura.Descuento.ToString("0", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "",
                    F351_BASE_GRAVABLE = "1",
                    F351_DOCTO_BANCO = "",
                    F351_NRO_DOCTO_BANCO = "",
                    F351_NOTAS = $"FAC {consecutivo} DESCUENTO PROMOCIÓN",
                    F351_ID_SUCURSAL = _siesa.Sucursal ?? "001"
                });
            }
            var requestContent = new
            {
                Inicial = new List<object> { new { F_CIA = "1" } },
                Documentocontable = new List<object> { new {
                    F_CIA = "1",
                    F_CONSEC_AUTO_REG = _siesa.ConsecutivoAutoRegulado,
                    F350_ID_CO = _siesa.CentroOperacionesDocumento,
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                    F350_ID_TERCERO = factura.Tercero.identificacion?.ToString() ?? "",
                    F350_IND_ESTADO = "1",
                    F350_NOTAS = $"Factura combustible {factura.Combustible.Trim()} id local {consecutivo}",
                }},
                Movimientocontable = movimientos,
                Final = new List<object> { new { F_CIA = "1" } }
            };
            return requestContent;
        }

        private MovimientosCaja ConvertirAMovimientoSiesaCaja(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce)
        {
            var requestContent = new MovimientosCaja()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                Caja = new List<Caja> {
                new Caja {

                        F_CIA = "1",
                        F350_ID_CO = _siesa.CentroOperacionesCaja,
                        F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_NOTAS = "Venta combustible",
                        F351_ID_AUXILIAR = auxiliarCruce ?? "",
                        F351_ID_CCOSTO = _siesa.CentroCostoCaja,
                        F351_ID_CO_MOV = _siesa.MovimientoCaja,
                        F351_ID_UN = _siesa.UnidadNegocioCaja,
                        F351_VALOR_CR = "0",
                        F351_VALOR_DB = factura.Total.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_ID_FE = _siesa.IdFe,
                        F358_COD_SEGURIDAD = "",
                        F358_FECHA_VCTO = factura.fecha.ToString("yyyyMMdd"),
                        F358_ID_CAJA = _siesa.Caja,
                        F358_ID_MEDIOS_PAGO = "EFE",
                        F358_NOTAS = $"Factura combustible {factura.Combustible.Trim()} id local {consecutivo}",
                        F358_NRO_AUTORIZACION="",
                        F358_NRO_CUENTA=auxiliarCruce ?? "",
                        F358_REFERENCIA_OTROS=""



                }
                },
                Documentocontable = new List<Documentocontable> { new Documentocontable() {
                F_CIA = "1",
                F_CONSEC_AUTO_REG = _siesa.ConsecutivoAutoRegulado,
                F350_ID_CO = _siesa.CentroOperacionesDocumento,
                F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                F350_CONSEC_DOCTO = consecutivo,
                F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                F350_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F350_IND_ESTADO = "1",
                F350_NOTAS = $"Factura combustible {factura.Combustible.Trim()} id local {consecutivo}",


                }
               },
                Movimientocontable = new List<Movimientocontable>()
                {
                    new Movimientocontable()
                    {
                        F_CIA = "1",
                        F350_ID_CO = _siesa.CentroOperaciones,
                        F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_BASE_GRAVABLE = "",
                        F351_NOTAS = $"Factura combustible {factura.Combustible.Trim()} id local {consecutivo}",
                        F351_DOCTO_BANCO = "",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = auxiliarContable ?? "",
                        F351_ID_CCOSTO = _siesa.CentroCosto,
                        F351_ID_CO_MOV = _siesa.Movimiento,
                        F351_ID_FE = consecutivo,
                        F351_NRO_DOCTO_BANCO="",
                        F351_ID_UN = _siesa.UnidadNegocio,
                        F351_VALOR_CR = factura.Total.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_VALOR_DB = "0",



                    }

                },

            };
            return requestContent;
        }

        private async Task<bool> EnviarTercero(Tercero t)
        {
            var requestContent = ConvertirATercero(t);
            var responseString = "";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_siesa.UrlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={_siesa.IdCompania}&idSistema={_siesa.Idsistema}&idDocumento={_siesa.IdDOcumentoCliente}&nombreDocumento=TERCERO_CLIENTE_INTEGRADO");
                    request.Headers.Add("ConniKey", _siesa.KeySiesa);
                    request.Headers.Add("ConniToken", _siesa.Tokensiesa);
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);
                    responseString = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();

                    Logger.Info($"Tercero enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                    return true;
                }
            }
            catch (Exception ex)
            {

                if (ex is HttpRequestException && responseString.Contains("No tiene acceso a modificar"))
                {
                    Logger.Info($"Tercero enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                    return true;
                }

                Logger.Info($"Tercero no enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                return false;
            }
        }

        private Root ConvertirATercero(Tercero x)
        {
            var nombre = "";
            var apellido = "";
            var nombreCompleto = x?.Nombre?.Trim() ?? "";
            if (nombreCompleto.Split(' ').Count() > 1)
            {
                nombre = nombreCompleto.Substring(0, nombreCompleto.LastIndexOf(" "));
                apellido = nombreCompleto.Split(' ').Last();
            }
            else
            {
                nombre = nombreCompleto;
                apellido = "no informado";
            }


            return new Root()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                Imptos_Reten = new List<ImptosReten>
                {
                    new ImptosReten
                    {
                        F_TIPO_REG = "46",
                        F_CIA = "1",
                        F_ID_TERCERO= x.identificacion?.Trim() ?? "",
                        F_ID_SUCURSAL = _siesa.Sucursal,
                        F_ID_CLASE = "1",
                        F_ID_VALOR_TERCERO = "1"
                    },

                    new ImptosReten
                    {
                        F_TIPO_REG = "46",
                        F_CIA = "1",
                        F_ID_TERCERO= x.identificacion?.Trim() ?? "",
                        F_ID_SUCURSAL = _siesa.Sucursal,
                        F_ID_CLASE = "2",
                        F_ID_VALOR_TERCERO = "1"
                    }
                },

                Clientes = new List<ClienteSiesa> {
                    new ClienteSiesa
                    {
                        F_CIA = "1",
                        F201_ID_TERCERO = x.identificacion?.Trim() ?? "",
                        F201_ID_SUCURSAL =_siesa.Sucursal,
                        F201_DESCRIPCION_SUCURSAL = "YAVEGAS",
                        F201_ID_VENDEDOR = "",
                        F201_ID_COND_PAGO = "001",
                        F201_CUPO_CREDITO = "",
                        F201_ID_TIPO_CLI = "0004",
                        F201_ID_LISTA_PRECIO ="",
                        F201_IND_BLOQUEADO = "1",
                        F201_IND_BLOQUEO_CUPO = "0",
                        F201_IND_BLOQUEO_MORA = "0",
                        F201_ID_CO_FACTURA = "001",
                        F015_CONTACTO = "SIGES",
                        F015_DIRECCION1 = "1",
                        F015_DIRECCION2 = "1",
                        F015_DIRECCION3 = "1",
                        F015_ID_PAIS = "169",
                        F015_ID_DEPTO="05",
                        F015_ID_CIUDAD = "001",
                         F015_TELEFONO = x.Telefono.Trim().Length > 20 ? x.Telefono.Substring(0, 20) : x.Telefono.Trim(),
                    F015_EMAIL = x.Correo,
                    F201_FECHA_INGRESO = DateTime.Now.ToString("yyyyMMdd"),
                    F201_ID_CO_MOVTO_FACTURA = "",
                    F201_ID_UN_MOVTO_FACTURA = "",
                    f015_celular = "1"
                    }
                },

                Terceros = new List<TerceroSiesa> {
                    new TerceroSiesa
                    {
                        F_CIA = "1",
                        F200_ID = x.identificacion?.Trim() ?? "",
                        F200_NIT = x.identificacion?.Trim() ?? "",
                    F200_ID_TIPO_IDENT = x.tipoIdentificacionS == "Nit" ? "N" : "C",
                    F200_IND_TIPO_TERCERO = x.tipoIdentificacionS == "Nit" ? "2" :"1",
                    F200_RAZON_SOCIAL = nombreCompleto.Length > 40 ? nombreCompleto.Substring(0, 40) : nombreCompleto,
                    F200_APELLIDO1 = apellido,
                    F200_APELLIDO2 = "NA",
                    F200_NOMBRES = nombre,
                    F200_NOMBRE_EST = nombre,
                    F015_CONTACTO = "SIGES",
                        F015_DIRECCION1 = x.Direccion.Trim().Length > 40 ? x.Direccion.Substring(0, 40) : x.Direccion.Trim(),
                        F015_DIRECCION2 = "",
                        F015_DIRECCION3 = "",
                    F015_ID_PAIS = "169",
                    F015_ID_DEPTO = "05",
                    F015_ID_CIUDAD = "001",
                    F015_TELEFONO = x.Telefono.Trim().Length > 20 ? x.Telefono.Substring(0, 20) : x.Telefono.Trim(),
                    F015_EMAIL = x.Correo,
                    F200_FECHA_NACIMIENTO = "20000101",
                    F200_ID_CIIU = "0010",
                    F015_CELULAR = x.Telefono.Trim().Length > 20 ? x.Telefono.Substring(0, 20) : x.Telefono.Trim()
                }
                }
            };
        }
    }
}