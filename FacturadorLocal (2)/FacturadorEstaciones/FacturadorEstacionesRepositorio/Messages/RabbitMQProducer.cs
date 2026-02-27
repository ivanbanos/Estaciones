using FacturadorEstacionesPOSWinForm;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using Modelo;
using NLog;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManejadorSurtidor.Messages
{
    public class RabbitMQProducer : IMessageProducer, IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly InfoEstacion _infoEstacion;
        private IConnection _connection;
        private bool _disposed = false;

        public RabbitMQProducer(IOptions<InfoEstacion> infoEstacion)
        {
            _infoEstacion = infoEstacion.Value;
        }

        private async Task EnsureConnectionAsync()
        {
            if (_connection != null && _connection.IsOpen)
            {
                return;
            }

            try
            {
                ConnectionFactory factory = new ConnectionFactory();
                factory.UserName = "siges";
                factory.Password = "siges";
                factory.VirtualHost = "/";
                factory.HostName = _infoEstacion.RabbitHost;
                
                _connection = await factory.CreateConnectionAsync();
                _logger.Log(LogLevel.Info, $"RabbitMQ conexión abierta en host {_infoEstacion.RabbitHost}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error creando conexión RabbitMQ a host {_infoEstacion.RabbitHost}: {ex.Message}");
                throw new InvalidOperationException($"Failed to create RabbitMQ connection to {_infoEstacion.RabbitHost}", ex);
            }
        }

        public async Task SendMessage<T>(T message, string queue)
        {
            await EnsureConnectionAsync();
            
            if (_connection == null || _connection.IsOpen == false)
            {
                throw new InvalidOperationException("RabbitMQ connection is not available");
            }

            using var channel = await _connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: queue,
                                       durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _logger.Log(LogLevel.Info, $"Publicando mensaje en cola '{queue}'. Intento {i + 1}/{maxRetries}");
                    await channel.BasicPublishAsync(exchange: "", routingKey: queue, body: body);
                    _logger.Log(LogLevel.Info, $"Mensaje publicado correctamente en cola '{queue}'");
                    break;
                }
                catch (Exception ex)
                {
                    if (i < maxRetries - 1)
                    {
                        _logger.Log(LogLevel.Warn, $"Falló publicación en cola '{queue}' intento {i + 1}: {ex.Message}. Reintentando...");
                        await Task.Delay(1000);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, $"Falló publicación en cola '{queue}' tras {maxRetries} intentos: {ex.Message}");
                        throw new InvalidOperationException($"Failed to send message to queue '{queue}' after {maxRetries} attempts", ex);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing RabbitMQ connection: {ex.Message}");
            }

            _disposed = true;
        }
    }
}
