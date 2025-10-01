using AutoMapper;
using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal
{
    public class ManejadorInformacionLocalNegocio : IManejadorInformacionLocalNegocio
    {
    private readonly IEstacionesRepository _estacionesRepository;
    private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;
    private readonly ITipoIdentificacionRepositorio _tipoIdentificacionRepositorio;
    private readonly ITerceroRepositorio _terceroRepositorio;
    private readonly IResolucionRepositorio _resolucionRepositorio;
    private readonly IMapper _mapper;
    private readonly IApiContabilidad _apiContabilidad;
    private readonly IFacturacionElectronicaFacade _alegraFacade;
    private readonly IFacturaCanastillaRepository _facturaCanastillaRepository;
    private readonly ICanastillaRepositorio _canastillaRepositorio;
    private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;
    private readonly Alegra _alegra;

        public ManejadorInformacionLocalNegocio(ITerceroRepositorio tercerosRepositorio, IMapper mapper, IResolucionRepositorio resolucionRepositorio,
            IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio,
            IApiContabilidad apiContabilidad, ITipoIdentificacionRepositorio tipoIdentificacionRepositorio,
            IFacturacionElectronicaFacade alegraFacade, IOptions<Alegra> alegra, IFacturaCanastillaRepository facturaCanastillaRepository, ICanastillaRepositorio canastillaRepositorio, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica, IEstacionesRepository estacionesRepository)
        {
            _terceroRepositorio = tercerosRepositorio;
            _resolucionRepositorio = resolucionRepositorio;
            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
            _apiContabilidad = apiContabilidad;
            _mapper = mapper;

            _alegra = alegra.Value;
            _alegraFacade = alegraFacade;
            _tipoIdentificacionRepositorio = tipoIdentificacionRepositorio;
            _facturaCanastillaRepository = facturaCanastillaRepository;
            _canastillaRepositorio = canastillaRepositorio;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
            _estacionesRepository = estacionesRepository;
        }

        public async Task EnviarOrdenesDespacho(IEnumerable<Modelo.OrdenDeDespacho> ordenDeDespachos, Guid estacion)
        {
            await EnviarTerceros(ordenDeDespachos.Select(x => x.Tercero));
            Console.WriteLine("EnvioDirecto " + _alegra.EnvioDirecto);
            Console.WriteLine("EnviaCreditos " + _alegra.EnviaCreditos);
            foreach (var x in ordenDeDespachos)
            {
                if (_alegra.MultiplicarPorDies)
                {

                    x.Precio = _alegra.MultiplicarPorDies ? x.Precio * 10 : x.Precio;
                }
                var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(x.IdVentaLocal, estacion)).FirstOrDefault();

                if ((ordenDeDespachoEntity == null
                    || ordenDeDespachoEntity.idFacturaElectronica == null
                    || ordenDeDespachoEntity.idFacturaElectronica.StartsWith("error")
                    || ordenDeDespachoEntity.idFacturaElectronica.StartsWith("Error"))
                    && _alegra.EnvioDirecto)
                {
                    if ((x.Fecha > DateTime.Now.AddMonths(-1)
                        || (_alegra.EnviaMes && DateTime.Now.AddMonths(-1) < x.Fecha))
                        && (_alegra.EnviaCreditos || (!x.FormaDePago.ToLower().Contains("dir") && !x.FormaDePago.ToLower().Contains("calibra") && !x.FormaDePago.ToLower().Contains("puntos"))))
                    {

                        var id = await EnviarAFacturacion(x, estacion);
                        await Task.Delay(2000); // Esperar 2 segundos para no saturar el servicio
                        Console.WriteLine(id);

                        x.idFacturaElectronica = id;
                    }

                }
                if (ordenDeDespachoEntity == null
                    || ordenDeDespachoEntity.idFacturaElectronica == null
                    || ordenDeDespachoEntity.idFacturaElectronica.Contains("error") || !_alegra.Desactivado)
                {

                    var ordenesentity = new List<Repositorio.Entities.OrdenDeDespacho>
                    {
                        new Repositorio.Entities.OrdenDeDespacho()
                        {
                        guid = x.guid.ToString(),
                        Cantidad = x.Cantidad,
                        Cara = x.Cara,
                        Combustible = x.Combustible,
                        Descuento = x.Descuento,
                        Estado = x.Estado,
                        Fecha = x.Fecha,
                        FechaReporte = x.FechaReporte,
                        FechaProximoMantenimiento = x.FechaProximoMantenimiento,
                        FormaDePago = x.FormaDePago,
                        Identificacion = x.Identificacion,
                        IdentificacionTercero = x.IdentificacionTercero,
                        IdEstacion = x.IdEstacion,
                        IdEstadoActual = x.IdEstadoActual,
                        IdFactura = x.IdFactura,
                        IdInterno = x.IdInterno,
                        IdLocal = x.IdLocal,
                        IdTerceroLocal = x.IdTerceroLocal,
                        IdVentaLocal = x.IdVentaLocal,
                        Kilometraje = x.Kilometraje,
                        Manguera = x.Manguera,
                        NombreTercero = x.NombreTercero,
                        Placa = x.Placa,
                        Precio = x.Precio,
                        SubTotal = x.SubTotal,
                        Surtidor = x.Surtidor,
                        Total = x.Total,
                        idFacturaElectronica = x.idFacturaElectronica ?? ordenDeDespachoEntity?.idFacturaElectronica,
                        Vendedor = x.Vendedor,
                        }
                    };
                    await _ordenDeDespachoRepositorio.AddRange(ordenesentity, estacion);
                }



            }
        }

        private async Task<string> EnviarAFacturacion(Modelo.OrdenDeDespacho ordenDeDespacho, Guid estacion)
        {



            try
            {
                var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenDeDespacho.Identificacion)).FirstOrDefault();
                if (terceroEntity != null)
                {
                    var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
                    if (_alegra.ValidaTercero && tercero.idFacturacion == null)
                    {

                        return "error:Tercero no está apto para facturación electrónica";
                    }
                    ordenDeDespacho.Tercero = tercero;
                    var response = await _alegraFacade.GenerarFacturaElectronica(ordenDeDespacho, ordenDeDespacho.Tercero, estacion);

                    return response;
                }
                return $"error:Tercero {ordenDeDespacho.Identificacion} no encontrado";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return $"error:{e.Message}:{e.StackTrace}";
            }
        }

        public async Task EnviarTerceros(IEnumerable<Modelo.Tercero> terceros)
        {
            try
            {

                terceros = terceros.Where(x => !string.IsNullOrEmpty(x.Identificacion) && !string.IsNullOrEmpty(x.DescripcionTipoIdentificacion));
                await _terceroRepositorio.AddOrUpdate(_mapper.Map<IEnumerable<TerceroInput>>(terceros));
                if (_alegra.Proveedor == "ALEGRA")
                {
                    foreach (var tercero in terceros)
                    {

                        if (tercero.idFacturacion != null)
                        {
                            await _alegraFacade.ActualizarTercero(tercero, tercero.idFacturacion);
                        }
                        else
                        {
                            var idFacturacion = await _alegraFacade.GenerarTercero(tercero);
                            await _terceroRepositorio.SetIdFacturacion(tercero.Guid, idFacturacion);
                        }
                    }
                }
            }
            catch (Exception) { }
        }


        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(Guid estacion)
        {
            try
            {
                IEnumerable<Repositorio.Entities.OrdenDeDespacho> ordenes = await _ordenDeDespachoRepositorio.GetOrdenesImprimir(estacion);
                var ordenesDeDespacho = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenes);
                return ordenesDeDespacho;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<IEnumerable<Modelo.Tercero>> GetTercerosActualizados(Guid estacion)
        {
            try
            {
                var terceros = await _terceroRepositorio.GetTercerosActualizados(estacion);
                return _mapper.Map<IEnumerable<Repositorio.Entities.Tercero>, IEnumerable<Modelo.Tercero>>(terceros);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<IEnumerable<Modelo.Tercero>> GetTercerosActualizados()
        {
            try
            {
                var terceros = await _terceroRepositorio.GetTercerosActualizados();
                return _mapper.Map<IEnumerable<Repositorio.Entities.Tercero>, IEnumerable<Modelo.Tercero>>(terceros);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<string>> GetTipos()
        {
            return await _tipoIdentificacionRepositorio.GetTipos();
        }

        public async Task<string> GetInfoFacturaElectronica(int idVentaLocal, Guid estacion)
        {

            var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion))?.FirstOrDefault();
            if (ordenDeDespachoEntity != null)
            {
                if (ordenDeDespachoEntity.idFacturaElectronica != null)
                {

                    if (ordenDeDespachoEntity.idFacturaElectronica.Split(':')[0] != "error")
                    {
                        if (_alegra.Proveedor == "SIIGO")
                        {
                            return await _alegraFacade.GetFacturaElectronica(ordenDeDespachoEntity.idFacturaElectronica, estacion);
                        }
                        var info = ordenDeDespachoEntity.idFacturaElectronica.Split(':');
                        return $"Factura electrónica\n\r{info[1]}\n\rCUFE:\n\r{info[2]}";
                    }
                }
                return null;
            }
            return null;
        }

        public async Task<int> AddFacturaCanastilla(IEnumerable<Modelo.FacturaCanastilla> facturas, Guid estacion)
        {
            try
            {
                
                await EnviarTerceros(facturas.Select(x => x.terceroId));
                // Get all canastillas from repo for this estacion
                var canastillasRepo = await _canastillaRepositorio.GetCanastillas(estacion);
                foreach (var factura in facturas)
                {
                    try
                    {
                        // Replace campoextra in each factura.canastillas from repo
                        foreach (var canastilla in factura.canastillas)
                        {
                            if (canastilla.Canastilla != null)
                            {
                                var repoCanastilla = canastillasRepo.FirstOrDefault(c => c.guid == canastilla.Canastilla.guid);
                                if (repoCanastilla != null)
                                {
                                    canastilla.Canastilla.campoextra = repoCanastilla.campoextra;
                                }
                            }
                        }

                        var facturaCanastilla = await _facturaCanastillaRepository.GetFacturaPorIdCanastilla(factura.FacturasCanastillaId, estacion);
                        var idFactruraElectronica = "";
                        if (facturaCanastilla == null
                            || facturaCanastilla.idFacturaElectronica == null
                            || facturaCanastilla.idFacturaElectronica.StartsWith("error")
                            || facturaCanastilla.idFacturaElectronica.StartsWith("Error"))
                        {
                            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(factura.terceroId.Identificacion)).FirstOrDefault();
                            if (terceroEntity != null)
                            {
                                var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
                                if (_alegra.ValidaTercero && tercero.idFacturacion == null)
                                {
                                    idFactruraElectronica = "error:Tercero no está apto para facturación electrónica";
                                }
                                var response = await _alegraFacade.GenerarFacturaElectronica(factura, tercero, estacion);
                                await Task.Delay(2000);
                                idFactruraElectronica = response;
                                factura.idFacturaElectronica = idFactruraElectronica;
                            }
                            else
                            {
                                if ((factura.fecha > DateTime.Now.AddMonths(-1)
                                    || (_alegra.EnviaMes && DateTime.Now.AddMonths(-1) < factura.fecha))
                                    && (_alegra.EnviaCreditos || (!factura.codigoFormaPago.Descripcion.ToLower().Contains("dir") && !factura.codigoFormaPago.Descripcion.ToLower().Contains("calibra") && !factura.codigoFormaPago.Descripcion.ToLower().Contains("puntos"))))
                                {
                                    var response = await _alegraFacade.GenerarFacturaElectronica(factura, factura.terceroId, estacion);
                                    await Task.Delay(2000);
                                    idFactruraElectronica = response;
                                    factura.idFacturaElectronica = idFactruraElectronica;
                                }
                            }
                        }

                        var facturaRepo = _mapper.Map<Modelo.FacturaCanastilla, Repositorio.Entities.FacturaCanastilla>(factura);
                        await _facturaCanastillaRepository.Add(facturaRepo, facturaRepo.canastillas, estacion);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        factura.idFacturaElectronica = ex.Message + ex.StackTrace;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
        }

        public async Task<ResolucionElectronica> GetResolucionElectronica()
        {
            var estacion = await _estacionesRepository.GetEstaciones();
            return await _alegraFacade.GetResolucionElectronica(estacion.First().guid.ToString());
        }

        public async Task<string> JsonFacturaElectronica(int idVentaLocal, Guid estacion)
        {
            var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion))?.FirstOrDefault();
            var ordenesDeDespacho = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity);

            if (ordenDeDespachoEntity == null)
            {
                return "error:Venta no existe en el sistema";
            }
            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenesDeDespacho.Identificacion))?.FirstOrDefault();
            var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            if (_alegra.ValidaTercero && tercero.idFacturacion == null)
            {

                return "error:Tercero no está apto para facturación electrónica";
            }
            ordenesDeDespacho.Tercero = tercero;
            return await _alegraFacade.getJson(ordenesDeDespacho, estacion) + " respuesta " + (ordenDeDespachoEntity.idFacturaElectronica ?? "No generada");
        }

        public async Task<string> GetInfoFacturaElectronicaCanastilla(int consecutivo, Guid estacion)
        {
            var ordenDeDespachoEntity = (await _facturaCanastillaRepository.GetFacturaPorIdCanastilla(consecutivo, estacion));
            if (ordenDeDespachoEntity != null)
            {
                if (ordenDeDespachoEntity.idFacturaElectronica != null)
                {

                    if (ordenDeDespachoEntity.idFacturaElectronica.Split(':')[0] != "error")
                    {
                        if (_alegra.Proveedor == "SIIGO")
                        {
                            return await _alegraFacade.GetFacturaElectronica(ordenDeDespachoEntity.idFacturaElectronica, estacion);
                        }
                        var info = ordenDeDespachoEntity.idFacturaElectronica.Split(':');
                        return $"Factura electrónica\n\r{info[1]}\n\rCUFE:\n\r{info[2]}";
                    }
                }
                return null;
            }
            return null;
        }

        public async Task AgregarFechaReporteFactura(IEnumerable<Modelo.FacturaFechaReporte> facturaFechaReporte, Guid estacion)
        {
            var facturas = _mapper.Map<IEnumerable<Modelo.FacturaFechaReporte>, IEnumerable<Repositorio.Entities.FacturaFechaReporte>>(facturaFechaReporte);
            await _ordenDeDespachoRepositorio.AgregarFechaReporteFactura(facturas, estacion);
        }

    }
}
