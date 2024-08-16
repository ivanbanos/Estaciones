using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturadorEstacionesRepositorio;
using Newtonsoft.Json;
using ReporteFacturas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public class ImpresionService
    {
        private Dictionary<int, string> carasImpresoras;
        private Dictionary<string, string> islasImpresoras;
        private int imprimiendo = 0;
        private bool ImpresionAutomatica = false;
        private bool impresionFormaDePagoOrdenDespacho = false;
        private string firstMacAddress;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly Guid estacionFuente;
        private readonly bool MultiplicarPor10;
        private readonly IFidelizacion _fidelizacion;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly bool generaFacturaElectronica;

        public ImpresionService()
        {
            estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            MultiplicarPor10 = bool.Parse(ConfigurationManager.AppSettings["MultiplicarPor10"]);
            _conexionEstacionRemota = new ConexionEstacionRemota();
            firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();
            firstMacAddress = firstMacAddress ?? "Mac Unknown";
            Console.WriteLine(ConfigurationManager.AppSettings["Razon"].ToString());
            ImpresionAutomatica = bool.Parse(ConfigurationManager.AppSettings["ImpresionAutomatica"]);
            impresionFormaDePagoOrdenDespacho = bool.Parse(ConfigurationManager.AppSettings["impresionFormaDePagoOrdenDespacho"]);
            _infoEstacion = new InfoEstacion();
            generaFacturaElectronica = bool.Parse(ConfigurationManager.AppSettings["GeneraFacturaElectronica"].ToString());
            _infoEstacion.CaracteresPorPagina = int.Parse(ConfigurationManager.AppSettings["CaracteresPorPagina"].ToString());
            _infoEstacion.ImpresionPDA = bool.Parse(ConfigurationManager.AppSettings["ImpresionPDA"].ToString());
            _infoEstacion.Direccion = ConfigurationManager.AppSettings["Direccion"].ToString();
            _infoEstacion.Linea1 = ConfigurationManager.AppSettings["Linea1"].ToString();
            _infoEstacion.Linea2 = ConfigurationManager.AppSettings["Linea2"].ToString();
            _infoEstacion.Linea3 = ConfigurationManager.AppSettings["Linea3"].ToString();
            _infoEstacion.Linea4 = ConfigurationManager.AppSettings["Linea4"].ToString();
            _infoEstacion.NIT = ConfigurationManager.AppSettings["NIT"].ToString();
            _infoEstacion.Nombre = ConfigurationManager.AppSettings["Nombre"].ToString();
            _infoEstacion.Razon = ConfigurationManager.AppSettings["Razon"].ToString();

            _infoEstacion.Telefono = ConfigurationManager.AppSettings["Telefono"].ToString();
            _infoEstacion.vecesPermitidasImpresion = int.Parse(ConfigurationManager.AppSettings["vecesPermitidasImpresion"].ToString());


            carasImpresoras = new Dictionary<int, string>();
            islasImpresoras = new Dictionary<string, string>();
            Console.WriteLine("Caras");
            foreach (string nameValueItem in ConfigurationManager.AppSettings)
            {
                if (nameValueItem.Contains("CARA"))
                {
                    Console.WriteLine(nameValueItem);
                    string impresora = ConfigurationManager.AppSettings[nameValueItem];
                    int isla = Int32.Parse(nameValueItem.Split(' ')[1]);
                    carasImpresoras.Add(isla, impresora);
                }
            }
            foreach (string nameValueItem in ConfigurationManager.AppSettings)
            {
                if (nameValueItem.Contains("ISLA"))
                {
                    Console.WriteLine(nameValueItem);
                    string impresora = ConfigurationManager.AppSettings[nameValueItem];
                    string isla = nameValueItem;
                    islasImpresoras.Add(isla, impresora);
                }
            }
            _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            formas = _estacionesRepositorio.BuscarFormasPagos();
            imprimiendo = 0;
            _fidelizacion = new FidelizacionConexionApi();
        }
        public void Execute()
        {
            DateTime lastTimeExec = DateTime.Now.AddMinutes(-5);
            while (true)
            {
                try
                {
                    try { 
                    var objetoImprimir = _estacionesRepositorio.GetObjetoImprimir().FirstOrDefault();
                    if (imprimiendo == 0 && objetoImprimir != null)
                    {
                        switch (objetoImprimir.Objeto)
                        {
                            case "Cierre":
                                {
                                    var turnoimprimir = _estacionesRepositorio.ObtenerTurnoCerradoPorIslaFecha(objetoImprimir.fecha, objetoImprimir.Isla);
                                    if (turnoimprimir == null)
                                    {
                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);

                                    }
                                    else 
                                    if (imprimiendo == 0)
                                    {
                                        imprimiendo++;
                                        Logger.Info($"imprimirnedo {JsonConvert.SerializeObject(turnoimprimir)} ");
                                        ImprimirTurno(turnoimprimir, objetoImprimir.Isla);

                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                    }
                                    else
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                            break;
                            case "Reimprimir":
                                {
                                    var turnoimprimir = _estacionesRepositorio.ObtenerTurnoIslaYFecha(objetoImprimir.fecha, objetoImprimir.Isla, objetoImprimir.Numero);
                                    if (turnoimprimir == null)
                                    {
                                        turnoimprimir = _estacionesRepositorio.ObtenerTurnoIslaYFecha(objetoImprimir.fecha.AddDays(-1), objetoImprimir.Isla, objetoImprimir.Numero);

                                    }
                                    if (turnoimprimir == null)
                                    {
                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);

                                    }
                                    else
                                    if (imprimiendo == 0)
                                    {
                                        imprimiendo++;
                                        Logger.Info($"imprimirnedo {JsonConvert.SerializeObject(turnoimprimir)} ");
                                        ImprimirTurno(turnoimprimir, objetoImprimir.Isla);

                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                    }
                                    else
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                                break;
                            case "Apertura":
                                {
                                    var turnoimprimir = _estacionesRepositorio.ObtenerTurnoPorIslaFecha(objetoImprimir.fecha, objetoImprimir.Isla);
                                    if (turnoimprimir == null)
                                    {
                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);

                                    }
                                    else if(imprimiendo == 0)
                                    {
                                        imprimiendo++;
                                        Logger.Info($"imprimirnedo {JsonConvert.SerializeObject(turnoimprimir)} ");
                                        ImprimirTurno(turnoimprimir, objetoImprimir.Isla);

                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                    }
                                    else
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                                break;
                            case "Bolsa":
                                {
                                    var bolsaimprimir = _estacionesRepositorio.ObtenerBolsa(objetoImprimir.fecha, objetoImprimir.Isla, objetoImprimir.Numero);
                                    if(bolsaimprimir.Consecutivo == 0)
                                    {
                                        bolsaimprimir = _estacionesRepositorio.ObtenerBolsa(objetoImprimir.fecha.AddDays(-1), objetoImprimir.Isla, objetoImprimir.Numero);

                                    }
                                    if (bolsaimprimir.Consecutivo == 0)
                                    {

                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                    }
                                    else if (imprimiendo == 0)
                                    {
                                        imprimiendo++;
                                        Logger.Info($"imprimirnedo {JsonConvert.SerializeObject(bolsaimprimir)} ");
                                        ImprimirBolsa(bolsaimprimir);

                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                    }
                                    else
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                                break;

                        }
                        
                    }

                    }
                    catch (Exception ex)
                    {

                        imprimiendo = 0;
                        Logger.Error("Error " + ex.Message);
                        Logger.Error("Error " + ex.StackTrace);
                        Thread.Sleep(100);
                    }
                    _estacionesRepositorio.AgregarFacturaDesdeIdVenta();
                    if (imprimiendo == 0)
                    {
                        if (lastTimeExec < DateTime.Now.AddHours(-5))
                        {
                            lastTimeExec = DateTime.Now;
                            try
                            {
                                var infoTemp = _conexionEstacionRemota.getInfoEstacion(estacionFuente, _conexionEstacionRemota.getToken());
                                _infoEstacion = infoTemp;
                            }
                            catch (Exception e)
                            {
                                _infoEstacion = new InfoEstacion
                                {
                                    CaracteresPorPagina = int.Parse(ConfigurationManager.AppSettings["CaracteresPorPagina"].ToString()),
                                    Direccion = ConfigurationManager.AppSettings["Direccion"].ToString(),
                                    Linea1 = ConfigurationManager.AppSettings["Linea1"].ToString(),
                                    Linea2 = ConfigurationManager.AppSettings["Linea2"].ToString(),
                                    Linea3 = ConfigurationManager.AppSettings["Linea3"].ToString(),
                                    Linea4 = ConfigurationManager.AppSettings["Linea4"].ToString(),
                                    NIT = ConfigurationManager.AppSettings["NIT"].ToString(),
                                    Nombre = ConfigurationManager.AppSettings["Nombre"].ToString(),
                                    Razon = ConfigurationManager.AppSettings["Razon"].ToString(),

                                    Telefono = ConfigurationManager.AppSettings["Telefono"].ToString(),
                                    vecesPermitidasImpresion = int.Parse(ConfigurationManager.AppSettings["vecesPermitidasImpresion"].ToString())
                                };

                            }
                        }
                        //Console.WriteLine("Buscando facturas");
                        var caras = _estacionesRepositorio.getCaras();
                         Console.WriteLine($"Caras {JsonConvert.SerializeObject(caras)}");
                        foreach (var cara in caras)
                        {

                            var facturai = _estacionesRepositorio.getUltimasFacturas(cara.COD_CAR, 1).FirstOrDefault();
                            if (ImpresionAutomatica)
                            {
                                if (facturai != null && facturai.Venta != null && facturai.Venta.CONSECUTIVO != -1
                                    && ((facturai.impresa == 0)))
                                {

                                    Console.WriteLine("Imprimiendo facturas");
                                    imprimiendo++;
                                    Imprimir(facturai);
                                    Console.WriteLine("Fin impresion");
                                    Thread.Sleep(100);
                                    break;
                                }


                            }
                        }

                        var factura = _estacionesRepositorio.getFacturasImprimir();

                        if (imprimiendo == 0 && factura != null && factura.Venta != null && factura.Venta.CONSECUTIVO != -1
                            && ((factura.impresa == 0 && ImpresionAutomatica) || factura.impresa <= -1))
                        {


                            while (true)
                            {
                                if (imprimiendo == 0)
                                {
                                    imprimiendo++;
                                    Imprimir(factura);
                                    factura.impresa++;
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                }
                                if (factura.impresa == 0)
                                {
                                    break;
                                }
                            }
                            Thread.Sleep(100);
                        }


                        Thread.Sleep(100);

                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {

                    imprimiendo = 0;
                    Logger.Error("Error " + ex.Message);
                    Logger.Error("Error " + ex.StackTrace);
                    Thread.Sleep(100);
                }
            }
        }

        private List<LineasImprimir> lineasImprimirBolsa;
        private void ImprimirBolsa(Models.Bolsa bolsaimprimir)
        {
            try
            {

                getLineasImprimirTurnoBolsa(bolsaimprimir);
                try
                {
                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintBolsa);
                    pd.DefaultPageSettings.Margins.Bottom = 20; 
                    if (bolsaimprimir != null && bolsaimprimir.Isla != null && islasImpresoras.ContainsKey(bolsaimprimir.Isla))
                    {

                        Console.WriteLine("Selecionando impresora " + islasImpresoras[bolsaimprimir.Isla].Trim());
                        pd.PrinterSettings.PrinterName = islasImpresoras[bolsaimprimir.Isla].Trim();
                    }
                    pd.Print();

                }
                catch (Exception ex)
                {

                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintBolsa);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    pd.Print();

                }
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimirTurnoBolsa(Bolsa bolsaimprimir)
        {
            lineasImprimirBolsa = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _infoEstacion.CaracteresPorPagina);
            // Iterate over the file, printing each line.
            lineasImprimirBolsa.Add(new LineasImprimir(".", true));
            lineasImprimirBolsa.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimirBolsa.Add(new LineasImprimir("NIT             " + _infoEstacion.NIT, false));
            lineasImprimirBolsa.Add(new LineasImprimir(_infoEstacion.Nombre, false));
            lineasImprimirBolsa.Add(new LineasImprimir(_infoEstacion.Direccion, false));
            lineasImprimirBolsa.Add(new LineasImprimir(_infoEstacion.Telefono, false));
            lineasImprimirBolsa.Add(new LineasImprimir(guiones.ToString(), false));

            lineasImprimirBolsa.Add(new LineasImprimir("Isla: "+bolsaimprimir.Isla, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Turno: " + bolsaimprimir.NumeroTurno, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Fecha: " + bolsaimprimir.Fecha, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Empleado: " + bolsaimprimir.Empleado, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Consecutivo: " + bolsaimprimir.Consecutivo, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Bilete: " + bolsaimprimir.Billete, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Moneda: " + bolsaimprimir.Moneda, false));

            lineasImprimirBolsa.Add(new LineasImprimir(guiones.ToString(), false));

            lineasImprimirBolsa.Add(new LineasImprimir("Fabricado por:" + " SIGES SOLUCIONES SAS ", true));
            lineasImprimirBolsa.Add(new LineasImprimir("Nit:" + " 901430393-2 ", true));
            lineasImprimirBolsa.Add(new LineasImprimir("Nombre:" + " Facturador SIGES ", true));
            lineasImprimirBolsa.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress ?? ""), false));
            lineasImprimirBolsa.Add(new LineasImprimir(".", true));
        }

        private List<LineasImprimir> lineasImprimirTurno;
        private void ImprimirTurno(Turno turnoimprimir, int isla)
        {

            try
            {

                getLineasImprimirTurno(turnoimprimir, isla);
                try
                {
                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintTurno);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    if (turnoimprimir != null && turnoimprimir.Isla != null && islasImpresoras.ContainsKey(turnoimprimir.Isla))
                    {

                        Console.WriteLine("Selecionando impresora " + islasImpresoras[turnoimprimir.Isla].Trim());
                        pd.PrinterSettings.PrinterName = islasImpresoras[turnoimprimir.Isla].Trim();
                    }
                    pd.Print();

                }
                catch (Exception ex)
                {

                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintTurno);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    pd.Print();

                }
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimirTurno(Turno turnoimprimir, int isla)
        {
            lineasImprimirTurno = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _infoEstacion.CaracteresPorPagina);
            // Iterate over the file, printing each line.
            lineasImprimirTurno.Add(new LineasImprimir(".", true));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimirTurno.Add(new LineasImprimir("NIT             " + _infoEstacion.NIT, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Nombre, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Direccion, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Telefono, false));
            lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimirTurno.Add(new LineasImprimir("Empleado:       " + turnoimprimir.Empleado, false));
            lineasImprimirTurno.Add(new LineasImprimir("Isla:           " + turnoimprimir.Isla, false));
            lineasImprimirTurno.Add(new LineasImprimir("Numero:           " + turnoimprimir.Numero, false));
            lineasImprimirTurno.Add(new LineasImprimir("Fecha apertura: " + turnoimprimir.FechaApertura.ToString(), false));
            var reporteCierrePorTotal = new List<FactoradorEstacionesModelo.Objetos.Factura>();
            if (turnoimprimir.FechaCierre.HasValue)
            {
                lineasImprimirTurno.Add(new LineasImprimir("Fecha cierre:   " + turnoimprimir.FechaCierre.Value.ToString(), false));
                reporteCierrePorTotal = _estacionesRepositorio.getFacturaPorTurno(isla, turnoimprimir.FechaApertura,turnoimprimir.Numero).ToList();
            }


            var totalCantidad = 0m;
            var totalVenta = 0m;

            lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            foreach (var turnosurtidor in turnoimprimir.turnoSurtidores)
            {
                if (MultiplicarPor10)
                {
                    turnosurtidor.precioCombustible *= 10;
                }
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Manguera :", turnosurtidor.Manguera), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Combustible :", turnosurtidor.Combustible), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Precio :", $"${string.Format("{0:N2}", turnosurtidor.precioCombustible)}"), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Apertura :", turnosurtidor.Apertura.ToString()), false));
                if (turnoimprimir.FechaCierre.HasValue)
                {
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Cierre :", turnosurtidor.Cierre.ToString()), false));

                    Logger.Info("reporte " + JsonConvert.SerializeObject(reporteCierrePorTotal));
                    Logger.Info("turno surtidor " + JsonConvert.SerializeObject(turnosurtidor));
                    totalCantidad += Convert.ToDecimal( turnosurtidor.Cierre.Value - turnosurtidor.Apertura);
                    totalVenta += Convert.ToDecimal((turnosurtidor.Cierre.Value - turnosurtidor.Apertura) * turnosurtidor.precioCombustible);
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Cantidad :", string.Format("{0:N2}", turnosurtidor.Cierre - turnosurtidor.Apertura)), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${string.Format("{0:N2}", (turnosurtidor.Cierre - turnosurtidor.Apertura) * turnosurtidor.precioCombustible)}"), false));

                }

                lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            }
            if (turnoimprimir.FechaCierre.HasValue)
            {
                if (reporteCierrePorTotal != null && reporteCierrePorTotal.Any())
                {

                    //Por forma
                    lineasImprimirTurno.Add(new LineasImprimir($"Resumen por forma de pago", true));
                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    var groupForma = reporteCierrePorTotal.GroupBy(x => x.codigoFormaPago);
                    Logger.Info("facturas turno " + JsonConvert.SerializeObject(groupForma));

                    var cantidadTotalmenosEfectivo = 0m;
                    var ventaTotalmenosEfectivo = 0m;
                    foreach (var forma in groupForma)
                    {
                        if (formas.Any(x => x.Id == forma.Key) && forma.Key != 1)
                        {
                            cantidadTotalmenosEfectivo += forma.Sum(x => x.Venta.CANTIDAD);
                            ventaTotalmenosEfectivo += forma.Sum(x => x.Venta.TOTAL);
                            lineasImprimirTurno.Add(new LineasImprimir(formatoTotales($"{formas.First(x => x.Id == forma.Key).Descripcion.Trim()} :", $"${string.Format("{0:N2}", forma.Sum(x => x.Venta.TOTAL))}"), false));

                        }
                    }

                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${string.Format("{0:N2}", totalVenta)}"), false));


                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));

                    lineasImprimirTurno.Add(new LineasImprimir($"Resumen por Combustibles", true));
                    //Totalizador
                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    var porCombustible = reporteCierrePorTotal.GroupBy(x => x.Venta.Combustible);
                    foreach(var combustible in porCombustible)
                    {
                        var total = combustible.Sum(x => x.Venta.SUBTOTAL);
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Combustible :", combustible.Key), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Precio :", $"${string.Format("{0:N2}", combustible.First().Venta.PRECIO_UNI)}"), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Subtotal :", $"${string.Format("{0:N2}", total)}"), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Calibracion :", "$0,00"), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Descuento :", $"${string.Format("{0:N2}", total)}"), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${string.Format("{0:N2}", total)}"), false));


                        lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    }

                    lineasImprimirTurno.Add(new LineasImprimir($"Resumen de bolsas", true));
                    //Totalizador
                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Numero de bolsas :", turnoimprimir.Bolsas.Count().ToString()), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total billetes :", turnoimprimir.Bolsas.Sum(x=>x.Billete).ToString()), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total monedas :", turnoimprimir.Bolsas.Sum(x => x.Moneda).ToString()), false));

                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                }

            }

            lineasImprimirTurno.Add(new LineasImprimir("Fabricado por:" + " SIGES SOLUCIONES SAS ", true));
            lineasImprimirTurno.Add(new LineasImprimir("Nit:" + " 901430393-2 ", true));
            lineasImprimirTurno.Add(new LineasImprimir("Nombre:" + " Facturador SIGES ", true));
            lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress ?? ""), false));
            lineasImprimirTurno.Add(new LineasImprimir(".", true));
        }

        private Font printFont;
        private FactoradorEstacionesModelo.Objetos.Factura _factura;
        private Venta _venta;
        private FactoradorEstacionesModelo.Objetos.Tercero _tercero;
        private Manguera _mangueras;
        private InfoEstacion _infoEstacion;
        private int _charactersPerPage;
        private EstacionesRepositorioSqlServer _estacionesRepositorio;
        private List<FormasPagos> formas;
        private List<LineasImprimir> lineasImprimir;

        private IEnumerable<LineasImprimir> getPuntos(int ventaId)
        {
            var fidelizado = _estacionesRepositorio.getFidelizado(ventaId);
            if (fidelizado != null)
            {
                fidelizado = _fidelizacion.GetFidelizados(fidelizado.Documento).Result != null ? _fidelizacion.GetFidelizados(fidelizado.Documento).Result.FirstOrDefault() : fidelizado;
                if (fidelizado != null)
                {
                    return new List<LineasImprimir>() {
                    new LineasImprimir(formatoTotales("Fidelizado:", fidelizado.Nombre??fidelizado.Documento), false)
                , new LineasImprimir(formatoTotales("Puntos:", fidelizado.Puntos.ToString()), false)};
                }
            }
            return new List<LineasImprimir>() { new LineasImprimir("Usuario no fidelizado", false) };


        }
        private void Imprimir(FactoradorEstacionesModelo.Objetos.Factura factura)
        {
            _factura = factura;
            _venta = factura.Venta;
            _tercero = factura.Tercero;
            _mangueras = factura.Manguera;
            getLineasImprimir();
            //imprimir
            try
            {

                try
                {
                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintPageOnly);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    // Print the document.
                    if (_venta != null && _venta.COD_CAR != null && carasImpresoras.ContainsKey(_venta.COD_CAR))
                    {

                        Console.WriteLine("Selecionando impresora " + carasImpresoras[_venta.COD_CAR].Trim());
                        pd.PrinterSettings.PrinterName = carasImpresoras[_venta.COD_CAR].Trim();
                    }

                    pd.Print();

                }
                catch (Exception ex)
                {

                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintPageOnly);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    pd.Print();

                }
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimir()
        {
            _charactersPerPage = _infoEstacion.CaracteresPorPagina;
            if (_factura.codigoFormaPago == 0)
            {
                _factura.codigoFormaPago = _venta.COD_FOR_PAG;
            }
            if (_charactersPerPage == 0)
            {
                _charactersPerPage = 40;
            }
            lineasImprimir = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _charactersPerPage);
            // Iterate over the file, printing each line.
            lineasImprimir.Add(new LineasImprimir(".", true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimir.Add(new LineasImprimir("NIT " + _infoEstacion.NIT, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Nombre, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Direccion, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Telefono, true));
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            var infoTemp = "";
            if (generaFacturaElectronica && _factura.codigoFormaPago != 6)
            {
                try
                {
                    var intentos = 0;
                    do
                    {
                        infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(_factura.ventaId, estacionFuente, _conexionEstacionRemota.getToken());
                        Thread.Sleep(100);
                    } while (infoTemp == null || intentos++<3);

                    Console.WriteLine("info fac elec " + infoTemp);
                    Logger.Info("info fac elec " + infoTemp);
                }
                catch (Exception ex )
                {
                    Logger.Info("info fac elec " + ex.Message);
                    Logger.Info("info fac elec " + ex.StackTrace);
                    Console.WriteLine("info fac elec " + ex.Message);
                    Console.WriteLine("info fac elec " + ex.StackTrace);
                }
            }
            if (!string.IsNullOrEmpty(infoTemp))
            {
                infoTemp = infoTemp.Replace("\n\r", " ");

                var facturaElectronica = infoTemp.Split(' ');

                lineasImprimir.Add(new LineasImprimir("Factura Electrónica de Venta " + facturaElectronica[2], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[3], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[4].Substring(0,facturaElectronica[4].Length / 2), true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[4].Substring(facturaElectronica[4].Length / 2), true));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("Venta: " + _venta.CONSECUTIVO, false));
            }
            else if (_factura.Consecutivo == 0)
            {

                lineasImprimir.Add(new LineasImprimir("Orden de despacho No: " + _venta.CONSECUTIVO, true));
            }
            else
            {
                lineasImprimir.Add(new LineasImprimir("Orden de Servicio Temporal: " + _venta.CONSECUTIVO, true));
            }

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            var placa = (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _venta.PLACA + "").Trim();
            if (_venta.COD_FOR_PAG != 4)
            {

                lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", _tercero.Nombre == null ? "" : _tercero.Nombre.Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _tercero.identificacion.Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", placa), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : _venta.KILOMETRAJE + "").Trim()), false));
                var codigoInterno = _factura.Venta.COD_INT != null ? _factura.Venta.COD_INT : _estacionesRepositorio.ObtenerCodigoInterno(placa, _tercero.identificacion.Trim());
                if (codigoInterno != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Cod Int : ", codigoInterno), false));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_tercero.Nombre))
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a :", " CONSUMIDOR FINAL".Trim()), false));
                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", _tercero.Nombre.Trim()) + "", false));
                }
                if (string.IsNullOrEmpty(_tercero.identificacion))
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", "222222222222".Trim()), false));
                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _tercero.identificacion.Trim()), false));
                }
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _venta.PLACA + "").Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : _venta.KILOMETRAJE + "").Trim()), false));
                var codigoInterno = _factura.Venta.COD_INT != null ? _factura.Venta.COD_INT : _estacionesRepositorio.ObtenerCodigoInterno(placa, _tercero.identificacion.Trim());
                if (codigoInterno != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Cod Int : ", codigoInterno), false));
                }
            }

            if (_venta.FECH_PRMA.HasValue && (_mangueras.DESCRIPCION.ToLower().Contains("gn") || _mangueras.DESCRIPCION.ToLower().Contains("gas")))
            {
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Proximo mantenimiento : ", _venta.FECH_PRMA.Value.ToString("dd/MM/yyyy").Trim()), false));
            }

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Fecha : ", _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss")), false));

            lineasImprimir.Add(new LineasImprimir(formatoTotales("Surtidor : ", _venta.COD_SUR + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Cara : ", _venta.COD_CAR + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Manguera : ", _mangueras.COD_MAN + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendedor : ", _venta.EMPLEADO.Trim() + ""), false));
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            if (MultiplicarPor10)
            {
                _venta.PRECIO_UNI = _venta.PRECIO_UNI * 10;
                _venta.VALORNETO = _venta.VALORNETO * 10;
                _venta.VALORNETO = _venta.TOTAL * 10;
                _venta.Descuento = _venta.Descuento * 10;
            }
            if (_infoEstacion.ImpresionPDA)
            {
                lineasImprimir.Add(new LineasImprimir($"Producto: {_mangueras.DESCRIPCION.Trim()}", false));
                lineasImprimir.Add(new LineasImprimir($"Cantidad: {string.Format("{0:#,0.000}", _venta.CANTIDAD)}", false));
                lineasImprimir.Add(new LineasImprimir($"Precio: {_venta.PRECIO_UNI.ToString("F")}", false));
                lineasImprimir.Add(new LineasImprimir($"Total: {_venta.VALORNETO}", false));

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Precio", "   Total") + "", false));
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), _venta.PRECIO_UNI.ToString("F"), String.Format("{0:#,0.00}", _venta.VALORNETO), true) + "", false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            lineasImprimir.Add(new LineasImprimir("DISCRIMINACION TARIFAS IVA" + "", true));
            //  lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            if (_infoEstacion.ImpresionPDA)
            {
                lineasImprimir.Add(new LineasImprimir($"Producto: {_mangueras.DESCRIPCION.Trim()}", false));
                lineasImprimir.Add(new LineasImprimir($"Cantidad: {string.Format("{0:#,0.000}", _venta.CANTIDAD)}", false));
                lineasImprimir.Add(new LineasImprimir($"Tafira: 0 % ", false));
                lineasImprimir.Add(new LineasImprimir($"Total: {_venta.VALORNETO}", false));

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Tafira", "   Total") + "", false));
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), "0%", String.Format("{0:#,0.00}", _venta.VALORNETO), true) + "", false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Descuento: ", String.Format("{0:#,0.00}", _venta.Descuento)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal sin IVA : ", String.Format("{0:#,0.00}", _venta.TOTAL)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal IVA :", "0,00"), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("TOTAL : ", String.Format("{0:#,0.00}", _venta.TOTAL)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));

            if (_factura.Consecutivo != 0 || impresionFormaDePagoOrdenDespacho)
            {
                if (formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago) != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Forma de pago : ", formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago).Descripcion.Trim()), false));

                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Forma de pago :", " Efectivo"), false));

                }
                if (!string.IsNullOrEmpty( _factura.numeroTransaccion)&&_factura.numeroTransaccion != "NA")
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("N Tran :", _factura.numeroTransaccion), false));
                }

            }

            if (!string.IsNullOrEmpty(infoTemp))
            {
                try
                {
                    var resoluconElectronica = _conexionEstacionRemota.GetResolucionElectronica(_conexionEstacionRemota.getToken());

                    lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                    lineasImprimir.Add(new LineasImprimir(resoluconElectronica.invoiceText, false));
                }
                catch (Exception)
                {
                    lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                    lineasImprimir.Add(new LineasImprimir("Modalidad Factura Electrónica ", false));
                }

            }

            if (!String.IsNullOrEmpty(_infoEstacion.Linea1))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea1, false));
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea2))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea2, false));
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea3))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea3, false));
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea4))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea4, false));
            }
            lineasImprimir.Add(new LineasImprimir("Fabricado por:" + " SIGES SOLUCIONES SAS ", true));
            lineasImprimir.Add(new LineasImprimir("Nit:" + " 901430393-2 ", true));
            lineasImprimir.Add(new LineasImprimir("Nombre:" + " Facturador SIGES ", true));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress), false));
            lineasImprimir.Add(new LineasImprimir(".", true));

        }

        private void pd_PrintPageOnly(object sender, PrintPageEventArgs ev)
        {
            try
            {
                if (_factura.codigoFormaPago == 0)
                {
                    _factura.codigoFormaPago = _venta.COD_FOR_PAG;
                }
                float yPos = 0;
                int count = 0;
                float leftMargin = 5;
                float topMargin = 10;
                String line = null;
                int sizePaper = ev.PageSettings.PaperSize.Width;
                int fonSizeInches = 72 / 9;
                _charactersPerPage = _infoEstacion.CaracteresPorPagina;
                if (_charactersPerPage == 0)
                {
                    _charactersPerPage = fonSizeInches * sizePaper / 100;
                }
                foreach (var linea in lineasImprimir)
                {

                    count = printLine(linea.linea, ev, count, leftMargin, topMargin, linea.centrada);
                }
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;


                _estacionesRepositorio.SetFacturaImpresa(_factura.ventaId);
                imprimiendo--;
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }

        }

        
        private void pd_PrintBolsa(object sender, PrintPageEventArgs ev)
        {
            try
            {

                float yPos = 0;
                int count = 0;
                float leftMargin = 5;
                float topMargin = 10;
                String line = null;
                int sizePaper = ev.PageSettings.PaperSize.Width;
                int fonSizeInches = 72 / 9;
                if (_infoEstacion.CaracteresPorPagina == 0)
                {
                    _infoEstacion.CaracteresPorPagina = fonSizeInches * sizePaper / 100;
                }
                foreach (var linea in lineasImprimirBolsa)
                {

                    count = printLine(linea.linea, ev, count, leftMargin, topMargin, linea.centrada);
                }
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;


                imprimiendo--;
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }

        }

        private void pd_PrintTurno(object sender, PrintPageEventArgs ev)
        {
            try
            {

                float yPos = 0;
                int count = 0;
                float leftMargin = 5;
                float topMargin = 10;
                String line = null;
                int sizePaper = ev.PageSettings.PaperSize.Width;
                int fonSizeInches = 72 / 9;
                if (_infoEstacion.CaracteresPorPagina == 0)
                {
                    _infoEstacion.CaracteresPorPagina = fonSizeInches * sizePaper / 100;
                }
                foreach (var linea in lineasImprimirTurno)
                {

                    count = printLine(linea.linea, ev, count, leftMargin, topMargin, linea.centrada);
                }
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;


                imprimiendo--;
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }

        }



        private string formatoTotales(string v1, string v2)
        {
            var result = v1;
            var tabs = new StringBuilder();
            tabs.Append(v1);
            var whitespaces = _charactersPerPage - v1.Length - v2.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v2);
            return tabs.ToString();
        }

        private string getLienaTarifas(string v1, string v2, string v3, string v4, bool after = false)
        {
            var spacesInPage = _charactersPerPage / 4;
            var tabs = new StringBuilder();
            if (_charactersPerPage == 40)
            {
                tabs.Append(v1.Substring(0, v1.Length < 12 ? v1.Length : 12));
                var whitespaces = 12 - v1.Length;
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);


                if (after)
                {
                    whitespaces = 8 - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v2.Substring(0, v2.Length < 8 ? v2.Length : 8));

                    whitespaces = 8 - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v3.Substring(0, v3.Length < 8 ? v3.Length : 8));

                    whitespaces = 12 - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v4.Substring(0, v4.Length < 12 ? v4.Length : 12));
                }
                else
                {
                    tabs.Append(v2.Substring(0, v2.Length < 8 ? v2.Length : 8));
                    whitespaces = 8 - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v3.Substring(0, v3.Length < 8 ? v3.Length : 8));
                    whitespaces = 8 - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v4.Substring(0, v4.Length < 12 ? v4.Length : 12));
                    whitespaces = 12 - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);


                }
                return tabs.ToString();
            }
            else
            {
                tabs.Append(v1.Substring(0, v1.Length < spacesInPage ? v1.Length : spacesInPage));
                var whitespaces = spacesInPage - v1.Length;
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);


                if (after)
                {
                    whitespaces = spacesInPage - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));

                    whitespaces = spacesInPage - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));

                    whitespaces = spacesInPage - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
                }
                else
                {
                    tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));
                    whitespaces = spacesInPage - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));
                    whitespaces = spacesInPage - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
                    whitespaces = spacesInPage - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);


                }
                return tabs.ToString();
            }
        }

        private int printLine(string text, PrintPageEventArgs ev, int count, float leftMargin, float topMargin, bool center = false)
        {
            if (center)
            {
                var whitespaces = (_charactersPerPage - text.Length) / 2;
                var tabs = new StringBuilder();
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);
                text = tabs.ToString() + text;
            }
            float yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
            ev.Graphics.DrawString(text, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
            count++;
            return count;
        }
    }

    public class LineasImprimir
    {
        public LineasImprimir(string linea, bool centrada)
        {
            this.linea = linea;
            this.centrada = centrada;
        }

        public string linea;
        public bool centrada;

    }
}