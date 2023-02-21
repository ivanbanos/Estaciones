// See https://aka.ms/new-console-template for more information
using FactoradorEstacionesModelo;
using ManejadorSurtidor.Messages;

Console.WriteLine("Hello, World!");
var _messageProducer = new RabbitMQProducer();
var still = true;
while (still)
{
    Console.WriteLine("Escribe mensaje");
    Console.WriteLine("Surtidor:");
    var SurtidorId = Console.ReadLine();
    Console.WriteLine("Estado:");
    var Estado = Console.ReadLine();
    Console.WriteLine("Ubicacion:");
    var Ubicacion = Console.ReadLine();
    Console.WriteLine("Turno:");
    var Turno = Console.ReadLine();
    Console.WriteLine("Empleado:");
    var Empleado = Console.ReadLine();
    await _messageProducer.SendMessage(new Mensaje()
    {
        SurtidorId = Int32.Parse(SurtidorId),
        Estado = Estado,
        Ubicacion = Ubicacion,
        Turno = Turno,
        Empleado = Empleado
    });
}
