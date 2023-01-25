using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FacturadorEstacionesRepositorio;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Microsoft.Extensions.Options;
using ManejadorSurtidor.SICOM;
using FactoradorEstacionesModelo.Siges;
using System.Numerics;

namespace ManejadorSurtidor
{
    public class ObtenerVehiculosWorker : BackgroundService
    {
        private readonly ILogger<ObtenerVehiculosWorker> _logger;
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;

        private readonly ISicomConection _sicomConection;
        public ObtenerVehiculosWorker(ILogger<ObtenerVehiculosWorker> logger, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _logger = logger;
            _sicomConection = sicomConection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Bajando SUIC");
                    string suic = "Fail"; //await _sicomConection.GetInfoCarros();
                    if (suic == "Fail")
                    {
                        suic = File.ReadAllText("SUIC.txt");
                    }
                    await setCarInDatabase(suic);

                    Thread.Sleep(1000 * 60 * 60 * 12);
                } catch(Exception ex)
                {
                    _logger.LogError($"Error. {ex.Message}.{ex.StackTrace}");
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
                        fechaInicio = DateTime.Parse(line[1]),
                        fechaFin = DateTime.Parse(line[2]),
                        placa = line[3],
                        vin = line[4],
                        servicio = line[5],
                        capacidad = line[6],
                        estado = Int32.Parse(line[7]),
                        motivo = line[8],
                        motivoTexto = line[9],
                    });
                    if (vehiculos.Count() == 10000)
                    {

                        _logger.LogInformation($"procesado {i} de {lines.Length}");
                        _estacionesRepositorio.ActualizarCarros(vehiculos);
                        vehiculos.Clear();
                    }
                } catch(Exception ex) { }
                
            }
            try
            {

                if (vehiculos.Count() < 10000)
                {

                    _logger.LogInformation($"procesado {vehiculos.Count()} de {lines.Length}");
                    _estacionesRepositorio.ActualizarCarros(vehiculos);
                    vehiculos.Clear();
                }
            }
            catch (Exception ex) { }
           
        }
    }
}