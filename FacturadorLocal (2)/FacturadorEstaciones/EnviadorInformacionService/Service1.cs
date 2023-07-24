using EnviadorInformacion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public partial class Service1 : ServiceBase
    {
        private readonly IEnviadorDeInformacion enviadorDeInformacion;
        private Thread envioThread;
        private readonly ImpresionService impresionService;
        private Thread impresionThread;
        private readonly IEnviadorProsoft enviadorProsoft;
        private Thread enviadorProsoftThread;
        private readonly ICanastillaService canastillaService;
        private Thread canastillaServiceThread;
        private Thread canastillaWebServiceThread;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public Service1()
        {
            InitializeComponent();
            enviadorDeInformacion = new EnviadorDeInformacion();
            impresionService = new ImpresionService();
            enviadorProsoft = new EnviadorProsoft();
            //canastillaService = new CanastillaService();
        }

        protected override void OnStart(string[] args)
        {
            try
            {

                Logger.Error("Iniciando ");
                if (ConfigurationManager.AppSettings["EnvioInformacion"] == "true")
                {
                    envioThread = new Thread(new ThreadStart(enviadorDeInformacion.EnviarInformacion));
                    envioThread.Start();
                }

                Logger.Info(ConfigurationManager.AppSettings["EnvioAProsoft"]);
                if (ConfigurationManager.AppSettings["EnvioAProsoft"] == "true" )
                {

                    Logger.Info("Iniciando interfaz Prosoft");
                    enviadorProsoftThread = new Thread(new ThreadStart(enviadorProsoft.EnviarInformacion));
                    enviadorProsoftThread.Start();
                }
                impresionThread = new Thread(new ThreadStart(impresionService.Execute));
                impresionThread.Start();
                //canastillaServiceThread = new Thread(new ThreadStart(canastillaService.ProcesoCanastilla));
                //canastillaServiceThread.Start();
                //canastillaWebServiceThread = new Thread(new ThreadStart(canastillaService.WebCanastilla));
                //canastillaWebServiceThread.Start();

            }
            catch (Exception ex)
            {
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
            }


        }



        protected override void OnStop()
        {
            try
            {
                envioThread.Abort();
                impresionThread.Abort();
                enviadorProsoftThread.Abort();
                //canastillaServiceThread.Abort();
            }
            catch (Exception ex)
            {
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
            }
        }
    }
}
