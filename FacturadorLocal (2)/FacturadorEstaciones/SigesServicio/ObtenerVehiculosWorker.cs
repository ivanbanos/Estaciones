using FacturadorEstacionesRepositorio;
using NLog;
using Microsoft.Extensions.Options;
using ManejadorSurtidor.SICOM;
using FactoradorEstacionesModelo.Siges;
using LogLevel = NLog.LogLevel;
using Modelo;

namespace ManejadorSurtidor
{
    public class ObtenerVehiculosWorker : BackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;
        private readonly IOptions<InfoEstacion> _optionsInfo;

        private readonly ISicomConection _sicomConection;

        public override void Dispose()
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
        public ObtenerVehiculosWorker(IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection, IOptions<InfoEstacion> optionsInfo)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _sicomConection = sicomConection;
            _optionsInfo = optionsInfo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Logger.Log(LogLevel.Info,"Bajando SUIC");
                    string suic = await _sicomConection.GetInfoCarros();
                    if (suic == "Fail")
                    {
                        suic = File.ReadAllText(_optionsInfo.Value.ArchivoSiCOM + "SUIC.txt");
                    }
                    await setCarInDatabase(suic);

                    Thread.Sleep(1000 * 60 * 60 * 12);
                } catch(Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Error. {ex.Message}.{ex.StackTrace}");
                    Thread.Sleep(1000 * 60 * 5);
                }
            }
        });

        private async Task setCarInDatabase(string suic)
        {
            var lines = suic.Split('\n');
            var vehiculos = new List<VehiculoSuic>();
            for(var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Split(',');
                try
                {

                    vehiculos.Add(new VehiculoSuic()
                    {
                        idrom = line[0],
                        fechaInicio = string.IsNullOrEmpty(line[1])?DateTime.MinValue :DateTime.Parse(line[1]),
                        fechaFin = string.IsNullOrEmpty(line[2]) ? DateTime.MinValue : DateTime.Parse(line[2]),
                        placa = line[3],
                        vin = line[4],
                        servicio = line[5],
                        capacidad = line[6],
                        estado = string.IsNullOrEmpty(line[7]) ? 0 : Int32.Parse(line[7]),
                        motivo = line[8],
                        motivoTexto = line[9],
                    });
                } catch(Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Error. {ex.Message}.{ex.StackTrace}");
                    
                }
                try
                {

                    if (vehiculos.Count() == 10000)
                    {

                        Logger.Log(LogLevel.Info, $"procesado {i} de {lines.Length}");
                        _estacionesRepositorio.ActualizarCarros(vehiculos);
                        vehiculos.Clear();
                    }

                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Error. {ex.Message}.{ex.StackTrace}");
                    Thread.Sleep(1000 * 60 * 5);
                }
            }
            try
            {

                if (vehiculos.Count() < 10000)
                {

                    Logger.Log(LogLevel.Info, $"procesado {vehiculos.Count()} de {lines.Length}");
                    _estacionesRepositorio.ActualizarCarros(vehiculos);
                    vehiculos.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Error. {ex.Message}.{ex.StackTrace}");
                Thread.Sleep(1000 * 60 * 5);
            }
           
        }
    }
}