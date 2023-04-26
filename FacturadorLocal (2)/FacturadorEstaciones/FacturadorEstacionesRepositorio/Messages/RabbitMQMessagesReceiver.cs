using FactoradorEstacionesModelo;
using FactoradorEstacionesModelo.Siges;
using FacturadorEstacionesPOSWinForm;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using Modelo;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControladorEstacion.Messages
{
    public class RabbitMQMessagesReceiver : IMessagesReceiver , IObservable<string>
    {
        List<IObserver<string>> observers = new List<IObserver<string>>();
        EventingBasicConsumer consumer;
        private readonly InfoEstacion _infoEstacion;
        public RabbitMQMessagesReceiver(IOptions<InfoEstacion> infoEstacion)
        {
            _infoEstacion = infoEstacion.Value;
            var factory = new ConnectionFactory() { HostName = _infoEstacion.RabbitHost, UserName="siges", Password="siges", Port = Protocols.DefaultProtocol.DefaultPort };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue:_infoEstacion.Isla,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensaje = Encoding.UTF8.GetString(body);
                foreach (var observer in observers)
                {
                    observer.OnNext(mensaje);
                }
            };
            channel.BasicConsume(queue: _infoEstacion.Isla,
                                 autoAck: true,
                                 consumer: consumer);
        }
        public void ReceiveMessages(string queue)
        {
            
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            // Check whether observer is already registered. If not, add it
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber(observers, observer);
        }

        internal class Unsubscriber : IDisposable
        {
            private List<IObserver<string>> _observers;
            private IObserver<string> _observer;

            internal Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }
}
