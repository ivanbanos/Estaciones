using EnviadorInformacionService;
using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturadorEstacionesRepositorio;
using ReporteFacturas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Web.Http;

namespace FacturadorEstacionesAPI.Controllers
{
    /// <summary>
    /// Controller to get data from Reeltime source
    /// </summary>
    [Route("api/[controller]")]
    public class VentasController : ApiController
    {

        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly ConexionEstacionRemota _conexionEstacionRemota;
        private string token;
        private readonly InfoEstacion _infoEstacion;
        private string firstMacAddress;
        private readonly bool generaFacturaElectronica;
        private readonly Guid estacionFuente;
        private bool impresionFormaDePagoOrdenDespacho = false;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="estacionesRepositorio"></param>
        public VentasController()
        {

            estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            impresionFormaDePagoOrdenDespacho = bool.Parse(ConfigurationManager.AppSettings["impresionFormaDePagoOrdenDespacho"]);
            generaFacturaElectronica = bool.Parse(ConfigurationManager.AppSettings["GeneraFacturaElectronica"].ToString());
            firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();

            _estacionesRepositorio = new EstacionesRepositorioSqlServer();

            _infoEstacion = new InfoEstacion()
            {
                CaracteresPorPagina = int.Parse(ConfigurationManager.AppSettings["CaracteresPorPagina"].ToString()),
                Direccion = ConfigurationManager.AppSettings["Direccion"].ToString(),
                Linea1 = ConfigurationManager.AppSettings["Linea1"].ToString(),
                Linea2 = ConfigurationManager.AppSettings["Linea2"].ToString(),
                NIT = ConfigurationManager.AppSettings["NIT"].ToString(),
                Nombre = ConfigurationManager.AppSettings["Nombre"].ToString(),
                Razon = ConfigurationManager.AppSettings["Razon"].ToString(),
                Linea3 = ConfigurationManager.AppSettings["Linea3"].ToString(),
                Linea4 = ConfigurationManager.AppSettings["Linea4"].ToString(),

                Telefono = ConfigurationManager.AppSettings["Telefono"].ToString(),
                vecesPermitidasImpresion = int.Parse(ConfigurationManager.AppSettings["vecesPermitidasImpresion"].ToString())
            };
            _conexionEstacionRemota = new ConexionEstacionRemota();

            //token = _conexionEstacionRemota.getToken();
        }

        [HttpGet]
        [Route("api/Ventas/GetCaras")]
        public List<Cara> GetCaras()
        {
            var caras = _estacionesRepositorio.getCaras();
            return caras;
        }

        [HttpGet]
        [Route("api/Ventas/GetIsTerceroValidoPorIdentificacion/{identificacion}")]
        public bool GetIsTerceroValidoPorIdentificacion(string identificacion)
        {
            try
            {

                var isValid = _conexionEstacionRemota.GetIsTerceroValidoPorIdentificacion(identificacion, token);
                return isValid;
            }
            catch (Exception)
            {
                token = _conexionEstacionRemota.getToken(); 
                var isValid = _conexionEstacionRemota.GetIsTerceroValidoPorIdentificacion(identificacion, token);
                return isValid;
            }
        }

        [HttpGet]
        [Route("api/Ventas/GetInfoFacturaElectronica/{idVentaLocal}/estacion/{estacion}")]
        public string GetInfoFacturaElectronica(int idVentaLocal, Guid estacion)
        {
            try
            {

                var isValid = _conexionEstacionRemota.GetInfoFacturaElectronica(idVentaLocal, estacion, token);
                return isValid;
            }
            catch (Exception)
            {
                token = _conexionEstacionRemota.getToken();
                var isValid = _conexionEstacionRemota.GetInfoFacturaElectronica(idVentaLocal, estacion, token);
                return isValid;
            }
        }

        [HttpGet]
        [Route("api/Ventas/ObtenerFacturaPorIdVentaLocal/{idVentaLocal}")]
        public Guid ObtenerFacturaPorIdVentaLocal(int idVentaLocal)
        {
            try
            {

                var guid = _conexionEstacionRemota.ObtenerFacturaPorIdVentaLocal(idVentaLocal, token);
                return guid;
            }
            catch (Exception)
            {
                token = _conexionEstacionRemota.getToken();
                var guid = _conexionEstacionRemota.ObtenerFacturaPorIdVentaLocal(idVentaLocal, token);
                return guid;
            }
        }

        [HttpGet]
        [Route("api/Ventas/ObtenerOrdenDespachoPorIdVentaLocal/{idVentaLocal}")]
        public Guid ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal)
        {
            try
            {

                var guid = _conexionEstacionRemota.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, token);
                return guid;
            }
            catch (Exception)
            {
                token = _conexionEstacionRemota.getToken();
                var guid = _conexionEstacionRemota.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, token);
                return guid;
            }
        }

        [HttpGet]
        [Route("api/Ventas/GetTerceros")]
        public Tercero GetTerceros(string identificacion)
        {
            var tercero = _estacionesRepositorio.getTercero(identificacion);
            return tercero;
        }

        [HttpGet]
        [Route("api/Ventas/GetIslas")]
        public List<Isla> GetIslas()
        {
            var islas = _estacionesRepositorio.getIslas();
            return islas;
        }

        [HttpGet]
        [Route("api/Ventas/GetTipoIdentificaciones")]
        public List<TipoIdentificacion> GetTIpoIdentificaciones()
        {
            var tipos = _estacionesRepositorio.getTiposIdentifiaciones();
            return tipos;
        }


        [HttpGet]
        [Route("api/Ventas/GetFactura")]
        public Factura GetFactura(short COD_CAR)
        {
            var _factura = _estacionesRepositorio.getUltimasFacturas(COD_CAR, 1).FirstOrDefault();
            _factura.codigoFormaPago = _factura.Venta.COD_FOR_PAG;
            return _factura;
        }

        [HttpGet]
        [Route("api/Ventas/GetFormasPago")]
        public List<FormasPagos> GetFormasPago()
        {
            var formas = _estacionesRepositorio.BuscarFormasPagos();
            return formas;
        }

        [HttpPost]
        [Route("api/Ventas/Trama")]
        public string sendTrama(string trama)
        {
            return send_cmd(trama+"*");
        }

        [HttpPost]
        [Route("api/Ventas/EnviarFacturacion")]
        public bool EnviarFacturacion(int ventaId)
        {
            _estacionesRepositorio.enviarFacturacionSiigo(ventaId);
            return true;
        }


        [HttpPost]
        [Route("api/Ventas/ConvertirAFactura")]
        public bool ConvertirAFactura(int ventaId)
        {
            
                _estacionesRepositorio.ConvertirAFactura(ventaId);
            
            return true;
        }
        [HttpPost]
        [Route("api/Ventas/ConvertirAOrden")]
        public bool ConvertirAOrden(int ventaId)
        {
                _estacionesRepositorio.ConvertirAOrden(ventaId);
            
            return true;
        }


        public string send_cmd(string szData)
        {
            Socket m_socClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                var puerto = ConfigurationManager.AppSettings["Puerto"].ToString();
                var ip = ConfigurationManager.AppSettings["Ip"].ToString();

                int alPort = System.Convert.ToInt16(puerto, 10);
                System.Net.IPAddress remoteIPAddress = System.Net.IPAddress.Parse(ip);
                System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddress, alPort);
                m_socClient.Connect(remoteEndPoint);

                byte[] byData = System.Text.Encoding.ASCII.GetBytes(szData);
                m_socClient.Send(byData);
                byte[] b = new byte[100];
                m_socClient.Receive(b);
                string szReceived = Encoding.ASCII.GetString(b);
                m_socClient.Close();
                m_socClient.Dispose();
                return szReceived;
            }
            catch (Exception)
            {
                m_socClient.Close();
                m_socClient.Dispose();
                return "Error";
            }
            //Dispose();
        }

        private Font printFont;
        private int _charactersPerPage;
        private Factura _factura;
        private Venta _venta;
        private Tercero _tercero;
        private Cara _cara;
        private Manguera _mangueras;

        [HttpPost]
        [Route("api/Ventas/ImprimirFactura")]
        public string ImprimirFactura( Factura factura)
        {
            try
            {

                _factura = factura;
                _venta = _factura.Venta;
                _tercero = _factura.Tercero;
                _mangueras = _factura.Manguera;
                _charactersPerPage = _infoEstacion.CaracteresPorPagina;
                _cara = _estacionesRepositorio.getCaras().Single(x => x.COD_CAR == _venta.COD_CAR);
                _factura.Tercero = _tercero = _estacionesRepositorio.crearTercero(_tercero.terceroId, new TipoIdentificacion() { TipoIdentificacionId = _tercero.tipoIdentificacion.Value }, _tercero.identificacion, _tercero.Nombre, _tercero.Telefono, _tercero.Correo, _tercero.Direccion, _venta.COD_CLI);

                _estacionesRepositorio.ActualizarFactura(_factura.facturaPOSId, _factura.Placa, _factura.Kilometraje, _factura.codigoFormaPago, _tercero.terceroId, _factura.ventaId);

                int vecesImprimir = System.Convert.ToInt16(ConfigurationManager.AppSettings["vecesImprimir"].ToString(), 10);
                for (int i = 0; i < vecesImprimir; i++)
                {

                    _estacionesRepositorio.MandarImprimir(factura.ventaId);
                }
                return "Ok";
            }catch(Exception ex)
            {
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
                return "Error";
            }
            
        }

        [HttpPost]
        [Route("api/Ventas/ImprimirFactura/{imprimir}")]
        public string ImprimirFactura(bool imprimir, Factura factura)
        {

            _factura = factura;
            _venta = _factura.Venta;
            _tercero = _factura.Tercero;
            _mangueras = _factura.Manguera;
            _charactersPerPage = _infoEstacion.CaracteresPorPagina;
            _cara = _estacionesRepositorio.getCaras().Single(x => x.COD_CAR == _venta.COD_CAR);
            _factura.Tercero = _tercero = _estacionesRepositorio.crearTercero(_tercero.terceroId, new TipoIdentificacion() { TipoIdentificacionId = _tercero.tipoIdentificacion.Value }, _tercero.identificacion, _tercero.Nombre, _tercero.Telefono, _tercero.Correo, _tercero.Direccion, _venta.COD_CLI);

            _estacionesRepositorio.ActualizarFactura(_factura.facturaPOSId, _factura.Placa, _factura.Kilometraje, _factura.codigoFormaPago, _tercero.terceroId, _factura.ventaId);

            if (imprimir)
            {
                int vecesImprimir = System.Convert.ToInt16(ConfigurationManager.AppSettings["vecesImprimir"].ToString(), 10);
                for (int i = 0; i < vecesImprimir; i++)
                {

                    _estacionesRepositorio.MandarImprimir(factura.ventaId);
                }
                return "Ok";
            }
            else
            {
                return getLineasImprimir(_factura);
            }
        }

        private string getLineasImprimir(Factura factura)
        {
            var
            formas = _estacionesRepositorio.BuscarFormasPagos();
            _charactersPerPage = _infoEstacion.CaracteresPorPagina;
            if (_factura.codigoFormaPago == 0)
            {
                _factura.codigoFormaPago = _venta.COD_FOR_PAG;
            }
            if (_charactersPerPage == 0)
            {
                _charactersPerPage = 40;
            }
            var lineasImprimir = new StringBuilder();
            var guiones = new StringBuilder();
            guiones.Append('-', _charactersPerPage);
            // Iterate over the file, printing each line.
            lineasImprimir.Append(".");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(_infoEstacion.Razon);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append("NIT " + _infoEstacion.NIT);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(_infoEstacion.Nombre);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(_infoEstacion.Direccion);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(_infoEstacion.Telefono);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(guiones.ToString());
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

                lineasImprimir.Append("Factura Electrónica" + facturaElectronica[2]);
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(facturaElectronica[3]);
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(facturaElectronica[4]);
                lineasImprimir.Append("\n\r ");
            }
            else if (_factura.Consecutivo == 0)
            {

                lineasImprimir.Append("Orden de despacho No: " + _venta.CONSECUTIVO);
                lineasImprimir.Append("\n\r ");
            }
            else
            {
                lineasImprimir.Append("Factura de venta P.O.S No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo);
                lineasImprimir.Append("\n\r ");
            }

            lineasImprimir.Append(guiones.ToString());
            var placa = (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _venta.PLACA + "").Trim();
            if (_venta.COD_FOR_PAG != 4)
            {

                lineasImprimir.Append(formatoTotales("Vendido a : ", _tercero.Nombre == null ? "" : _tercero.Nombre.Trim()));
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(formatoTotales("Nit/C.C. : ", _tercero.identificacion.Trim()));
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(formatoTotales("Placa : ", placa));
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : _venta.KILOMETRAJE + "").Trim()));
                lineasImprimir.Append("\n\r ");
                var codigoInterno = _factura.Venta.COD_INT != null ? _factura.Venta.COD_INT : _estacionesRepositorio.ObtenerCodigoInterno(placa, _tercero.identificacion.Trim());
                if (codigoInterno != null)
                {
                    lineasImprimir.Append(formatoTotales("Cod Int : ", codigoInterno));
                    lineasImprimir.Append("\n\r ");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_tercero.Nombre))
                {
                    lineasImprimir.Append(formatoTotales("Vendido a :", " CONSUMIDOR FINAL".Trim()));
                    lineasImprimir.Append("\n\r ");
                }
                else
                {
                    lineasImprimir.Append(formatoTotales("Vendido a : ", _tercero.Nombre.Trim()) + "");
                    lineasImprimir.Append("\n\r ");
                }
                if (string.IsNullOrEmpty(_tercero.identificacion))
                {
                    lineasImprimir.Append(formatoTotales("Nit/C.C. : ", "222222222222".Trim()));
                    lineasImprimir.Append("\n\r ");
                }
                else
                {
                    lineasImprimir.Append(formatoTotales("Nit/C.C. : ", _tercero.identificacion.Trim()));
                    lineasImprimir.Append("\n\r ");
                }
                lineasImprimir.Append(formatoTotales("Placa : ", (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _venta.PLACA + "").Trim()));
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : _venta.KILOMETRAJE + "").Trim()));
                lineasImprimir.Append("\n\r ");
                var codigoInterno = _factura.Venta.COD_INT != null ? _factura.Venta.COD_INT : _estacionesRepositorio.ObtenerCodigoInterno(placa, _tercero.identificacion.Trim());
                if (codigoInterno != null)
                {
                    lineasImprimir.Append(formatoTotales("Cod Int : ", codigoInterno));
                    lineasImprimir.Append("\n\r ");
                }
            }

            if (_venta.FECH_PRMA.HasValue && (_mangueras.DESCRIPCION.ToLower().Contains("gn") || _mangueras.DESCRIPCION.ToLower().Contains("gas")))
            {
                lineasImprimir.Append(formatoTotales("Proximo mantenimiento : ", _venta.FECH_PRMA.Value.ToString("dd/MM/yyyy").Trim()));
                lineasImprimir.Append("\n\r ");
            }

            lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("Fecha : ", _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss")));
            lineasImprimir.Append("\n\r ");

            lineasImprimir.Append(formatoTotales("Surtidor : ", _venta.COD_SUR + ""));
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("Cara : ", _venta.COD_CAR + ""));
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("Manguera : ", _mangueras.COD_MAN + ""));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(formatoTotales("Vendedor : ", _venta.EMPLEADO.Trim() + ""));
            lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append("\n\r ");

            lineasImprimir.Append($"Producto: {_mangueras.DESCRIPCION.Trim()}");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append($"Cantidad: {string.Format("{0:#,0.000}", _venta.CANTIDAD)}");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append($"Precio: {_venta.PRECIO_UNI.ToString("F")}");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append($"Total: {_venta.VALORNETO}");

            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(guiones.ToString() + "");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append("DISCRIMINACION TARIFAS IVA" + "");
            lineasImprimir.Append("\n\r ");

            lineasImprimir.Append($"Producto: {_mangueras.DESCRIPCION.Trim()}");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append($"Cantidad: {string.Format("{0:#,0.000}", _venta.CANTIDAD)}");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append($"Tafira: 0 % ");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append($"Total: {_venta.VALORNETO}");
            lineasImprimir.Append("\n\r ");

            lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("Descuento: ", String.Format("{0:#,0.00}", _venta.Descuento)));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append(formatoTotales("Subtotal sin IVA : ", String.Format("{0:#,0.00}", _venta.TOTAL)));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append(formatoTotales("Subtotal IVA :", "0,00"));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append(formatoTotales("TOTAL : ", String.Format("{0:#,0.00}", _venta.TOTAL)));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(guiones.ToString());

            if (_factura.Consecutivo != 0 || impresionFormaDePagoOrdenDespacho)
            {
                if (formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago) != null)
                {
                    lineasImprimir.Append(formatoTotales("Forma de pago : ", formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago).Descripcion.Trim()));
                    lineasImprimir.Append("\n\r ");

                }
                else
                {
                    lineasImprimir.Append(formatoTotales("Forma de pago :", " Efectivo"));
                    lineasImprimir.Append("\n\r ");

                }
            }

            if (!string.IsNullOrEmpty(infoTemp))
            {
                try
                {
                    var resoluconElectronica = _conexionEstacionRemota.GetResolucionElectronica(_conexionEstacionRemota.getToken());

                    lineasImprimir.Append(guiones.ToString());
                    lineasImprimir.Append("\n\r ");
                    lineasImprimir.Append(resoluconElectronica.invoiceText);
                    lineasImprimir.Append("\n\r ");
                }
                catch (Exception)
                {
                    lineasImprimir.Append(guiones.ToString());
                    lineasImprimir.Append("\n\r ");
                    lineasImprimir.Append("Modalidad Factura Electrónica ");
                    lineasImprimir.Append("\n\r ");
                }

            }

            else if (_factura.Consecutivo != 0)
            {


                lineasImprimir.Append(guiones.ToString());
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("Resolucion de Facturacion No. ");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(_factura.Autorizacion + " de " + _factura.FechaInicioResolucion.ToString("dd/MM/yyyy") + " ");
                lineasImprimir.Append("\n\r ");
                var numeracion = "Numeracion Autorizada por la DIAN";
                if (_factura.habilitada)
                {
                    numeracion = "Numeracion Habilitada por la DIAN";
                }
                lineasImprimir.Append(numeracion + " ");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("Del " + _factura.DescripcionResolucion + "-" + _factura.Inicio + " al " + _factura.DescripcionResolucion + "-" + _factura.Final + "");
                lineasImprimir.Append("\n\r ");

            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea1))
            {
                lineasImprimir.Append(_infoEstacion.Linea1);
                lineasImprimir.Append("\n\r ");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea2))
            {
                lineasImprimir.Append(_infoEstacion.Linea2);
                lineasImprimir.Append("\n\r ");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea3))
            {
                lineasImprimir.Append(_infoEstacion.Linea3);
                lineasImprimir.Append("\n\r ");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea4))
            {
                lineasImprimir.Append(_infoEstacion.Linea4);
                lineasImprimir.Append("\n\r ");
            }
            lineasImprimir.Append("Fabricado por:" + " SIGES SOLUCIONES SAS ");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append("Nit:" + " 901430393-2 ");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append("Nombre:" + " Facturador SIGES ");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("SERIAL MAQUINA: ", firstMacAddress));
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(".");

            return lineasImprimir.ToString();
        }

        [HttpPost]
        [Route("api/Ventas/EnviarFacturaElectronica")]
        public string EnviarFacturaElectronica(Factura factura)
        {
            try
            {

                try
                {
                    var formas = _estacionesRepositorio.BuscarFormasPagos();
                    _conexionEstacionRemota.EnviarFacturas(new List<Factura>() { factura }, formas, token);

                    if (factura.Consecutivo == 0)
                    {
                        var guid = _conexionEstacionRemota.ObtenerOrdenDespachoPorIdVentaLocal(factura.ventaId, token);
                        var result = _conexionEstacionRemota.CrearFacturaOrdenesDeDespacho(guid.ToString(), token);
                        _estacionesRepositorio.ActuralizarFacturasEnviados(new List<int>() { factura.ventaId });
                        return result;
                    }
                    else
                    {
                        var guid = _conexionEstacionRemota.ObtenerFacturaPorIdVentaLocal(factura.ventaId, token);
                        var result = _conexionEstacionRemota.CrearFacturaFacturas(guid.ToString(), token);
                        _estacionesRepositorio.ActuralizarFacturasEnviados(new List<int>() { factura.ventaId });
                        return result;
                    }
                }
                catch (Exception)
                {
                    token = _conexionEstacionRemota.getToken();
                    var formas = _estacionesRepositorio.BuscarFormasPagos();
                    _conexionEstacionRemota.EnviarFacturas(new List<Factura>() { factura }, formas, token);

                    if (factura.Consecutivo == 0)
                    {
                        var guid = _conexionEstacionRemota.ObtenerOrdenDespachoPorIdVentaLocal(factura.ventaId, token);
                        return _conexionEstacionRemota.CrearFacturaOrdenesDeDespacho(guid.ToString(), token);

                    }
                    else
                    {
                        var guid = _conexionEstacionRemota.ObtenerFacturaPorIdVentaLocal(factura.ventaId, token);
                        return _conexionEstacionRemota.CrearFacturaFacturas(guid.ToString(), token);
                    }
                }
            } catch(Exception e)
            {
                return e.StackTrace;
            }
        }

        [HttpPost]
        [Route("api/Ventas/EnviarFactura")]
        public bool EnviarFactura(Factura factura)
        {
            try
            {
                var formas = _estacionesRepositorio.BuscarFormasPagos();
                return _conexionEstacionRemota.EnviarFacturas(new List<Factura>() { factura }, formas, token);
            }
            catch (Exception)
            {
                token = _conexionEstacionRemota.getToken();
                var formas = _estacionesRepositorio.BuscarFormasPagos();
                return _conexionEstacionRemota.EnviarFacturas(new List<Factura>() { factura }, formas, token);
            }
        }

        [HttpGet]
        [Route("api/Ventas/GetCanastilla")]
        public IEnumerable<Canastilla> GetCanastilla()
        {
            try
            {
                var canastillas = _conexionEstacionRemota.RecibirCanastilla(token);

                _estacionesRepositorio.ActualizarCanastilla(canastillas);
            }
            catch (Exception ex)
            {

                try
                {

                    token = _conexionEstacionRemota.getToken();
                    var canastillas = _conexionEstacionRemota.RecibirCanastilla(token);

                    _estacionesRepositorio.ActualizarCanastilla(canastillas);
                }
                catch (Exception exe)
                {
                    
                }
            }
            
            return _estacionesRepositorio.GetCanastillas();
        }


        [HttpPost]
        [Route("api/Ventas/GenerarFacturaCanastilla/{imprimir}")]
        public string GenerarFacturaCanastilla(bool imprimir, FacturaCanastilla factura)
        {
            try
            {

                if (factura.terceroId.terceroId == -1)
                {

                    factura.terceroId = _estacionesRepositorio.crearTercero(0, new TipoIdentificacion() { TipoIdentificacionId = 1 }, _tercero.identificacion, "No identificado", "No identificado", "No identificado", "No identificado", "");

                }
                return  _estacionesRepositorio.GenerarFacturaCanastilla(factura, imprimir)+"";

            }
            catch (Exception ex)
            {
                return ex.StackTrace;
            }

        }


        [HttpPost]
        [Route("api/Ventas/GenerarFacturaCanastilla")]
        public int GenerarFacturaCanastilla(FacturaCanastilla factura)
        {
            if (factura.terceroId.terceroId == -1)
            {

                factura.terceroId = _estacionesRepositorio.crearTercero(0, new TipoIdentificacion() { TipoIdentificacionId = 1 }, _tercero.identificacion, "No identificado", "No identificado", "No identificado", "No identificado", "");

            }
            return _estacionesRepositorio.GenerarFacturaCanastilla(factura, true);

        }


        [HttpGet]
        [Route("api/Ventas/getFacturaCanastilla/{consecutivo}")]
        public string getFacturaCanastilla(int consecutivo)
        {
            try
            {
                var facturaReal = _estacionesRepositorio.BuscarFacturaCanastillaPorConsecutivo(consecutivo);

                return getLineasImprimirCanastilla(facturaReal);
            } catch(Exception ex)
            {
                return ex.StackTrace;
            }
        }

        private string getLineasImprimirCanastilla(FacturaCanastilla _factura)
        {
            var formasDePago = _estacionesRepositorio.BuscarFormasPagos();
            _charactersPerPage = _infoEstacion.CaracteresPorPagina;

            if (_charactersPerPage == 0)
            {
                _charactersPerPage = 30;
            }
            var lineasImprimir = new StringBuilder();
            var guiones = new StringBuilder();
            guiones.Append('-', _charactersPerPage);
            // Iterate over the file, printing each line.
            lineasImprimir.Append(".");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(_infoEstacion.Razon);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append("NIT " + _infoEstacion.NIT);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(_infoEstacion.Nombre);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(_infoEstacion.Direccion);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(_infoEstacion.Telefono);
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(guiones.ToString());
            var infoTemp = "";
            if (!string.IsNullOrEmpty(infoTemp))
            {
                infoTemp = infoTemp.Replace("\n\r", " ");

                var facturaElectronica = infoTemp.Split(' ');

                lineasImprimir.Append("Factura Electrónica" + facturaElectronica[2]);
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(facturaElectronica[3]);
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(facturaElectronica[4]);
                lineasImprimir.Append("\n\r ");
            }

            lineasImprimir.Append("Factura de venta P.O.S No: " + _factura.resolucion.DescripcionResolucion + "-" + _factura.consecutivo);

            lineasImprimir.Append("\n\r ");

            lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("Vendido a : ", _factura.terceroId.Nombre == null ? "" : _factura.terceroId.Nombre.Trim()));

            lineasImprimir.Append("\n\r "); 
            lineasImprimir.Append(formatoTotales("Nit/C.C. : ", _factura.terceroId.identificacion.Trim()));
            lineasImprimir.Append("\n\r ");



            lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("Fecha : ", _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss")));
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append("\n\r ");

            foreach (var canastilla in _factura.canastillas)
                {
                    lineasImprimir.Append($"Producto: {canastilla.Canastilla.descripcion.Trim()}");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append($"Cantidad: {string.Format("{0:#,0.000}", canastilla.cantidad)}");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append($"Precio: {canastilla.precioBruto.ToString("F")}");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append($"Subtotal: {canastilla.subtotal}");
                lineasImprimir.Append("\n\r ");
            }

            lineasImprimir.Append(guiones.ToString() + "");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append("DISCRIMINACION TARIFAS IVA" + "");
            lineasImprimir.Append("\n\r ");
            //  lineasImprimir.Append(guiones.ToString() + "");

            foreach (var canastilla in _factura.canastillas)
                {
                    lineasImprimir.Append($"Producto: {canastilla.Canastilla.descripcion.Trim()}");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append($"Cantidad: {string.Format("{0:#,0.000}", canastilla.cantidad)}");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append($"Iva: {canastilla.iva}  ");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append($"Total: {canastilla.total}");
                lineasImprimir.Append("\n\r ");
            }
            
            lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("Descuento: ", String.Format("{0:#,0.00}", _factura.descuento)));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append(formatoTotales("Subtotal sin IVA : ", String.Format("{0:#,0.00}", _factura.subtotal)));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append(formatoTotales("Subtotal IVA :", $"{_factura.iva}"));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(guiones.ToString());
            lineasImprimir.Append(formatoTotales("TOTAL : ", String.Format("{0:#,0.00}", _factura.total)));
            lineasImprimir.Append("\n\r ");
            //lineasImprimir.Append(guiones.ToString());

            var forma = formasDePago.FirstOrDefault(x => x.Id == _factura.codigoFormaPago.Id);
            lineasImprimir.Append(formatoTotales("Forma de pago : ", forma?.Descripcion?.Trim()));
            lineasImprimir.Append("\n\r ");


            if (!string.IsNullOrEmpty(infoTemp))
            {
                lineasImprimir.Append(guiones.ToString());
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("Resolucion de Facturacion No. ");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("18764013579016 de 2021-05-24");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("Modalidad Factura Electrónica ");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("Desde N° FEE1 hasta FEE1000000");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("Vigencia hasta 2021-11-24");
                lineasImprimir.Append("\n\r ");

            }

            else if (_factura.consecutivo != 0)
            {


                lineasImprimir.Append(guiones.ToString());
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("Resolucion de Facturacion No. ");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append(_factura.resolucion.Autorizacion + " de " + _factura.resolucion.FechaInicioResolucion.ToString("dd/MM/yyyy") + " ");

                lineasImprimir.Append("\n\r "); 
                var numeracion = "Numeracion Autorizada por la DIAN";
                if (_factura.resolucion.Habilitada)
                {
                    numeracion = "Numeracion Habilitada por la DIAN";
                }
                lineasImprimir.Append(numeracion + " ");
                lineasImprimir.Append("\n\r ");
                lineasImprimir.Append("Del " + _factura.resolucion.DescripcionResolucion + "-" + _factura.resolucion.ConsecutivoInicial + " al " + _factura.resolucion.DescripcionResolucion + "-" + _factura.resolucion.ConsecutivoFinal + "");

                lineasImprimir.Append("\n\r ");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea1))
            {
                lineasImprimir.Append(_infoEstacion.Linea1);
                lineasImprimir.Append("\n\r ");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea2))
            {
                lineasImprimir.Append(_infoEstacion.Linea2);
                lineasImprimir.Append("\n\r ");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea3))
            {
                lineasImprimir.Append(_infoEstacion.Linea3);
                lineasImprimir.Append("\n\r ");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea4))
            {
                lineasImprimir.Append(_infoEstacion.Linea4);
                lineasImprimir.Append("\n\r ");
            }
            lineasImprimir.Append("Fabricado por:" + " SIGES SOLUCIONES SAS ");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append("Nit:" + " 901430393-2 ");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append("Nombre:" + " Facturador SIGES ");
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(formatoTotales("SERIAL MAQUINA: ", firstMacAddress));
            lineasImprimir.Append("\n\r ");
            lineasImprimir.Append(".");
            lineasImprimir.Append("\n\r ");
            return lineasImprimir.ToString();
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
    }
}
