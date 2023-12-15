﻿using EnviadorInformacion;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturadorEstacionesRepositorio;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") {
            Layout = "${longdate} ${logger} ${message} ${exception}",
            FileName = "${basedir}/logs/${shortdate}.log",
            ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
            ArchiveAboveSize = 5000000,
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Trace, LogLevel.Error, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Error, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new Service1()
            //};
            //ServiceBase.Run(ServicesToRun);
            //var service = new Service1();
            //service.OnStart(null);

            //var impresionService = new ImpresionService();
            //var enviadorDeInformacion = new EnviadorDeInformacion();
            ////impresionService.Execute();
            //enviadorDeInformacion.EnviarInformacion();
            //var enviadorDeInformacionThread = new Thread(new ThreadStart(impresionService.Execute));
            //var impresionThread = new Thread(new ThreadStart(enviadorDeInformacion.EnviarInformacion));
            ////////if (ConfigurationManager.AppSettings["EnvioInformacion"] == "true")
            ////////{
            ////////    enviadorDeInformacionThread.Start();

            ////////}
            //impresionThread.Start();
            //enviadorDeInformacionThread.Start();
            //var enviadorDeInformacion = new EnviadorDeInformacion();
            //enviadorDeInformacion.EnviarInformacion();
            var enviadorSilog = new EnviadorSilog();
            enviadorSilog.EnviarInformacion();
            //var canastillaService = new CanastillaService();
            //canastillaService.ProcesoCanastilla();

        }
    }
}
