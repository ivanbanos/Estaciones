using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace FacturadorEstacionesAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configFileName = "nlog.config";
            var logger = NLogBuilder.ConfigureNLog(configFileName).GetCurrentClassLogger();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception)
            {
                Environment.Exit(1);
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel();
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    webBuilder.UseIISIntegration();

                })
            .ConfigureLogging(l => {
                l.ClearProviders();
                l.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            }
            )
            .UseNLog();
    }
}
