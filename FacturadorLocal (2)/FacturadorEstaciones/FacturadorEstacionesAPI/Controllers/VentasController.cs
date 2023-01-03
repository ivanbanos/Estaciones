using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FactoradorEstacionesModelo.Objetos;
using FacturadorEstacionesAPI.Filters;
using FacturadorEstacionesRepositorio;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReporteFacturas;

namespace FacturadorEstacionesAPI.Controllers
{
    /// <summary>
    /// Controller to get data from Reeltime source
    /// </summary>
    [ApiController]
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ApiExceptionFilterAttribute))]
    public class VentasController : ControllerBase
    {

        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly ILogger<VentasController> _logger;
        private readonly InfoEstacion _infoEstacion;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="estacionesRepositorio"></param>
        public VentasController(ILogger<VentasController> logger
            , IEstacionesRepositorio estacionesRepositorio, IOptions<InfoEstacion> infoEstacion)
        {
            _estacionesRepositorio = estacionesRepositorio ?? throw new ArgumentNullException(nameof(estacionesRepositorio));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _infoEstacion = infoEstacion.Value;
        }

        [HttpGet]
        [Route("GetCaras")]
        [ProducesResponseType(typeof(IEnumerable<Cara>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCaras()
        {
            var caras = _estacionesRepositorio.getCaras();
            return Ok(caras);
        }

        [HttpGet]
        [Route("GetTipoIdentificaciones")]
        [ProducesResponseType(typeof(IEnumerable<TipoIdentificacion>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTIpoIdentificaciones()
        {
            var tipos = _estacionesRepositorio.getTiposIdentifiaciones();
            return Ok(tipos);
        }


        [HttpGet]
        [Route("GetFactura")]
        [ProducesResponseType(typeof(Factura), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFactura(short COD_CAR)
        {
            var _factura = _estacionesRepositorio.getUltimasFacturas(COD_CAR, 1).FirstOrDefault();
            return Ok(_factura);
        }

        private Font printFont;
        private int _charactersPerPage;
        private Factura _factura;
        private Venta _venta;
        private Tercero _tercero;
        private Cara _cara;
        private Manguera _mangueras;

        [HttpPost]
        [Route("ImprimirFactura")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImprimirFactura(Factura factura)
        {
            _factura = factura;
            _venta = _factura.Venta;
            _tercero = _factura.Tercero;
            _mangueras = _factura.Manguera;
            _charactersPerPage = _infoEstacion.CaracteresPorPagina;
            _cara = _estacionesRepositorio.getCaras().Single(x=>x.COD_CAR == _venta.COD_CAR);
            
                printFont = new Font("Console", 9);
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                // Print the document.
                if (_venta != null && _venta.IMP_NOM != null)
                {
                    pd.PrinterSettings.PrinterName = _venta.IMP_NOM.Trim();
                }
                try
                {
                    pd.Print();
                }
                catch (Exception ex)
                {

                    printFont = new Font("Console", 9);
                    PrintDocument pd2 = new PrintDocument();
                    pd2.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                    pd2.Print();
                }
            

            return Ok();
        }


        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float yPos = 0;
            int count = 0;
            float leftMargin = 5;
            float topMargin = 10;
            String line = null;
            int sizePaper = ev.PageSettings.PaperSize.Width;
            int fonSizeInches = 72 / 9;
            if (_charactersPerPage == 0)
            {
                _charactersPerPage = fonSizeInches * sizePaper / 100;
            }
            var guiones = new StringBuilder();
            guiones.Append('-', _charactersPerPage);
            // Iterate over the file, printing each line.

            count = printLine(_infoEstacion.Razon, ev, count, leftMargin, topMargin, true);
            count = printLine("NIT " + _infoEstacion.NIT, ev, count, leftMargin, topMargin, true);
            count = printLine(_infoEstacion.Nombre, ev, count, leftMargin, topMargin, true);
            count = printLine(_infoEstacion.Direccion, ev, count, leftMargin, topMargin, true);
            count = printLine(_infoEstacion.Telefono, ev, count, leftMargin, topMargin, true);
            count = printLine(guiones.ToString(), ev, count, leftMargin, topMargin);

            if (_factura.Consecutivo == 0)
            {

                count = printLine("Orden de despacho ", ev, count, leftMargin, topMargin, true);
            }
            else
            {
                count = printLine("Facturación P.O.S No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo, ev, count, leftMargin, topMargin, true);
            }

            count = printLine(guiones.ToString(), ev, count, leftMargin, topMargin);
            if (_venta.COD_FOR_PAG != 4)
            {

                count = printLine("Vendido a : " + _tercero.Nombre, ev, count, leftMargin, topMargin);
                count = printLine("Nit/C.C. : " + _tercero.identificacion, ev, count, leftMargin, topMargin);
                count = printLine("Placa : " + _factura.Placa, ev, count, leftMargin, topMargin);
                count = printLine("Kilometraje : " + _factura.Kilometraje, ev, count, leftMargin, topMargin);
                count = printLine("Cod Int : " + _venta.COD_INT, ev, count, leftMargin, topMargin);
            }
            else
            {
                if (string.IsNullOrEmpty(_tercero.Nombre))
                {
                    count = printLine("Vendido a : CONSUMIDOR FINAL\n\r", ev, count, leftMargin, topMargin);
                }
                else
                {
                    count = printLine("Vendido a : " + _tercero.Nombre + "\n\r", ev, count, leftMargin, topMargin);
                }
                if (string.IsNullOrEmpty(_tercero.identificacion))
                {
                    count = printLine("Nit/C.C. : 222222222222\n\r", ev, count, leftMargin, topMargin);
                }
                else
                {
                    count = printLine("Nit/C.C. : " + _tercero.identificacion + "\n\r", ev, count, leftMargin, topMargin);
                }
                count = printLine("Placa : " + _factura.Placa, ev, count, leftMargin, topMargin);
                count = printLine("Kilometraje : " + _factura.Kilometraje, ev, count, leftMargin, topMargin);
            }

            if (_venta.FECH_ULT_ACTU.HasValue)
            {
                count = printLine("Proximo mantenimiento : " + _venta.FECH_ULT_ACTU.Value.ToString("dd/MM/yyyy") + "\n\r", ev, count, leftMargin, topMargin);
            }
            count = printLine(guiones.ToString(), ev, count, leftMargin, topMargin);
            count = printLine("Fecha : " + _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss"), ev, count, leftMargin, topMargin);

            count = printLine("Surtidor : " + _cara.COD_SUR + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine("Cara : " + _cara.COD_CAR + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine("Manguera : " + _mangueras.COD_MAN + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine(guiones.ToString(), ev, count, leftMargin, topMargin);
            count = printLine(getLienaTarifas("Producto", "Cant.", "Precio", "Total") + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), _venta.CANTIDAD.ToString("F"), _venta.PRECIO_UNI.ToString("F"), _venta.TOTAL.ToString("F")) + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine(guiones.ToString() + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine("DISCRIMINACION TARIFAS IVA" + "\n\r", ev, count, leftMargin, topMargin, true);
            count = printLine(guiones.ToString() + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine(getLienaTarifas("Producto", "Cant.", "Tafira", "Total") + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), _venta.CANTIDAD.ToString("F"), "0%", _venta.TOTAL.ToString("F")) + "\n\r", ev, count, leftMargin, topMargin);
            count = printLine(guiones.ToString(), ev, count, leftMargin, topMargin);
            count = printLine("Subtotal sin IVA : " + _venta.TOTAL.ToString("F"), ev, count, leftMargin, topMargin);
            count = printLine(guiones.ToString(), ev, count, leftMargin, topMargin);
            count = printLine("Subtotal IVA : 0,00", ev, count, leftMargin, topMargin);
            count = printLine(guiones.ToString(), ev, count, leftMargin, topMargin);
            count = printLine("TOTAL : " + _venta.TOTAL.ToString("F"), ev, count, leftMargin, topMargin);
            count = printLine(guiones.ToString(), ev, count, leftMargin, topMargin);
            if (_factura.Consecutivo != 0)
            {
                count = printLine("Resolucion de Facturacion N0. \n\r", ev, count, leftMargin, topMargin);
                count = printLine(_factura.Autorizacion + " de " + _factura.FechaInicioResolucion.ToString("dd/MM/yyyy") + " \n\r", ev, count, leftMargin, topMargin);
                var numeracion = "Numeracion Autorizada por la DIAN";
                if (_factura.habilitada)
                {
                    numeracion = "Numeracion Habilitada por la DIAN";
                }
                count = printLine(numeracion + " \n\r", ev, count, leftMargin, topMargin);
                count = printLine("Del " + _factura.DescripcionResolucion + "-" + _factura.Inicio + " al " + _factura.DescripcionResolucion + "-" + _factura.Final + "\n\r", ev, count, leftMargin, topMargin);
                count = printLine(_infoEstacion.Linea1, ev, count, leftMargin, topMargin, true);
                count = printLine(_infoEstacion.Linea2, ev, count, leftMargin, topMargin, true);
            }
            count = printLine("Fabricado por: SAM SAS ", ev, count, leftMargin, topMargin, true);
            count = printLine("Nit: 900725658-2 ", ev, count, leftMargin, topMargin, true);
            count = printLine("Nombre: Facturador GES ", ev, count, leftMargin, topMargin, true);

            // If more lines exist, print another page.
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
            _tercero = _estacionesRepositorio.crearTercero(_tercero.terceroId, new TipoIdentificacion() { TipoIdentificacionId = _tercero.tipoIdentificacion.Value }, _tercero.identificacion, _tercero.Nombre, _tercero.Telefono, _tercero.Correo, _tercero.Direccion, _venta.COD_CLI);

            _estacionesRepositorio.ActualizarFactura(_factura.facturaPOSId, _factura.Placa, _factura.Kilometraje, _factura.codigoFormaPago);
            _factura.impresa++;
            
        }


        private string getLienaTarifas(string v1, string v2, string v3, string v4)
        {
            var spacesInPage = _charactersPerPage / 4;
            var tabs = new StringBuilder();
            tabs.Append(v1.Substring(0, v1.Length < spacesInPage ? v1.Length : spacesInPage));
            var whitespaces = spacesInPage - v1.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

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
            return tabs.ToString();
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
}
