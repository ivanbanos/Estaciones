
using EnviadorInformacionService;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturadorEstacionesRepositorio;
using ReporteFacturas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacion
{
    public class EnviadorDeInformacion : IEnviadorDeInformacion
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly IApiContabilidad _apiContabilidad;
        private readonly Guid estacionFuente;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public EnviadorDeInformacion()
        {
            _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            _conexionEstacionRemota = new ConexionEstacionRemota();
            _apiContabilidad = new ApiContabilidad();
            estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
        }

        public void EnviarInformacion()
        {
            while (true)
            {
                try
                {
                    EnviarFacturas();
                    Thread.Sleep(300000);
                }
                catch (Exception ex)
                {

                    Logger.Error("Ex" + ex.Message);
                    Logger.Error("Ex" + ex.StackTrace);
                    Thread.Sleep(300000);
                }
            }
        }



        private void EnviarFacturas()
        {
            string token = _conexionEstacionRemota.getToken();

            var ResolucionesRemota = _conexionEstacionRemota.GetResolucionEstacion(estacionFuente, token);
            var resolucion = _estacionesRepositorio.BuscarResolucionActiva(ResolucionesRemota);
            

            var terceros = _estacionesRepositorio.BuscarTercerosNoEnviados();
            if(terceros.Any(t => t.identificacion != null))
            {
                terceros = terceros.Where(t => t.identificacion != null).ToList();

                Logger.Info($"Subiendo {terceros.Count} terceros");
                var okTercero = _conexionEstacionRemota.EnviarTerceros(terceros, token);
                if (okTercero)
                {
                    _estacionesRepositorio.ActuralizarTercerosEnviados(terceros.Select(x => x.terceroId));
                }
                else
                {

                    Logger.Info("No subieron tereceros");
                }
            }
            var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadas();
            if (facturas.Any()) {
                var formas = _estacionesRepositorio.BuscarFormasPagos();

                var okFacturas = _conexionEstacionRemota.EnviarFacturas(facturas, formas, estacionFuente, token);
                if (okFacturas)
                {
                    _estacionesRepositorio.ActuralizarFacturasEnviados(facturas.Select(x => x.ventaId));
                }
                else
                {

                    Logger.Info("No subieron facturas");
                }
            }
            


            var facturasFechas = _estacionesRepositorio.BuscarFechasReportesNoEnviadas();
            if (facturasFechas.Any())
            {

                var okFacturasFechas = _conexionEstacionRemota.AgregarFechaReporteFactura(facturasFechas, estacionFuente, token);
                if (okFacturasFechas)
                {
                    _estacionesRepositorio.ActuralizarFechasReportesEnviadas(facturasFechas.Select(x => x.IdVentaLocal));
                }
                else
                {

                    Logger.Warn("No subieron facturas");
                }
            }

            var tercerosRecibidos = _conexionEstacionRemota.RecibirTercerosActualizados(estacionFuente,token);
            foreach (var tercero in tercerosRecibidos)
            {

                _estacionesRepositorio.ActuralizarTerceros(tercero);

                Logger.Info($"Tercero {tercero.identificacion} agregado");
            }
            var facturasIdImprimir = _conexionEstacionRemota.RecibirFacturasImprimir(estacionFuente, token);
            var ordenesIdImprimir = _conexionEstacionRemota.RecibirOrdenesImprimir(estacionFuente, token);
            
            
            foreach (var orden in ordenesIdImprimir)
            {
                _estacionesRepositorio.MandarImprimir(orden.IdVentaLocal);
            }
            foreach (var factura in facturasIdImprimir)
            {
                _estacionesRepositorio.MandarImprimir(factura.IdVentaLocal);
            }

        }
         }

}

