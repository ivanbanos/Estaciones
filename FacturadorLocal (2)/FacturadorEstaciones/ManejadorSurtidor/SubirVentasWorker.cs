using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FacturadorEstacionesRepositorio;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Microsoft.Extensions.Options;
using ManejadorSurtidor.SICOM;
using System;

namespace ManejadorSurtidor
{
    public class SubirVentasWorker : BackgroundService
    {
        private readonly Logger _logger;
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;

        private readonly ISicomConection _sicomConection;
        public SubirVentasWorker(ILogger<SubirVentasWorker> logger, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _logger = NLog.LogManager.GetCurrentClassLogger(); ;
            _sicomConection = sicomConection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var ventas = _estacionesRepositorio.getVentaSinSubirSICOM();
                if(ventas.Any(x=>x.IButton != ""))
                {

                    foreach (var venta in ventas.Where(x => x.IButton != ""))
                    {
                        try
                        {

                            await _sicomConection.enviarVenta(venta.IButton, (float)venta.Cantidad);
                            _estacionesRepositorio.actualizarVentaSubidaSicom(venta.ventaId);
                        }
                        catch (Exception ex) { }
                    }
                }
                await Task.Delay(300000, stoppingToken);
            }
        }


    }
}