﻿using FactoradorEstacionesModelo;
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
    public class RabbitMQMessagesReceiver : IObservable<Mensaje>
    {
        List<IObserver<Mensaje>> observers = new List<IObserver<Mensaje>>();
        EventingBasicConsumer consumer;
        public RabbitMQMessagesReceiver()
        {
            var factory = new ConnectionFactory() { HostName = "LAPTOP-7BMLM7UO", UserName = "siges", Password = "siges", Port = Protocols.DefaultProtocol.DefaultPort };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            
                channel.QueueDeclare(queue: "controlador",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var mensaje = JsonConvert.DeserializeObject<Mensaje>(Encoding.UTF8.GetString(body));
                    foreach (var observer in observers)
                    {
                        observer.OnNext(mensaje);
                    }
                };
                channel.BasicConsume(queue: "controlador",
                                     autoAck: true,
                                     consumer: consumer);
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
