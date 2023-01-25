using ControladorEstacion.Messages;
using EnviadorInformacionService;
using FacturadorEstacionesPOSWinForm;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using System.Reflection.Metadata;

namespace ControladorEstacion
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            var configFilename = "nlog.config";
            var logger = NLogBuilder.ConfigureNLog(configFilename).GetCurrentClassLogger();

            try
            {
                logger.Info("Starting");
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var host = CreateHostBuilder(args).Build();

                var services = host.Services;
                var mainForm = services.GetRequiredService<Form1>();
                

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                logger.Error("Failed to start.", ex);
                Environment.Exit(1);
            }
            Console.Read();
        }


        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IEstacionesRepositorio, EstacionesRepositorioSqlServer>();
                    services.AddSingleton<IConexionEstacionRemota, ConexionEstacionRemota>();
                    services.AddSingleton<Form1>();
                    services.Configure<ConnectionStrings>(options => hostContext.Configuration.GetSection("ConnectionStrings").Bind(options));
                    services.Configure<InfoEstacion>(options => hostContext.Configuration.GetSection("InfoEstacion").Bind(options));
                });
        }
    }
}