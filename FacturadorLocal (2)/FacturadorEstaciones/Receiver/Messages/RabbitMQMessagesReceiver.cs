using FactoradorEstacionesModelo;
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
    public class RabbitMQMessagesReceiver : IObservable<Mensaje>, IDisposable
    {
        List<IObserver<Mensaje>> observers = new List<IObserver<Mensaje>>();
        AsyncEventingBasicConsumer consumer;
        private IConnection _connection;
        private IChannel _channel;
        
        public RabbitMQMessagesReceiver()
        {
            var factory = new ConnectionFactory() { HostName = "LAPTOP-7BMLM7UO", UserName = "siges", Password = "siges", Port = Protocols.DefaultProtocol.DefaultPort };
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
            
            _channel.QueueDeclareAsync(queue: "controlador",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null).Wait();

            consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensaje = JsonConvert.DeserializeObject<Mensaje>(Encoding.UTF8.GetString(body));
                foreach (var observer in observers)
                {
                    observer.OnNext(mensaje);
                }
            };
            _channel.BasicConsumeAsync(queue: "controlador",
                                     autoAck: true,
                                     consumer: consumer).Wait();
        }

        public IDisposable Subscribe(IObserver<Mensaje> observer)
        {
            // Check whether observer is already registered. If not, add it
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber<Mensaje>(observers, observer);
        }

        public void Dispose()
        {
            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing RabbitMQ resources: {ex.Message}");
            }
        }

        internal class Unsubscriber<Mensaje> : IDisposable
        {
            private List<IObserver<Mensaje>> _observers;
            private IObserver<Mensaje> _observer;

            internal Unsubscriber(List<IObserver<Mensaje>> observers, IObserver<Mensaje> observer)
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
