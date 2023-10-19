using FacturadorEstacionesPOSWinForm;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using Modelo;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManejadorSurtidor.Messages
{
    public class RabbitMQProducer : IMessageProducer
    {
        private readonly InfoEstacion _infoEstacion;
        private readonly ConnectionFactory factory;
        private readonly IConnection connection;

        public RabbitMQProducer(IOptions<InfoEstacion> infoEstacion)
        {
            _infoEstacion = infoEstacion.Value; 
            factory = new ConnectionFactory() { HostName = _infoEstacion.RabbitHost, UserName = "siges", Password = "siges", Port = Protocols.DefaultProtocol.DefaultPort };

             connection = factory.CreateConnection();
        }
        public async Task SendMessage<T>(T message, string queue)
        {
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queue,
                                      durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            for(int i = 0; i < 3; i++)
            {
                try
                {
                    channel.BasicPublish(exchange: "", routingKey: queue, body: body);
                    break;
                }catch(Exception ex)
                {

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
