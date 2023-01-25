
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using NLog.Web;
using System;
using NLog;
using NLog.Targets;
using System.Threading.Tasks;
using ManejadorSurtidor.SICOM;
using ManejadorSurtidor.Messages;
using System.Collections.Generic;

namespace ManejadorSurtidor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                Layout = "${longdate} ${logger} ${message} ${exception}",
                FileName = "${basedir}/logs/${shortdate}.log",
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                ArchiveAboveSize = 5000000,
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Iniciando");

            await Task.Delay(5000, default);
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ISicomConection, SicomConection>();
                    services.AddSingleton<IEstacionesRepositorio, EstacionesRepositorioSqlServer>();
                    services.AddSingleton<IMessageProducer, RabbitMQProducer>();
                    services.Configure<ConnectionStrings>(options => hostContext.Configuration.GetSection("ConnectionStrings").Bind(options));
                    services.AddSingleton<ISicomConection, SicomConection>();
                    services.AddSingleton<IMessageProducer, RabbitMQProducer>();
                    services.Configure<Sicom>(options => hostContext.Configuration.GetSection("Sicom").Bind(options));
                    services.AddHostedService<Worker>();
                    services.Configure<FacturadorEstacionesPOSWinForm.InfoEstacion>(options => hostContext.Configuration.GetSection("InfoEstacion").Bind(options));
                    services.Configure<List<ServicioSIGES.CaraImpresora>>(options => hostContext.Configuration.GetSection("CarasImpresoras").Bind(options));

                });
    }
}
