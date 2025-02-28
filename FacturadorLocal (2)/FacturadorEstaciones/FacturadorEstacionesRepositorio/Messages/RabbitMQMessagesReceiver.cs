using Dominio.Entidades;
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
        AsyncEventingBasicConsumer consumer;
        private readonly InfoEstacion _infoEstacion;
        public RabbitMQMessagesReceiver(IOptions<InfoEstacion> infoEstacion)
        {
            _infoEstacion = infoEstacion.Value;
            ConnectionFactory factory = new ConnectionFactory();
            // "guest"/"guest" by default, limited to localhost connections
            factory.UserName = "siges";
            factory.Password = "siges";
            factory.VirtualHost = "/";
            factory.HostName = _infoEstacion.RabbitHost;
            factory.Port = 5672;
            var conn = factory.CreateConnectionAsync().Result;
            var channel = conn.CreateChannelAsync().Result;

            var ok = channel.QueueDeclareAsync(queue: _infoEstacion.Isla,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null).Result;

            consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensaje = Encoding.UTF8.GetString(body);
                foreach (var observer in observers)
                {
                    observer.OnNext(mensaje);
                }
            };
            var basicOk = channel.BasicConsumeAsync(queue: _infoEstacion.Isla,
                                  autoAck: true,
                                  consumer: consumer).Result;
        }
        public void ReceiveMessages(string queue)
        {

        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
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
