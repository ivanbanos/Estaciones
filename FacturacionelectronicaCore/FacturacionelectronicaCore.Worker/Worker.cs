
using FacturacionelectronicaCore.Negocio.OrdenDeDespacho;
using EstacionesServicio.Modelo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using FacturacionelectronicaCore.Negocio.Modelo;
using Newtonsoft.Json;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using Microsoft.Extensions.Options;
using FacturacionelectronicaCore.Negocio.FacturaCanastillaNegocio;
using System.Linq;

namespace FacturacionelectronicaCore.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOrdenDeDespachoNegocio _ordenDeDespachoNegocio;
        private readonly FacturacionelectronicaCore.Negocio.Estacion.IEstacionNegocio _estacionNegocio;
        private readonly FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal.IManejadorInformacionLocalNegocio _manejadorInformacionLocalNegocio;
        private readonly IFacturaCanastillaNegocio _facturaCanastillaNegocio;
        private readonly Alegra _alegraOptions;

        public Worker(
            ILogger<Worker> logger,
            IOrdenDeDespachoNegocio ordenDeDespachoNegocio,
            FacturacionelectronicaCore.Negocio.Estacion.IEstacionNegocio estacionNegocio,
            FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal.IManejadorInformacionLocalNegocio manejadorInformacionLocalNegocio,
            IFacturaCanastillaNegocio facturaCanastillaNegocio,
            IOptions<Alegra> alegra)
        {
            _logger = logger;
            _ordenDeDespachoNegocio = ordenDeDespachoNegocio;
            _estacionNegocio = estacionNegocio;
            _manejadorInformacionLocalNegocio = manejadorInformacionLocalNegocio;
            _facturaCanastillaNegocio = facturaCanastillaNegocio;
            _alegraOptions = alegra.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker service starting...");
            
            // Start the actual work in a fire-and-forget manner to return control immediately
            _ = ProcessingLoop(stoppingToken);
            
            _logger.LogInformation("Worker service started successfully.");
            
            // Keep the service running by waiting on the cancellation token
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker service shutdown requested.");
            }
        }

        private async Task ProcessingLoop(CancellationToken stoppingToken)
        {
            try
            {
                // Initial delay to ensure all dependencies are ready
                _logger.LogInformation("ProcessingLoop starting with 15-second initialization delay...");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                
                while (!stoppingToken.IsCancellationRequested)
                {
                try
                {
                    _logger.LogInformation("Starting processing cycle for orders and facturas canastilla...");
                    var estaciones = await _estacionNegocio.GetEstaciones();
                    foreach (var estacion in estaciones)
                    {
                        // Check for cancellation before processing each station
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        var startDate = _alegraOptions.WorkerStartDate ?? DateTime.Now.AddMonths(-2);
                        var currentDate = DateTime.Now;
                        var totalOrdenesReenviadas = 0;
                        var totalOrdenesEncontradas = 0;
                        var totalCanastillasReenviadas = 0;
                        var totalCanastillasEncontradas = 0;

                        // Calculate how many months to process
                        var monthsToProcess = ((currentDate.Year - startDate.Year) * 12) + currentDate.Month - startDate.Month + 1;

                        // Process each month separately to reduce memory usage
                        for (int monthOffset = 0; monthOffset < monthsToProcess; monthOffset++)
                        {
                            // Check for cancellation before processing each month
                            if (stoppingToken.IsCancellationRequested)
                                break;

                            var fechaFinal = currentDate.AddMonths(-monthOffset);
                            var fechaInicial = fechaFinal.AddMonths(-1);

                            // For the first month, use the exact start date if it's more recent
                            if (monthOffset == monthsToProcess - 1 && startDate > fechaInicial)
                            {
                                fechaInicial = startDate;
                            }

                            // Skip if the date range is invalid
                            if (fechaInicial >= fechaFinal)
                                continue;

                            var filtro = new FiltroBusqueda
                            {
                                FechaInicial = fechaInicial,
                                FechaFinal = fechaFinal,
                                Identificacion = null,
                                NombreTercero = null,
                                Estacion = estacion.guid
                            };

                            var ordenes = await _ordenDeDespachoNegocio.GetOrdenesDeDespacho(filtro);
                            totalOrdenesEncontradas += ordenes?.Count() ?? 0;

                            var ordenesAReenviar = ordenes?
                                .Where(o => (
                                    // No factura registrada yet or previously marked as error
                                    string.IsNullOrEmpty(o.idFacturaElectronica)
                                    || o.idFacturaElectronica.StartsWith("error", StringComparison.OrdinalIgnoreCase)
                                    // Or the stored idFacturaElectronica contains an embedded provider invoice
                                    // and the local IdVentaLocal appears more than once -> suspicious payload
                                    || (o.idFacturaElectronica != null
                                        && o.idFacturaElectronica.IndexOf("order_reference", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (o.idFacturaElectronica.IndexOf(o.IdVentaLocal.ToString(), StringComparison.OrdinalIgnoreCase)
                                    == o.idFacturaElectronica.LastIndexOf(o.IdVentaLocal.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || o.idFacturaElectronica.IndexOf(o.IdVentaLocal.ToString(), StringComparison.OrdinalIgnoreCase) < 0)
                                    )
                                )
                                // Exclude Crédito Directo payments (trim + case-insensitive)
                                && !(o.FormaDePago?.Trim().Equals("Crédito Directo", StringComparison.OrdinalIgnoreCase) ?? false)
                                && (_alegraOptions.EnviaCreditos || (!(o.FormaDePago?.ToLower().Contains("dir") ?? false) && !(o.FormaDePago?.ToLower().Contains("calibra") ?? false) && !(o.FormaDePago?.ToLower().Contains("puntos") ?? false))))
                                .Take(20) // Limit per month to avoid overwhelming the system
                                .ToList();

                            if (ordenesAReenviar != null && ordenesAReenviar.Count > 0)
                            {
                                totalOrdenesReenviadas += ordenesAReenviar.Count;
                                _logger.LogInformation($"Estación: {estacion.Nombre} ({estacion.guid}) - Mes {fechaInicial:MM/yyyy} - Reenviando {JsonConvert.SerializeObject(ordenesAReenviar.Select(o => o.IdVentaLocal))} órdenes a facturación...");
                                await _manejadorInformacionLocalNegocio.EnviarOrdenesDespacho(ordenesAReenviar, estacion.guid);
                            }

                            // Process facturas canastilla for the same month
                            var facturas = await _facturaCanastillaNegocio.GetFacturas(fechaInicial, fechaFinal, null, null, estacion.guid);
                            totalCanastillasEncontradas += facturas?.Count() ?? 0;

                            var facturasAReenviar = facturas?
                                .Where(f => (
                                    // No factura registrada yet or previously marked as error
                                    string.IsNullOrEmpty(f.idFacturaElectronica)
                                    || f.idFacturaElectronica.StartsWith("error", StringComparison.OrdinalIgnoreCase)
                                    // Or the stored idFacturaElectronica contains an embedded provider invoice
                                    // and the local consecutivo appears more than once -> suspicious payload
                                    || (f.idFacturaElectronica != null
                                        && f.idFacturaElectronica.IndexOf("order_reference", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (f.idFacturaElectronica.IndexOf(f.consecutivo.ToString(), StringComparison.OrdinalIgnoreCase)
                                    == f.idFacturaElectronica.LastIndexOf(f.consecutivo.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || f.idFacturaElectronica.IndexOf(f.consecutivo.ToString(), StringComparison.OrdinalIgnoreCase) < 0)
                                    )
                                )
                                // Exclude Crédito Directo payments (trim + case-insensitive)
                                && !(f.codigoFormaPago?.Descripcion?.Trim().Equals("Crédito Directo", StringComparison.OrdinalIgnoreCase) ?? false)
                                && (_alegraOptions.EnviaCreditos || (!(f.codigoFormaPago?.Descripcion?.ToLower().Contains("dir") ?? false) && !(f.codigoFormaPago?.Descripcion?.ToLower().Contains("calibra") ?? false) && !(f.codigoFormaPago?.Descripcion?.ToLower().Contains("puntos") ?? false))))
                                .Take(20) // Limit per month to avoid overwhelming the system
                                .ToList();

                            if (facturasAReenviar != null && facturasAReenviar.Count > 0)
                            {
                                // Convert entities to models for AddFacturaCanastilla
                                var facturasModelo = facturasAReenviar.Select(f => ConvertToModel(f)).ToList();
                                totalCanastillasReenviadas += facturasAReenviar.Count;
                                _logger.LogInformation($"Estación: {estacion.Nombre} ({estacion.guid}) - Mes {fechaInicial:MM/yyyy} - Reenviando {JsonConvert.SerializeObject(facturasAReenviar.Select(f => f.consecutivo))} facturas canastilla a facturación...");
                                await _manejadorInformacionLocalNegocio.AddFacturaCanastilla(facturasModelo, estacion.guid);
                            }

                            // Small delay between months to avoid overwhelming the system
                            await Task.Delay(1000, stoppingToken);

                            // Yield control periodically to allow service shutdown
                            if (monthOffset > 0 && monthOffset % 3 == 0)
                            {
                                await Task.Yield();
                            }
                        }

                        _logger.LogInformation($"Estación: {estacion.Nombre} ({estacion.guid}) - Total procesado: {totalOrdenesEncontradas} órdenes ({totalOrdenesReenviadas} reenviadas), {totalCanastillasEncontradas} canastillas ({totalCanastillasReenviadas} reenviadas) desde {startDate:yyyy-MM-dd}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener órdenes de despacho y facturas canastilla por estación");
                }

                _logger.LogInformation("Processing cycle completed (orders and canastillas). Waiting 5 minutes before next cycle...");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ProcessingLoop was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in ProcessingLoop.");
            }
        }

        /// <summary>
        /// Converts a FacturaCanastilla entity to a FacturaCanastilla model for processing
        /// </summary>
        private FacturacionelectronicaCore.Negocio.Modelo.FacturaCanastilla ConvertToModel(FacturacionelectronicaCore.Repositorio.Entities.FacturaCanastilla entity)
        {
            return new FacturacionelectronicaCore.Negocio.Modelo.FacturaCanastilla
            {
                idFacturaElectronica = entity.idFacturaElectronica,
                FacturasCanastillaId = entity.FacturasCanastillaId,
                fecha = entity.fecha,
                consecutivo = entity.consecutivo,
                estado = entity.estado,
                impresa = entity.impresa,
                enviada = entity.enviada,
                subtotal = entity.subtotal,
                descuento = entity.descuento,
                iva = entity.iva,
                total = entity.total,
                Guid = Guid.TryParse(entity.Guid, out var guid) ? guid : Guid.Empty,
                IdEstacion = entity.IdEstacion,
                // Set complex types to null for now - the essential fields for processing are above
                resolucion = null,
                terceroId = null,
                codigoFormaPago = null,
                canastillas = null
            };
        }
    }
}
