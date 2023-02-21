using EnviadorInformacionService;
using FactoradorEstacionesModelo;
using FacturadorEstacionesPOSWinForm.Repo;
using ManejadorSurtidor;
using ManejadorSurtidor.Messages;
using ManejadorSurtidor.SICOM;
using SigesServicio;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<FacturadorEstacionesPOSWinForm.InfoEstacion>(options => hostContext.Configuration.GetSection("InfoEstacion").Bind(options));
        services.AddSingleton<FacturadorEstacionesRepositorio.IEstacionesRepositorio, FacturadorEstacionesRepositorio.EstacionesRepositorioSqlServer>();
        services.Configure<FacturadorEstacionesRepositorio.ConnectionStrings>(options => hostContext.Configuration.GetSection("ConnectionStrings").Bind(options));
        services.Configure<List<ServicioSIGES.CaraImpresora>>(options => hostContext.Configuration.GetSection("CarasImpresoras").Bind(options));
        services.AddSingleton<ISicomConection, SicomConection>();
        services.AddSingleton<IMessageProducer, RabbitMQProducer>();
        services.Configure<Sicom>(options => hostContext.Configuration.GetSection("Sicom").Bind(options));
        services.Configure<InformacionCuenta>(options => hostContext.Configuration.GetSection("InformacionCuenta").Bind(options));

        services.AddSingleton<IConexionEstacionRemota, ConexionEstacionRemota>();
        services.AddHostedService<SubirVentasWorker>();
        services.AddHostedService<ObtenerVehiculosWorker>();
        services.AddHostedService<WorkerImpresion>();
        services.AddHostedService<CanastillaWorker>();
        services.AddHostedService<FacturasWorker>();

    })
    .Build();

await host.RunAsync();
