using SigesServicio;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
        services.Configure<FacturadorEstacionesPOSWinForm.InfoEstacion>(options => hostContext.Configuration.GetSection("InfoEstacion").Bind(options));
        services.AddSingleton<FacturadorEstacionesRepositorio.IEstacionesRepositorio, FacturadorEstacionesRepositorio.EstacionesRepositorioSqlServer>();
        services.Configure<FacturadorEstacionesRepositorio.ConnectionStrings>(options => hostContext.Configuration.GetSection("ConnectionStrings").Bind(options));
        services.Configure<List<ServicioSIGES.CaraImpresora>>(options => hostContext.Configuration.GetSection("CarasImpresoras").Bind(options));

    })
    .Build();

await host.RunAsync();
