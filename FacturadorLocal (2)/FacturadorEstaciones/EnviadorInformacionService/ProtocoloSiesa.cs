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

                    var terceros = facturas.Select(x => x.Tercero);
                    if (terceros.Any(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                    {
                        foreach(var t in terceros.Where(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value)){

                            _apiContabilidad.EnviarTercero(t);
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
                                        infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(factura.ventaId, estacionFuente, _conexionEstacionRemota.getToken());
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
                                            if (auxiliarContable == null)
                                            {

                                                Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Venta.Combustible} no se envió no exite auxiliar contrable creado");
                                            }
                                            if (auxiliarCruce == null)
                                            {

                                                Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Venta.Combustible} no se envió no exite auxiliar cruce creado");
                                            }
                                            _apiContabilidad.EnviarFactura(factura, facturaElectronica[2], numeros, auxiliarContable, auxiliarCruce);
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
                    if(facturasEnviadas.Any() )
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
        }
    }
}
