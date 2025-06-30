using Polly;
using Polly.Retry;
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

        private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy;

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
            retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (result, timeSpan, retryCount, context) =>
                    {
                        Logger.Warn($"Retry {retryCount} for {context.PolicyKey} at {context.OperationKey}, due to: {result.Exception?.Message ?? result.Result.ReasonPhrase}");
                    });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            Logger.Info("Iniciando interfaz Siesa");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadasSiesa();

                    var terceros = facturas.Select(x => x.Tercero);
                    if (terceros.Any(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                    {
                        foreach (var t in terceros.Where(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                        {

                            EnviarTercero(t);
                            _estacionesRepositorio.MarcarTercerosEnviadosASiesa(terceros.Select(x => x.terceroId));
                        }
                    }
                    var facturasEnviadas = new List<int>();
                    foreach (var factura in facturas)
                    {
                        try
                        {
                            var infoTemp = "";
                            var facelec = "";
                            var consecutivo = "";

                            if (factura.codigoFormaPago != 6)
                            {
                                try
                                {
                                    var intentos = 0;
                                    do
                                    {
                                        infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(factura.ventaId, Guid.Parse(_infoEstacion.EstacionFuente), _conexionEstacionRemota.getToken());
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
                                            EnviarFactura(factura, facturaElectronica[2], numeros, auxiliarContable, auxiliarCruce);
                                            //_apiContabilidad.EnviarRecibo(factura, facturaElectronica[2], numeros, _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, true), _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, false));
                                            facturasEnviadas.Add(factura.ventaId);
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {

                                    Logger.Info($"Factura {JsonConvert.SerializeObject(factura)} no se envió {ex.Message}");
                                }
                            }
                            else
                            {

                                facturasEnviadas.Add(factura.ventaId);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Info($"Factura {JsonConvert.SerializeObject(factura)} no se envió {ex.Message}");
                        }
                    }
                    if (facturasEnviadas.Any())
                    {
                        Logger.Info($"Facturas {JsonConvert.SerializeObject(facturasEnviadas)} enviadas");
                        _estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(facturasEnviadas);
                    }
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {

                    Logger.Info("Ex" + ex.Message);
                    Thread.Sleep(300000);
                }
            }
        });

        private async Task  EnviarFactura(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce)
        {
            var requestContent = ConvertirAReciboSiesa(factura, facturaelectronica, consecutivo, auxiliarContable, auxiliarCruce);
            var responseString = "";

            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_siesa.UrlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={_siesa.IdCompania}&idSistema={_siesa.Idsistema}&idDocumento={_siesa.IdDocumento}&nombreDocumento=Documento_Contablev2");
                    request.Headers.Add("ConniKey", _siesa.KeySiesa);
                    request.Headers.Add("ConniToken", _siesa.Tokensiesa);
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");

                    var response = await retryPolicy.ExecuteAsync(async () => await client.SendAsync(request));
                    responseString = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();

                    Logger.Info($"Recibo enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                }
            }
            catch (Exception ex)
            {
                if (!responseString.Contains("El documento ya existe"))
                {
                    Logger.Info($"Recibo no enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                    throw;
                }
            }
        }

        private MovimientosCaja ConvertirAReciboSiesa(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce)
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
                        F351_ID_AUXILIAR = auxiliarCruce,
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
                        F358_NRO_CUENTA=auxiliarCruce,
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
                        F351_NOTAS = $"Factura combustible {factura.Combustible.Trim()} id local {factura.ventaId}",
                        F351_DOCTO_BANCO = "",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = auxiliarContable,
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

        private async Task EnviarTercero(Tercero t)
        {
            var requestContent = ConvertirATercero(t);
            var responseString = "";

            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_siesa.UrlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={_siesa.IdCompania}&idSistema={_siesa.Idsistema}&idDocumento={_siesa.IdDocumento}&nombreDocumento=Documento_Contablev2");
                    request.Headers.Add("ConniKey", _siesa.KeySiesa);
                    request.Headers.Add("ConniToken", _siesa.Tokensiesa);
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");

                    var response = await retryPolicy.ExecuteAsync(async () => await client.SendAsync(request));
                    responseString = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();

                    Logger.Info($"Recibo enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                }
            }
            catch (Exception ex)
            {
                if (!responseString.Contains("El documento ya existe"))
                {
                    Logger.Info($"Recibo no enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                    throw;
                }
            }
        }

        private Root ConvertirATercero(Tercero x)
        {
            var nombre = "";
            var apellido = "";
            var nombreCompleto = x?.Nombre?.Trim();
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
                        F_ID_TERCERO= x.identificacion.Trim(),
                        F_ID_SUCURSAL = _siesa.Sucursal,
                        F_ID_CLASE = "1",
                        F_ID_VALOR_TERCERO = "1"
                    },

                    new ImptosReten
                    {
                        F_TIPO_REG = "46",
                        F_CIA = "1",
                        F_ID_TERCERO= x.identificacion.Trim(),
                        F_ID_SUCURSAL = _siesa.Sucursal,
                        F_ID_CLASE = "2",
                        F_ID_VALOR_TERCERO = "1"
                    }
                },

                Clientes = new List<ClienteSiesa> {
                    new ClienteSiesa
                    {
                        F_CIA = "1",
                        F201_ID_TERCERO = x.identificacion.Trim(),
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
                        F200_ID = x.identificacion.Trim(),
                        F200_NIT = x.identificacion.Trim(),
                    F200_ID_TIPO_IDENT = x.tipoIdentificacionS == "Nit" ? "N" : "C",
                    F200_IND_TIPO_TERCERO = "1",
                    F200_RAZON_SOCIAL = nombreCompleto,
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