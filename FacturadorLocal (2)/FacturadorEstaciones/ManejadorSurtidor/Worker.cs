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
using System;
using ControladorEstacion.Messages;

namespace ManejadorSurtidor
{
    public class Worker : BackgroundService, IObserver<string>
    {
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;

        private readonly ISicomConection _sicomConection;
        private readonly IMessageProducer _messageProducer;
        private readonly IFidelizacion _fidelizacion;
        private readonly Islas _islas;

        public override void Dispose()
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            _logger.Log(NLog.LogLevel.Info, "Iniciando");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _logger.Log(NLog.LogLevel.Info, "Cerrando");
        }
        public Worker(ILogger<Worker> logger, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection, IMessageProducer messageProducer, IFidelizacion fidelizacion,
            //IMessagesReceiver messageReceiver, 
            Islas islas)
        {

           // ((IObservable<string>)messageReceiver).Subscribe(this);
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _sicomConection = sicomConection;
            _messageProducer = messageProducer;
            _fidelizacion = fidelizacion;
            _islas = islas;
        }
    

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Log(NLog.LogLevel.Info, "Buscando Caras");
            var surtidores = _estacionesRepositorio.GetSurtidoresSiges();

            _logger.Log(NLog.LogLevel.Info, $"Caras {JsonConvert.SerializeObject(surtidores)}");
            List<Task> tasks = new List<Task>();
            tasks.AddRange(surtidores.GroupBy(x => x.Puerto).Select(x => new OperadorCara(_logger, x.ToList(), _estacionesRepositorio, _options, _sicomConection, _messageProducer, _fidelizacion, _islas).OperarCara(stoppingToken)));
            Task.WaitAll(tasks.ToArray());
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(string value)
        {
            _islas.AgregarIsla(value);
        }
    }
}