using FactoradorEstacionesModelo;
using FactoradorEstacionesModelo.Siges;
using Microsoft.AspNetCore.Connections;
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
    public class RabbitMQMessagesReceiver : IMessagesReceiver , IObservable<VehiculoSuic>
    {
        List<IObserver<VehiculoSuic>> observers = new List<IObserver<VehiculoSuic>>();
        EventingBasicConsumer consumer;
        public RabbitMQMessagesReceiver()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "vehiculo",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensaje = JsonConvert.DeserializeObject<VehiculoSuic>(Encoding.UTF8.GetString(body));
                foreach (var observer in observers)
                {
                    observer.OnNext(mensaje);
                }
            };
            channel.BasicConsume(queue: "vehiculo",
                                 autoAck: true,
                                 consumer: consumer);
        }
        public void ReceiveMessages()
        {
            
        }

        public IDisposable Subscribe(IObserver<VehiculoSuic> observer)
        {
            // Check whether observer is already registered. If not, add it
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber<VehiculoSuic>(observers, observer);
        }

        internal class Unsubscriber<VehiculoSuic> : IDisposable
        {
            private List<IObserver<VehiculoSuic>> _observers;
            private IObserver<VehiculoSuic> _observer;

            internal Unsubscriber(List<IObserver<VehiculoSuic>> observers, IObserver<VehiculoSuic> observer)
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
