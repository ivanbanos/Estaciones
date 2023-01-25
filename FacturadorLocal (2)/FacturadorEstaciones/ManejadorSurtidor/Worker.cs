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
using ManejadorSurtidor.Messages;

namespace ManejadorSurtidor
{
    public class Worker : BackgroundService
    {
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;

        private readonly ISicomConection _sicomConection;
        private readonly IMessageProducer _messageProducer;

        public Worker(ILogger<Worker> logger, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection, IMessageProducer messageProducer)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _sicomConection = sicomConection;
            _messageProducer = messageProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Log(NLog.LogLevel.Info, "Buscando Caras");
            var surtidores = _estacionesRepositorio.GetSurtidoresSiges();

            _logger.Log(NLog.LogLevel.Info, $"Caras {JsonConvert.SerializeObject(surtidores)}");
            List<Task> tasks = new List<Task>();
            tasks.AddRange(surtidores.GroupBy(x => x.Puerto).Select(x => new OperadorCara(_logger, x.ToList(), _estacionesRepositorio, _options, _sicomConection, _messageProducer).OperarCara(stoppingToken)));
            Task.WaitAll(tasks.ToArray());
        }


    }
}