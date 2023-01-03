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

namespace ManejadorSurtidor
{
    public class ObtenerVehiculosWorker : BackgroundService
    {
        private readonly Logger _logger;
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;

        private readonly ISicomConection _sicomConection;
        public ObtenerVehiculosWorker(ILogger<ObtenerVehiculosWorker> logger, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _logger = NLog.LogManager.GetCurrentClassLogger(); ;
            _sicomConection = sicomConection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           //ObtenerVehiculos
           //Sino abrir SUIC.txt
           //Actualizar vehiculos en bd
        }


    }
}