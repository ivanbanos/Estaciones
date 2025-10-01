using EnviadorInformacionService.Contabilidad;
using EnviadorInformacionService.Models;
using FacturadorEstacionesRepositorio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public class ProtocoloSiesa
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConexionEstacionRemota _conexionEstacionRemota = new ConexionEstacionRemota();
        public void Ejecutar()
        {

            var _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            var _apiContabilidad = new ApiSiesa();

            var estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            Logger.Info("Iniciando interfaz Siesa");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
            while (true)
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
                            Logger.Info($"Iniciando envío de tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            if (_apiContabilidad.EnviarTercero(t))
                            {
                                tercerosEnviados.Add(t.terceroId);
                                Logger.Info($"Tercero enviado exitosamente - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            }
                            else
                            {
                                tercerosFallidos.Add($"ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                Logger.Warn($"Fallo al enviar tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            }
                        }

                        if (tercerosEnviados.Any())
                        {
                            _estacionesRepositorio.MarcarTercerosEnviadosASiesa(tercerosEnviados);
                            Logger.Info($"Total terceros enviados exitosamente: {tercerosEnviados.Count} - IDs: {string.Join(", ", tercerosEnviados)}");
                        }

                        if (tercerosFallidos.Any())
                        {
                            Logger.Warn($"Total terceros que fallaron al enviar: {tercerosFallidos.Count} - {string.Join(" | ", tercerosFallidos)}");
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
                            var consecutivo = "";

                            if (factura.codigoFormaPago != 6)
                            {
                                Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Venta.Combustible} Enviandose a Siesa");

                                try
                                {
                                    var intentos = 0;
                                    do
                                    {
                                        try
                                        {
                                            infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(factura.ventaId, estacionFuente, _conexionEstacionRemota.getToken());
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

                                            string auxiliarContable = _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, true).Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                                            string auxiliarCruce = _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, false).Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

                                            // Dynamic discount auxiliary lookup (after cruce)
                                            string auxiliarDescuento = ObtenerAuxiliarDescuento(factura.codigoFormaPago, factura.Venta.Combustible);

                                            // Obtener fecha de facturación desde Dataico
                                            try
                                            {
                                                string dataicoToken = ConfigurationManager.AppSettings["dataico_token"];
                                                string url = $"https://api.dataico.com/dataico_api/v2/invoices?number={facturaElectronica[2]}";
                                                using (var client = new System.Net.Http.HttpClient())
                                                {
                                                    client.DefaultRequestHeaders.Add("auth-token", dataicoToken);
                                                    var response = client.GetAsync(url).Result;
                                                    if (response.IsSuccessStatusCode)
                                                    {
                                                        var json = response.Content.ReadAsStringAsync().Result;
                                                        dynamic obj = JsonConvert.DeserializeObject(json);
                                                        // La fecha está en obj.invoice.issue_date
                                                        string fechaFacturacion = obj.invoice.issue_date;
                                                        // Setear la fecha en la factura
                                                        factura.fecha = DateTime.ParseExact(fechaFacturacion, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                                    }
                                                    else
                                                    {
                                                        Logger.Warn($"No se pudo obtener la fecha de facturación desde Dataico para factura {facturaElectronica[2]}. Código: {response.StatusCode}");
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Error($"Error al obtener la fecha de facturación desde Dataico: {ex.Message}");
                                            }

                                            if (auxiliarContable == null)
                                            {
                                                Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Venta.Combustible} no se envió no exite auxiliar contrable creado");
                                            }
                                            if (auxiliarCruce == null)
                                            {
                                                Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Venta.Combustible} no se envió no exite auxiliar cruce creado");
                                            }
                                            Logger.Info($"Iniciando envío de factura - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Combustible: {factura.Venta.Combustible}");
                                            _apiContabilidad.EnviarFactura(factura, facturaElectronica[2], numeros, auxiliarContable, auxiliarCruce, auxiliarDescuento);
                                            //_apiContabilidad.EnviarRecibo(factura, facturaElectronica[2], numeros, _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, true), _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, false));
                                            facturasEnviadas.Add(factura.ventaId);
                                            Logger.Info($"Factura enviada exitosamente - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Combustible: {factura.Venta.Combustible}");
                                        }
                                    }
                                    else
                                    {

                                        facturasEnviadas.Add(factura.ventaId);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                    Logger.Warn($"Fallo al enviar factura - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                }
                            }
                            else
                            {
                                facturasEnviadas.Add(factura.ventaId);
                            }
                        }
                        catch (Exception ex)
                        {
                            facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                            Logger.Warn($"Fallo al procesar factura - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
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
        }
        /// <summary>
        /// Busca el auxiliar de descuento según forma de pago y combustible.
        /// </summary>
        /// <param name="codigoFormaPago">Código de forma de pago</param>
        /// <param name="combustible">Nombre del combustible</param>
        /// <returns>Auxiliar de descuento</returns>
        private string ObtenerAuxiliarDescuento(int codigoFormaPago, string combustible)
        {
            // Normalizar nombre de combustible
            var combustibleKey = (combustible ?? "").Trim().ToLower();
            if (combustibleKey == "corriente") combustibleKey = "corriente";
            // Puedes agregar más normalizaciones si es necesario

            // Buscar clave específica
            string key = $"auxiliardescuento_{codigoFormaPago}_{combustibleKey}";
            var valor = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(valor))
                return valor;
            // Si no existe, usar el general
            return ConfigurationManager.AppSettings["auxiliardescuento"];
        }
    }
    
}
