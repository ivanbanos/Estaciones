using FactoradorEstacionesModelo;
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

namespace SigesServicio
{
    public class FacturasWorker : BackgroundService
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly InfoEstacion _infoEstacion;
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
        public FacturasWorker(IEstacionesRepositorio estacionesRepositorio, IOptions<InformacionCuenta> informacionCuenta, IOptions<InfoEstacion> infoEstacion, IConexionEstacionRemota conexionEstacionRemota, IFidelizacion fidelizacon)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion.Value;
            _informacionCuenta = informacionCuenta.Value;
            _conexionEstacionRemota = conexionEstacionRemota;
            _fidelizacon = fidelizacon;
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
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
        });



        private void EnviarFacturas()
        {
            string token = _conexionEstacionRemota.getToken();

            var ResolucionesRemota = _conexionEstacionRemota.GetResolucionEstacion(Guid.Parse(_infoEstacion.EstacionFuente), token);
            var resolucion = _estacionesRepositorio.BuscarResolucionActiva(ResolucionesRemota);


            var terceros = _estacionesRepositorio.BuscarTercerosNoEnviados();
            if (terceros.Any(t => t.identificacion != null))
            {
                terceros = terceros.Where(t => t.identificacion != null).ToList();

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
            var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadasSiges();
            if (facturas.Any())
            {
                var formas = _estacionesRepositorio.BuscarFormasPagosSiges();

                var okFacturas = _conexionEstacionRemota.EnviarFacturas(facturas, formas, Guid.Parse(_infoEstacion.EstacionFuente), token);
                if (okFacturas)
                {
                    _estacionesRepositorio.ActuralizarFacturasEnviados(facturas.Select(x => x.ventaId));
                }
                else
                {

                    Logger.Info("No subieron facturas");
                }
            }

            var facturasFechas = _estacionesRepositorio.BuscarFechasReportesNoEnviadasSiges();
            if (facturasFechas.Any())
            {

                var okFacturasFechas = _conexionEstacionRemota.AgregarFechaReporteFactura(facturasFechas.Where(x=>x.FechaReporte != null).ToList(), Guid.Parse(_infoEstacion.EstacionFuente), token);
                if (okFacturasFechas)
                {
                    _estacionesRepositorio.ActuralizarFechasReportesEnviadas(facturasFechas.Select(x => x.IdVentaLocal));
                }
                else
                {

                    Logger.Warn("No subieron facturas");
                }
            }

            //var tercerosRecibidos = _conexionEstacionRemota.RecibirTercerosActualizados(Guid.Parse(_infoEstacion.EstacionFuente), token);
            //foreach (var tercero in tercerosRecibidos)
            //{

            //    _estacionesRepositorio.ActuralizarTerceros(tercero);

            //}
            var facturasIdImprimir = _conexionEstacionRemota.RecibirFacturasImprimir(Guid.Parse(_infoEstacion.EstacionFuente), token);
            var ordenesIdImprimir = _conexionEstacionRemota.RecibirOrdenesImprimir(Guid.Parse(_infoEstacion.EstacionFuente), token);


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

