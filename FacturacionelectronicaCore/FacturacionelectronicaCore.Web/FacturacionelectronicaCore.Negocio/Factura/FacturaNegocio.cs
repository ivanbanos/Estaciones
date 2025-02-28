using AutoMapper;
using EstacionesServicio.Modelo;
using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Factura
{
    public class FacturaNegocio : IFacturaNegocio
    {
        private readonly IFacturasRepository _facturasRepository;
        private readonly ITurnoRepositorio _turnoRepositorio;
        private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;
        private readonly ITerceroRepositorio _terceroRepositorio;
        private readonly IMapper _mapper;
        private readonly IFacturacionElectronicaFacade _alegraFacade;
        private readonly Alegra _alegra;
        private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;
        public FacturaNegocio(IFacturasRepository facturasRepository, IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio, IMapper mapper, IFacturacionElectronicaFacade alegraFacade, IOptions<Alegra> alegra, ITerceroRepositorio terceroRepositorio, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica, ITurnoRepositorio turnoRepositorio)
        {
            _facturasRepository = facturasRepository;
            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
            _mapper = mapper;
            _alegraFacade = alegraFacade;
            _alegra = alegra.Value;
            _terceroRepositorio = terceroRepositorio;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
            _turnoRepositorio = turnoRepositorio;
        }

        public async Task<Modelo.Factura> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho)
        {
            try
            {
                var ordenesDeDespachoList = _mapper.Map<IEnumerable<EstacionesServicio.Modelo.OrdenesDeDespachoGuids>, IEnumerable<EstacionesServicio.Repositorio.Entities.OrdenesDeDespachoGuids>>(ordenesDeDespacho);
                var response = await _facturasRepository.CrearFacturaOrdenesDeDespacho(ordenesDeDespachoList);
                return _mapper.Map<IEnumerable<Repositorio.Entities.Factura>, List<Modelo.Factura>>(response).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new BusinessException("Ha ocurrido un error creando la factura con las ordenes de servicio seleccionadas. Favor verificar.");
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Modelo.Factura>> GetFacturas(FiltroBusqueda filtroFactura)
        {
            try
            {
                var facturasEntity = await _facturasRepository.GetFacturas(filtroFactura.FechaInicial
                    , filtroFactura.FechaFinal, filtroFactura.Identificacion, filtroFactura.NombreTercero, filtroFactura.Estacion);
                var facturas = _mapper.Map<IEnumerable<Repositorio.Entities.Factura>, IEnumerable<Modelo.Factura>>(facturasEntity);
                var nombresPorIdentificacion = new Dictionary<string, string>();
                foreach (var factura in facturas)
                {
                    factura.Estado = factura.idFacturaElectronica == null ? factura.Estado : "Anulada";
                    factura.Identificacion = factura.Identificacion == null ? "222222222222" : factura.Identificacion;
                    if (!nombresPorIdentificacion.ContainsKey(factura.Identificacion) || string.IsNullOrEmpty(nombresPorIdentificacion[factura.Identificacion]))
                    {
                        var tercero = await _terceroRepositorio.ObtenerTerceroPorIdentificacion(factura.Identificacion);
                        if (tercero.FirstOrDefault() != null)
                        {
                            if (!nombresPorIdentificacion.ContainsKey(factura.Identificacion))
                            {
                                nombresPorIdentificacion.Add(factura.Identificacion, tercero.FirstOrDefault()?.Nombre); 
                                
                            }
                            else
                            {
                                nombresPorIdentificacion[factura.Identificacion] = tercero.FirstOrDefault()?.Nombre;

                            }
                        }
                        else
                        {
                            if (!nombresPorIdentificacion.ContainsKey(factura.Identificacion))
                            {
                                nombresPorIdentificacion.Add(factura.Identificacion, " ");

                            }
                        }
                    }
                    factura.NombreTercero = nombresPorIdentificacion[factura.Identificacion];
                    factura.Fecha = factura.Fecha.ToLocalTime().AddHours(-7);
                    if(factura.Precio > 20000)
                    {

                        factura.Precio /= 10;
                        factura.SubTotal /= 10;
                        factura.Total /= 10;
                        factura.Descuento /= 10;
                    }
                }
                return facturas.OrderBy(x=>x.Consecutivo);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc />
        public Task<int> AnularFacturas(IEnumerable<EstacionesServicio.Modelo.FacturasEntity> facturas)
        {
            try
            {
                var facturasList = _mapper.Map<IEnumerable<EstacionesServicio.Modelo.FacturasEntity>, IEnumerable<Repositorio.Entities.FacturasEntity>>(facturas);
                return _facturasRepository.AnularFacturas(facturasList);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task AddFacturasImprimir(IEnumerable<EstacionesServicio.Modelo.FacturasEntity> facturas)
        {
            try
            {
                var facturasList = _mapper.Map<IEnumerable<EstacionesServicio.Modelo.FacturasEntity>, IEnumerable<Repositorio.Entities.FacturasEntity>>(facturas);
                _facturasRepository.AddFacturasImprimir(facturasList);
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(FiltroBusqueda filtroOrdenDeDespacho)
        {
            try
            {
                var ordenesDeDespacho = await _ordenDeDespachoRepositorio.GetOrdenesDeDespacho(filtroOrdenDeDespacho.FechaInicial,
                        filtroOrdenDeDespacho.FechaFinal, filtroOrdenDeDespacho.Identificacion, filtroOrdenDeDespacho.NombreTercero, filtroOrdenDeDespacho.Estacion);
                return _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenesDeDespacho);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <inheritdoc />
        public async Task<ReporteFiscal> GetReporteFiscal(FiltroBusqueda filtroFactura)
        {
            try
            {
                var facturas = await GetFacturas(filtroFactura).ConfigureAwait(true);
                var ordenes = await GetOrdenesDeDespacho(filtroFactura).ConfigureAwait(true);
                foreach(var factura in facturas)
                {
                    if (factura.Precio > 20000)
                    {

                        factura.Precio /= 10;
                        factura.SubTotal /= 10;
                        factura.Total /= 10;
                        factura.Descuento /= 10;
                    }
                }
                foreach (var orden in ordenes)
                {
                    if (orden.Precio > 20000)
                    {

                        orden.Precio /= 10;
                        orden.SubTotal /= 10;
                        orden.Total /= 10;
                        orden.Descuento /= 10;
                    }
                }
                if (!facturas.Any() && !ordenes.Any())
                {
                    return null;
                }

                var reporte = new ReporteFiscal
                {
                    ConsecutivoFacturaInicial = !facturas.Any()?0:facturas.Min(factura => factura.Consecutivo),
                    ConsecutivoDeFacturaFinal = !facturas.Any() ? 0 : facturas.Max(factura => factura.Consecutivo),
                    TotalDeVentas = !facturas.Any() ? 0 : facturas.Count(factura => factura.Estado != "Anulado" && factura.Estado != "Anulada" && factura.idFacturaElectronica == null),
                    CantidadDeFacturasAnuladas = !facturas.Any() ? 0 : facturas.Count(factura => factura.Estado == "Anulado" || factura.Estado == "Anulada" || factura.idFacturaElectronica != null),
                    ConsolidadoFacturasAnuladas = !facturas.Any() ? new List<ConsolidadoCombustible>() : GetConsolidados(facturas.Where(factura => factura.Estado == "Anulado" || factura.Estado == "Anulada" || factura.idFacturaElectronica != null)),
                    ConsolidadoOrdenesAnuladas = !ordenes.Any() ? new List<ConsolidadoCombustible>() : GetConsolidadosOrdenes(ordenes.Where(orden => orden.Estado == "Anulado" || orden.Estado == "Anulada" || orden.idFacturaElectronica != null)),
                    TotalDeOrdenes = !ordenes.Any() ? 0 : ordenes.Count(orden => orden.Estado != "Anulado" && orden.Estado != "Anulada" && orden.idFacturaElectronica == null),
                    Consolidados = !facturas.Any() ? new List<ConsolidadoCombustible>() : GetConsolidados(facturas.Where(factura => factura.Estado != "Anulado" && factura.Estado != "Anulada" && factura.idFacturaElectronica == null)),
                    ConsolidadosOrdenes = !ordenes.Any() ? new List<ConsolidadoCombustible>() : GetConsolidadosOrdenes(ordenes.Where(orden => orden.Estado != "Anulado" && orden.Estado != "Anulada" && orden.idFacturaElectronica == null)),
                    consolidadoClienteFacturas = !facturas.Any() ? new List<ConsolidadoCliente>() : GetConsolidadosClientes(facturas.Where(factura => factura.Estado != "Anulado" && factura.Estado != "Anulada" && factura.idFacturaElectronica == null)),
                    consolidadoClienteOrdenes = !ordenes.Any() ? new List<ConsolidadoCliente>() : GetConsolidadosOrdenesCliente(ordenes.Where(orden => orden.Estado != "Anulado" && orden.Estado != "Anulada" && orden.idFacturaElectronica == null)),
                    TotalFacturasAnuladas = !facturas.Any() ? 0 : facturas.Count(factura => factura.Estado == "Anulado" || factura.Estado == "Anulada" || factura.idFacturaElectronica != null),
                    TotalOrdenesAnuladas = !ordenes.Any() ? 0 : ordenes.Count(orden => orden.Estado == "Anulado" || orden.Estado == "Anulada" || orden.idFacturaElectronica != null),
                };
                return reporte;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private IEnumerable<ConsolidadoCliente> GetConsolidadosOrdenesCliente(IEnumerable<Modelo.OrdenDeDespacho> ordenes)
        {
            var consolidados = new List<ConsolidadoCliente>();
            foreach (var orden in ordenes)
            {
                if (!consolidados.Any(consolidado => consolidado.Cliente == orden.NombreTercero))
                {
                    consolidados.Add(new ConsolidadoCliente
                    {
                        Cliente = orden.NombreTercero,
                        Cantidad = 0,
                        Total = 0
                    });
                }
                var consolidado = consolidados.First(consolidado => consolidado.Cliente == orden.NombreTercero);
                consolidado.Cantidad += Convert.ToDecimal(orden.Cantidad);
                consolidado.Total += Convert.ToDecimal(orden.Total);
            }

            return consolidados;
        }

        private IEnumerable<ConsolidadoCliente> GetConsolidadosClientes(IEnumerable<Modelo.Factura> facturas)
        {
            var consolidados = new List<ConsolidadoCliente>();
            foreach (var factura in facturas)
            {
                if (!consolidados.Any(consolidado => consolidado.Cliente == factura.NombreTercero))
                {
                    consolidados.Add(new ConsolidadoCliente
                    {
                        Cliente = factura.NombreTercero,
                        Cantidad = 0,
                        Total = 0
                    });
                }
                var consolidado = consolidados.First(consolidado => consolidado.Cliente == factura.NombreTercero);
                consolidado.Cantidad += factura.Cantidad;
                consolidado.Total += factura.Total;
            }

            return consolidados;
        }

        private IEnumerable<ConsolidadoCombustible> GetConsolidadosOrdenes(IEnumerable<Modelo.OrdenDeDespacho> ordenes)
        {
            var consolidados = new List<ConsolidadoCombustible>();
            foreach (var orden in ordenes)
            {
                if (!consolidados.Any(consolidado => consolidado.Combustible == orden.Combustible))
                {
                    consolidados.Add(new ConsolidadoCombustible
                    {
                        Combustible = orden.Combustible,
                        Cantidad = 0,
                        Total = 0
                    });
                }
                var consolidado = consolidados.First(consolidado => consolidado.Combustible == orden.Combustible);
                consolidado.Cantidad += Convert.ToDecimal(orden.Cantidad);
                consolidado.Total += Convert.ToDecimal(orden.Total);
            }

            return consolidados;
        }

        private IEnumerable<ConsolidadoCombustible> GetConsolidados(IEnumerable<Modelo.Factura> facturas)
        {
            var consolidados = new List<ConsolidadoCombustible>();
            foreach (var factura in facturas)
            {
                if(!consolidados.Any(consolidado => consolidado.Combustible == factura.Combustible))
                {
                    consolidados.Add(new ConsolidadoCombustible{
                        Combustible = factura.Combustible,
                        Cantidad = 0,
                        Total = 0
                    });
                }
                var consolidado = consolidados.First(consolidado => consolidado.Combustible == factura.Combustible);
                consolidado.Cantidad += factura.Cantidad;
                consolidado.Total += factura.Total;
            }

            return consolidados;
        }

        public async Task<string> EnviarAFacturacion(string ordenGuid)
        {
            if (_validadorGuidAFacturaElectronica.FacturaSiendoProceada(ordenGuid))
            {
                return "Factura electrónica siendo procesada";
            }
            try
            {
                var facturaEntity = (await _facturasRepository.ObtenerFacturaPorGuid(ordenGuid)).FirstOrDefault();
            if (facturaEntity == null)
            {
                _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                return "Factura no existe";
            }
            if (facturaEntity.idFacturaElectronica != null)
            {
                _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                return "Factura electrónica existente";
                }
                facturaEntity.Fecha = facturaEntity.Fecha.ToLocalTime().AddHours(-7);
                var factura = _mapper.Map<Repositorio.Entities.Factura, Modelo.Factura>(facturaEntity);
            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(factura.Identificacion)).FirstOrDefault();
            var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            if (_alegra.ValidaTercero && tercero.idFacturacion == null)
            {

                _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                return "Tercero no está apto para facturación electrónica";
            }
            factura.Tercero = tercero;


                var response = "error";// await _alegraFacade.GenerarFacturaElectronica(factura, factura.Tercero, Guid.Parse(((FacturaMongo)facturaEntity).EstacionGuid));
                if (response != "Combustible no creado")
                {

                    await _facturasRepository.SetIdFacturaElectronicaFactura(response, factura.Guid);
                }
                else { return "Combustible no creado"; }
                _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                return "Ok";
            }
            catch (Exception e)
            {
                _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                return $"Fallo al crear factura electrónica Razón: {e.Message}";
            }
        }


        public async Task<string> CrearFacturaFacturas(IEnumerable<EstacionesServicio.Modelo.FacturasEntity> facturasGuids)
        {
            var guids = facturasGuids.Select(x => x.Guid);
            if (_validadorGuidAFacturaElectronica.FacturasSiendoProceada(guids))
            {
                return "Factura electrónica siendo procesada";
            }
            var facturas = new List<Modelo.Factura>();
            foreach (var guid in facturasGuids)
            {
                var facturaEntity = (await _facturasRepository.ObtenerFacturaPorGuid(guid.Guid)).FirstOrDefault();
                
                if (facturaEntity == null)
                {
                    _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                    return $"Factura {guid.Guid} no existe";
                }
                if (facturaEntity.idFacturaElectronica != null)
                {
                    _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                    return "Una factura ya tiene factura electrónica existente";
                }
                facturaEntity.Fecha = facturaEntity.Fecha.ToLocalTime().AddHours(-7);
                facturas.Add(_mapper.Map<Repositorio.Entities.Factura, Modelo.Factura>(facturaEntity));
            }
            if (facturas.GroupBy(x => x.Identificacion).Count() > 1)
            {
                _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                return $"Las facturas deben pertenecer al mismo tercero";

            }
            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(facturas.First().Identificacion)).FirstOrDefault();
            var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            if (tercero.idFacturacion == null)
            {
                _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                return "Tercero no está apto para facturación electrónica";
            }
            var combustibles = new List<string>();
            foreach(var factura in facturas)
            {
                if (!combustibles.Contains(factura.Combustible))
                {
                    combustibles.Add(factura.Combustible);
                }
            }
            var items = combustibles.Select(x => _alegraFacade.GetItem(x,null).Result);

            try
            {
                var idFacturaElectronica = "error";// await _alegraFacade.GenerarFacturaElectronica(facturas, tercero, items);
                foreach (var orden in facturas)
                {
                    await _ordenDeDespachoRepositorio.SetIdFacturaElectronicaOrdenesdeDespacho(idFacturaElectronica, orden.Guid);
                }
                _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                return "Ok";
            }
            catch (Exception e)
            {
                _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                return $"Fallo al crear factura electrónica Razón: {e.Message}";
            }
        }


        public async Task<Modelo.Factura> ObtenerFacturaPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {
            var facturaEntity = (await _facturasRepository.ObtenerFacturaPorIdVentaLocal(idVentaLocal, estacion)).FirstOrDefault();
            if (facturaEntity == null)
            {
                return null;
            }
            var factura = _mapper.Map<Repositorio.Entities.Factura, Modelo.Factura>(facturaEntity);
            if (factura.Precio > 20000)
            {

                factura.Precio /= 10;
                factura.SubTotal /= 10;
                factura.Total /= 10;
                factura.Descuento /= 10;
            }
            return factura;
        }

        public async Task AgregarTurnoAFactura(int idVentaLocal, DateTime fecha, string isla, int numero, Guid estacion)
        {
            var turno = (await _turnoRepositorio.Get(fecha, numero, isla, estacion.ToString())).FirstOrDefault();
            if(turno != null)
            {
               await _facturasRepository.AgregarTurnoAFactura(idVentaLocal, turno.Id, estacion);
            }
            else
            {
                throw new Exception();
            }

        }

        public async Task<IEnumerable<Modelo.Factura>> ObtenerFacturasPorTurno(Guid turno)
        {
            var facturasEntity = await _facturasRepository.ObtenerFacturasPorTurnoId(turno);

            var facturas = _mapper.Map<IEnumerable<Repositorio.Entities.Factura>, IEnumerable<Modelo.Factura>>(facturasEntity);
            foreach (var factura in facturas)
            {
                if (factura.Precio > 20000)
                {

                    factura.Precio /= 10;
                    factura.SubTotal /= 10;
                    factura.Total /= 10;
                    factura.Descuento /= 10;
                }
            }
            return facturas;
        }

        public async Task<string> EnviarAFacturacion(int idVentaLocal, Guid estacion)
        {
            var facturaEntity = (await _facturasRepository.ObtenerFacturaPorIdVentaLocal(idVentaLocal, estacion)).FirstOrDefault();
            if (facturaEntity == null)
            {
                return null;
            }
            if (_validadorGuidAFacturaElectronica.FacturaSiendoProceada(facturaEntity.Guid))
            {
                return "Factura electrónica siendo procesada";
            }
            try
            {
                if (facturaEntity == null)
                {
                    _validadorGuidAFacturaElectronica.SacarFactura(facturaEntity.Guid);
                    return "Factura no existe";
                }
                if (facturaEntity.idFacturaElectronica != null)
                {
                    _validadorGuidAFacturaElectronica.SacarFactura(facturaEntity.Guid);
                    return "Factura electrónica existente";
                }
                facturaEntity.Fecha = facturaEntity.Fecha.ToLocalTime().AddHours(-7);
                var factura = _mapper.Map<Repositorio.Entities.Factura, Modelo.Factura>(facturaEntity);
                var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(factura.Identificacion)).FirstOrDefault();
                var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
                if (_alegra.ValidaTercero && tercero.idFacturacion == null)
                {

                    _validadorGuidAFacturaElectronica.SacarFactura(facturaEntity.Guid);
                    return "Tercero no está apto para facturación electrónica";
                }
                factura.Tercero = tercero;


                var response = "error";// await _alegraFacade.GenerarFacturaElectronica(factura, factura.Tercero, Guid.Parse(((FacturaMongo)facturaEntity).EstacionGuid));
                if (response != "Combustible no creado")
                {

                    await _facturasRepository.SetIdFacturaElectronicaFactura(response, factura.Guid);
                }
                else { return "Combustible no creado"; }
                _validadorGuidAFacturaElectronica.SacarFactura(facturaEntity.Guid);
                return "Ok";
            }
            catch (Exception e)
            {
                _validadorGuidAFacturaElectronica.SacarFactura(facturaEntity.Guid);
                return $"Fallo al crear factura electrónica Razón: {e.Message}";
            }
        }
    }
}
