using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using FacturadorEstacionesRepositorio;
using FactoradorEstacionesModelo.Siges;

namespace ServicioSIGES
{
    public class Impresion : BackgroundService
    {
        private readonly ILogger<Impresion> _logger;
        private int imprimiendo = 0;
        private bool ImpresionAutomatica = false;
        private bool impresionFormaDePagoOrdenDespacho = false;
        private string firstMacAddress;
        private readonly Guid estacionFuente;
        private readonly IEstacionesRepositorio _estacionesRepositorio;

        private readonly bool generaFacturaElectronica;
        private readonly InfoEstacion _infoEstacion;
        private readonly List<CaraImpresora> _caraImpresoras; 
        public Impresion(ILogger<Impresion> logger, IEstacionesRepositorio estacionesRepositorio, IOptions<InfoEstacion> infoEstacion, IOptions<List<CaraImpresora>> caraImpresoras)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion.Value;
            _caraImpresoras = caraImpresoras.Value;
            estacionFuente = new Guid(_infoEstacion.EstacionFuente);
            firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();

            Console.WriteLine(_infoEstacion.Razon);
            ImpresionAutomatica = _infoEstacion.ImpresionAutomatica;
            impresionFormaDePagoOrdenDespacho = _infoEstacion.ImpresionFormaDePagoOrdenDespacho;
            generaFacturaElectronica = _infoEstacion.GeneraFacturaElectronica;
            

            formas = _estacionesRepositorio.BuscarFormasPagosSiges();
            imprimiendo = 0;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (true)
                {
                    try
                    {

                        if (imprimiendo == 0)
                        {
                            
                            var factura = _estacionesRepositorio.getFacturasImprimir();

                            if (imprimiendo == 0 && factura != null 
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
                                await Task.Delay(1000, stoppingToken);
                            }


                            await Task.Delay(1000, stoppingToken);

                        }
                        else
                        {
                            await Task.Delay(1000, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {

                        imprimiendo = 0;
                        _logger.LogError("Error " + ex.Message);
                        _logger.LogError("Error " + ex.StackTrace);
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
        }
        private Font printFont;
        private FacturaSiges _factura;
        private int _charactersPerPage;
        private List<FormaPagoSiges> formas;
        private List<LineasImprimir> lineasImprimir;
        private void Imprimir(FacturaSiges factura)
        {
            _factura = factura;
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
                    if (_caraImpresoras.Any(x=>x.Cara == factura.Cara))
                    {

                        Console.WriteLine("Selecionando impresora " + _caraImpresoras.First(x => x.Cara == factura.Cara).Impresora.Trim());
                        pd.PrinterSettings.PrinterName = _caraImpresoras.First(x => x.Cara == factura.Cara).Impresora.Trim();
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
                _logger.LogError("Error " + ex.Message);
                _logger.LogError("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimir()
        {
            _charactersPerPage = _infoEstacion.CaracteresPorPagina;
            
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

                lineasImprimir.Add(new LineasImprimir("Orden de despacho No: " + _factura.Consecutivo, true));
            }
            else
            {
                lineasImprimir.Add(new LineasImprimir("Factura de venta P.O.S No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo, true));
            }

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            var placa = (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _factura.Placa + "").Trim();
            if (_factura.codigoFormaPago != 1)
            {

                lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", _factura.Tercero.Nombre == null ? "" : _factura.Tercero.Nombre.Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _factura.Tercero.identificacion.Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", placa), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje :  "").Trim()), false));
                var codigoInterno = _factura.CodigoInterno ;
                if (codigoInterno != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Cod Int : ", codigoInterno), false));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_factura.Tercero.Nombre))
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a :", " CONSUMIDOR FINAL".Trim()), false));
                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", _factura.Tercero.Nombre.Trim()) + "", false));
                }
                if (string.IsNullOrEmpty(_factura.Tercero.identificacion))
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", "222222222222".Trim()), false));
                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _factura.Tercero.identificacion.Trim()), false));
                }
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _factura.Placa + "").Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje :  "").Trim()), false));
                var codigoInterno = _factura.CodigoInterno;
                if (codigoInterno != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Cod Int : ", codigoInterno), false));
                }
            }

            //if (_factura.fechaUltimaActualizacion.HasValue && (_mangueras.DESCRIPCION.ToLower().Contains("gn") || _mangueras.DESCRIPCION.ToLower().Contains("gas")))
            //{
            //    lineasImprimir.Add(new LineasImprimir(formatoTotales("Proximo mantenimiento : ", _venta.FECH_PRMA.Value.ToString("dd/MM/yyyy").Trim()), false));
            //}

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Fecha : ", _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss")), false));

            lineasImprimir.Add(new LineasImprimir(formatoTotales("Surtidor : ", _factura.Surtidor + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Cara : ", _factura.Cara + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Manguera : ", _factura.Mangueras + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendedor : ", _factura.Empleado?.Trim() + ""), false));
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            if (_infoEstacion.ImpresionPDA)
            {
                lineasImprimir.Add(new LineasImprimir($"Producto: {_factura.Combustible.Trim()}", false));
                lineasImprimir.Add(new LineasImprimir($"Cantidad: {string.Format("{0:#,0.000}", _factura.Cantidad)}", false));
                lineasImprimir.Add(new LineasImprimir($"Precio: {_factura.Precio.ToString("F")}", false));
                lineasImprimir.Add(new LineasImprimir($"Total: {_factura.Total}", false));

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Precio", "   Total") + "", false));
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas(_factura.Combustible.Trim(), String.Format("{0:#,0.000}", _factura.Cantidad), _factura.Precio.ToString("F"), String.Format("{0:#,0.00}", _factura.Total), true) + "", false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            lineasImprimir.Add(new LineasImprimir("DISCRIMINACION TARIFAS IVA" + "", true));
            //  lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            if (_infoEstacion.ImpresionPDA)
            {
                lineasImprimir.Add(new LineasImprimir($"Producto: {_factura.Combustible.Trim()}", false));
                lineasImprimir.Add(new LineasImprimir($"Cantidad: {string.Format("{0:#,0.000}", _factura.Cantidad)}", false));
                lineasImprimir.Add(new LineasImprimir($"Tafira: 0 % ", false));
                lineasImprimir.Add(new LineasImprimir($"Total: {_factura.Total}", false));

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Tafira", "   Total") + "", false));
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas(_factura.Combustible.Trim(), String.Format("{0:#,0.000}", _factura.Cantidad), "0%", String.Format("{0:#,0.00}", _factura.Total), true) + "", false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Descuento: ", String.Format("{0:#,0.00}", _factura.Descuento)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal sin IVA : ", String.Format("{0:#,0.00}", _factura.Subtotal)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal IVA :", "0,00"), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("TOTAL : ", String.Format("{0:#,0.00}", _factura.Total)), false));
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
                _logger.LogError("Error " + ex.Message);
                _logger.LogError("Error " + ex.StackTrace);
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