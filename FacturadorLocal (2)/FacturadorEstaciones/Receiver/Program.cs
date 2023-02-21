// See https://aka.ms/new-console-template for more information
using ControladorEstacion.Messages;

Console.WriteLine("Hello, World!");

var receiver = new RabbitMQMessagesReceiver();


Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();