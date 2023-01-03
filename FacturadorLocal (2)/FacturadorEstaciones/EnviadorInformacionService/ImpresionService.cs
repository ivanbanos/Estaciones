using FactoradorEstacionesModelo.Objetos;
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
        private int imprimiendo = 0;
        private bool ImpresionAutomatica = false;
        private bool impresionFormaDePagoOrdenDespacho = false;
        private string firstMacAddress;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly Guid estacionFuente;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly bool generaFacturaElectronica;

        public ImpresionService()
        {
            estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            _conexionEstacionRemota = new ConexionEstacionRemota();
            firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();

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
            _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            formas = _estacionesRepositorio.BuscarFormasPagos();
            imprimiendo = 0;
        }
        public void Execute()
        {
            DateTime lastTimeExec = DateTime.Now.AddMinutes(-5);
            while (true)
            {
                try
                {

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
                                    Thread.Sleep(1000);
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
                            Thread.Sleep(1000);
                        }


                        Thread.Sleep(500);

                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {

                    imprimiendo = 0;
                    Logger.Error("Error " + ex.Message);
                    Logger.Error("Error " + ex.StackTrace);
                    Thread.Sleep(500);
                }
            }
        }

        private Font printFont;
        private Factura _factura;
        private Venta _venta;
        private Tercero _tercero;
        private Manguera _mangueras;
        private InfoEstacion _infoEstacion;
        private int _charactersPerPage;
        private EstacionesRepositorioSqlServer _estacionesRepositorio;
        private List<FormasPagos> formas;
        private List<LineasImprimir> lineasImprimir;
        private void Imprimir(Factura factura)
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
            if (generaFacturaElectronica)
            {
                try
                {
                    infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(_factura.ventaId, estacionFuente, _conexionEstacionRemota.getToken());

                }
                catch (Exception)
                {
                    infoTemp = null;
                }
            }
            if (!string.IsNullOrEmpty(infoTemp))
            {
                infoTemp = infoTemp.Replace("\n\r", " ");

                var facturaElectronica = infoTemp.Split(' ');

                lineasImprimir.Add(new LineasImprimir("Factura Electrónica" + facturaElectronica[2], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[3], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[4], true));
            }
            else if (_factura.Consecutivo == 0)
            {

                lineasImprimir.Add(new LineasImprimir("Orden de despacho No: " + _venta.CONSECUTIVO, true));
            }
            else
            {
                lineasImprimir.Add(new LineasImprimir("Factura de venta P.O.S No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo, true));
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

            else if (_factura.Consecutivo != 0)
            {


                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("Resolucion de Facturacion No. ", false));
                lineasImprimir.Add(new LineasImprimir(_factura.Autorizacion + " de " + _factura.FechaInicioResolucion.ToString("dd/MM/yyyy") + " ", false));
                var numeracion = "Numeracion Autorizada por la DIAN";
                if (_factura.habilitada)
                {
                    numeracion = "Numeracion Habilitada por la DIAN";
                }
                lineasImprimir.Add(new LineasImprimir(numeracion + " ", false));
                lineasImprimir.Add(new LineasImprimir("Del " + _factura.DescripcionResolucion + "-" + _factura.Inicio + " al " + _factura.DescripcionResolucion + "-" + _factura.Final + "", false));

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