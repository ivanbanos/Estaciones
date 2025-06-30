using FactoradorEstacionesModelo.Objetos;
using Newtonsoft.Json;
using OfficeOpenXml.Drawing.Slicer.Style;
using OfficeOpenXml.Drawing.Vml;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Contabilidad
{
    public class ApiSiesa
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string urlSiesa = ConfigurationManager.AppSettings["urlsiesa"].ToString();
        private readonly string idsistema = ConfigurationManager.AppSettings["idsistema"].ToString();
        private readonly string idCompania = ConfigurationManager.AppSettings["idcompania"].ToString();
        private readonly string idDocumento = ConfigurationManager.AppSettings["iddocumento"].ToString();
        private readonly string idDocumentoCliente = ConfigurationManager.AppSettings["iddocumentocliente"].ToString();

private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                (result, timeSpan, retryCount, context) =>
                {
                    Logger.Warn($"Retry {retryCount} for {context.PolicyKey} at {context.OperationKey}, due to: {result.Exception?.Message ?? result.Result.ReasonPhrase}");
                });

        internal void EnviarRecibo(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce)
        {
            var requestContent = ConvertirAReciboSiesa(factura, facturaelectronica, consecutivo, auxiliarContable, cruce);
            var responseString = "";
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idsistema}&idDocumento={idDocumento}&nombreDocumento=Documento_Contablev2");
                request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"].ToString());
                request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"].ToString());
                var content = new StringContent(JsonConvert.SerializeObject(requestContent), null, "application/json");
                request.Content = content;
                 var response = retryPolicy.ExecuteAsync(async () => await client.SendAsync(request)).Result;
                responseString = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();

                Logger.Info($"Recibo enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
            }

            catch (Exception ex)
            {

                if(!responseString.Contains("El documento ya existe"))
                {

                    Logger.Info($"Recibo no enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                    throw;
                }
                else
                {

                    Logger.Info($"Recibo enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");

                }
            }
        }

        private object ConvertirAReciboSiesa(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce)
        {
            var requestContent = new Movimientos()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                MovimientoCxC = new List<MovimientoCxC> {
                new MovimientoCxC {

                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentorecibo"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = cruce,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"].ToString(),
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"].ToString(),
                        F351_VALOR_CR = "0",
                        F351_VALOR_DB = factura.Venta.TOTAL.ToString("0.00", CultureInfo.InvariantCulture),
                        F353_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
                        F353_CONSEC_DOCTO_CRUCE= ConfigurationManager.AppSettings["documentocruce"].ToString(),
                        F353_ID_TIPO_DOCTO_CRUCE=ConfigurationManager.AppSettings["documentorecibo"].ToString(),
                        F353_NRO_CUOTA_CRUCE="11",
                        F353_FECHA_DSCTO_PP=factura.fecha.ToString("yyyyMMdd"),
                        F353_FECHA_VCTO=factura.fecha.ToString("yyyyMMdd"),
                        F354_NOTAS=$"{ConfigurationManager.AppSettings["documentofactura"].ToString()} {factura.ventaId}",
                        F354_TERCERO_VEND= ConfigurationManager.AppSettings["vendedor"].ToString(),

                }
                },
                Documentocontable = new List<Documentocontable> { new Documentocontable() {
                F_CIA = "1",
                F_CONSEC_AUTO_REG = "1",
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentorecibo"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                F350_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F350_IND_ESTADO = "1",
                F350_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}"


                }
               },
                Movimientocontable = new List<Movimientocontable>()
                {
                    new Movimientocontable()
                    {
                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["movimiento"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentorecibo"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_BASE_GRAVABLE = "",
                        F351_NOTAS = $"Recibo combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                        F351_DOCTO_BANCO = "",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = auxiliarContable,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"].ToString(),
                        F351_ID_FE = ConfigurationManager.AppSettings["idfe"].ToString(),
                        F351_NRO_DOCTO_BANCO="",
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"].ToString(),
                        F351_VALOR_CR = factura.Venta.TOTAL.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_VALOR_DB = "0",



                    }

                },

            };
            return requestContent;
        }

        internal void EnviarFactura(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce)
        {
            var contentString = "";
            var responseString = "";
            if (factura.codigoFormaPago == 4)
            {
                var requestContent = ConvertirAMovimientoSiesaCaja(factura, facturaelectronica, consecutivo, auxiliarContable, cruce);
                contentString = JsonConvert.SerializeObject(requestContent);
            }
            else
            {

                var requestContent = ConvertirAMovimientoSiesaCaja(factura, facturaelectronica, consecutivo, auxiliarContable, cruce);
                contentString = JsonConvert.SerializeObject(requestContent);
            }

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idsistema}&idDocumento={idDocumento}&nombreDocumento=Documento_Contablev2");
                request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"].ToString());
                request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"].ToString());

                var content = new StringContent(contentString, null, "application/json");
                request.Content = content;
                var response = retryPolicy.ExecuteAsync(async () => await client.SendAsync(request)).Result;
                response.EnsureSuccessStatusCode();

                Logger.Info($"Factura enviada {contentString}. Respuesta {responseString}");
            }

            catch (Exception ex)
            {
                if (!responseString.Contains("El documento ya existe"))
                {

                    Logger.Info($"Factura no enviado {contentString}. Respuesta {responseString}");
                    throw;
                }
                else
                {

                    Logger.Info($"factura enviado {contentString}. Respuesta {responseString}");

                }
            }
        }

        private Movimientos ConvertirAMovimientoSiesa(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce)
        {
            var requestContent = new Movimientos()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                MovimientoCxC = new List<MovimientoCxC> {
                new MovimientoCxC {

                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescxc"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_NOTAS = "Venta combustible",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = cruce,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocxc"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocxc"].ToString(),
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocxc"].ToString(),
                        F351_VALOR_CR = "0",
                        F351_VALOR_DB = factura.Venta.TOTAL.ToString("0.00", CultureInfo.InvariantCulture),
                        F353_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
                        F353_CONSEC_DOCTO_CRUCE= ConfigurationManager.AppSettings["documentocruce"].ToString(),
                        F353_ID_TIPO_DOCTO_CRUCE=ConfigurationManager.AppSettings["documentofactura"].ToString(),
                        F353_NRO_CUOTA_CRUCE="11",
                        F353_FECHA_DSCTO_PP=factura.fecha.ToString("yyyyMMdd"),
                        F353_FECHA_VCTO=factura.fecha.ToString("yyyyMMdd"),
                        F354_NOTAS=$"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                        F354_TERCERO_VEND=ConfigurationManager.AppSettings["vendedor"].ToString(),

                }
                },
                Documentocontable = new List<Documentocontable> { new Documentocontable() {
                F_CIA = "1",
                F_CONSEC_AUTO_REG = ConfigurationManager.AppSettings["consecutivoautoregulado"].ToString(),
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesdocuemnto"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                F350_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F350_IND_ESTADO = "1",
                F350_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",


                }
               },
                Movimientocontable = new List<Movimientocontable>()
                {
                    new Movimientocontable()
                    {
                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_BASE_GRAVABLE = "",
                        F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                        F351_DOCTO_BANCO = "",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = auxiliarContable,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"].ToString(),
                        F351_ID_FE = ConfigurationManager.AppSettings["idfe"].ToString(),
                        F351_NRO_DOCTO_BANCO="",
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"].ToString(),
                        F351_VALOR_CR = factura.Venta.TOTAL.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_VALOR_DB = "0",



                    }

                },

            };
            return requestContent;
        }

        private MovimientosCaja ConvertirAMovimientoSiesaCaja(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce)
        {
            var requestContent = new MovimientosCaja()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                Caja = new List<Caja> {
                new Caja {

                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescaja"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_NOTAS = "Venta combustible",
                        F351_ID_AUXILIAR = cruce,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocaja"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocaja"].ToString(),
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocaja"].ToString(),
                        F351_VALOR_CR = "0",
                        F351_VALOR_DB = factura.Venta.TOTAL.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_ID_FE =ConfigurationManager.AppSettings["idfe"].ToString(),
                        F358_COD_SEGURIDAD = "",
                        F358_FECHA_VCTO = factura.fecha.ToString("yyyyMMdd"),
                        F358_ID_CAJA = ConfigurationManager.AppSettings["caja"].ToString(),
                        F358_ID_MEDIOS_PAGO = "EFE",
                        F358_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {consecutivo}",
                        F358_NRO_AUTORIZACION="",
                        F358_NRO_CUENTA=cruce,
                        F358_REFERENCIA_OTROS=""
                        
                        

                }
                },
                Documentocontable = new List<Documentocontable> { new Documentocontable() {
                F_CIA = "1",
                F_CONSEC_AUTO_REG = ConfigurationManager.AppSettings["consecutivoautoregulado"].ToString(),
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesdocuemnto"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                F350_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F350_IND_ESTADO = "1",
                F350_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {consecutivo}",


                }
               },
                Movimientocontable = new List<Movimientocontable>()
                {
                    new Movimientocontable()
                    {
                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_BASE_GRAVABLE = "",
                        F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                        F351_DOCTO_BANCO = "",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = auxiliarContable,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"].ToString(),
                        F351_ID_FE = consecutivo,
                        F351_NRO_DOCTO_BANCO="",
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"].ToString(),
                        F351_VALOR_CR = factura.Venta.TOTAL.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_VALOR_DB = "0",



                    }

                },

            };
            return requestContent;
        }

        public void EnviarTercero(Tercero tercero)
        {
            var requestContent = ConvertirATercerosSiesa(tercero);
            var responseString = "";

            try
            {


                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idsistema}&idDocumento={idDocumentoCliente}&nombreDocumento=TERCERO_CLIENTE_INTEGRADO");
                request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"].ToString());
                request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"].ToString());
                var content = new StringContent(JsonConvert.SerializeObject(requestContent), null, "application/json");
                request.Content = content;
                var response = retryPolicy.ExecuteAsync(async () => await client.SendAsync(request)).Result;
                response.EnsureSuccessStatusCode();

                Logger.Info($"Tercero enviado {JsonConvert.SerializeObject(requestContent)}.Respuesta { responseString}");
            }
            catch (Exception ex)
            {

                Logger.Info($"Tercero no enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                throw;
            }
        }

        private Root ConvertirATercerosSiesa(Tercero x)
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
                        F_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
                        F_ID_CLASE = "1",
                        F_ID_VALOR_TERCERO = "1"
                    },

                    new ImptosReten
                    {
                        F_TIPO_REG = "46",
                        F_CIA = "1",
                        F_ID_TERCERO= x.identificacion.Trim(),
                        F_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
                        F_ID_CLASE = "2",
                        F_ID_VALOR_TERCERO = "1"
                    }
                },

                Clientes = new List<ClienteSiesa> {
                    new ClienteSiesa
                    {
                        F_CIA = "1",
                        F201_ID_TERCERO = x.identificacion.Trim(), 
                        F201_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
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
