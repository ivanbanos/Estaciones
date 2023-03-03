﻿using AutoMapper;
using EstacionesServicio.Modelo;
using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Contabilidad.Alegra;
using FacturacionelectronicaCore.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.OrdenDeDespacho
{
    public class OrdenDeDespachoNegocio : IOrdenDeDespachoNegocio
    {
        private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;
        private readonly ITerceroRepositorio _terceroRepositorio;
        private readonly IMapper _mapper;
        private readonly IAlegraFacade _alegraFacade;
        private readonly bool usaAlegra;
        private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;

        public OrdenDeDespachoNegocio(IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio,
                                       IMapper mapper, IAlegraFacade alegraFacade, IOptions<Alegra> alegra, ITerceroRepositorio terceroRepositorio, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica)
        {
            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
            _mapper = mapper;
            _alegraFacade = alegraFacade;
            usaAlegra = alegra.Value.UsaAlegra;
            _terceroRepositorio = terceroRepositorio;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
        }

        public async Task<string> EnviarAFacturacion(Guid ordenGuid)
        {
            if (usaAlegra)
            {
                if (_validadorGuidAFacturaElectronica.FacturaSiendoProceada(ordenGuid))
                {
                    return "Factura electrónica siendo procesada";
                }
                var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorGuid(ordenGuid)).FirstOrDefault();
                if (ordenDeDespachoEntity == null)
                {
                    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                    return "Factura no existe";
                }
                if (ordenDeDespachoEntity.idFacturaElectronica != null)
                {
                    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                    return "Factura electrónica existente";
                }
                var ordenDeDespacho = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity);
                var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenDeDespacho.Identificacion)).FirstOrDefault();
                var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
                if (tercero.idFacturacion == null)
                {

                    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                    return "Tercero no está apto para facturación electrónica";
                }
                ordenDeDespacho.Tercero = tercero;
                var item = await _alegraFacade.GetItem(ordenDeDespacho.Combustible);
                if (item == null)
                {
                    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                    return "Combustible no creado en alegra";
                }
                try
                {
                    var idFacturaElectronica = await _alegraFacade.GenerarFacturaElectronica(ordenDeDespacho, item);
                    await _ordenDeDespachoRepositorio.SetIdFacturaElectronicaOrdenesdeDespacho(idFacturaElectronica, ordenDeDespacho.guid);

                    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                    return "Ok";
                }
                catch (Exception e)
                {
                    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
                    return $"Fallo al crear factura electrónica Razón: {e.Message}";
                }
            }

            return "No maneja factura electrónica";
        }

        /// <inheritdoc />
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

        public Task<int> AddOrdenesImprimir(IEnumerable<FacturasEntity> ordenDeDespachos)
        {
            try
            {
                return _ordenDeDespachoRepositorio.AddOrdenesImprimir(_mapper.Map<IEnumerable<FacturasEntity>, IEnumerable<Repositorio.Entities.FacturasEntity>>(ordenDeDespachos));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task AnularOrdenes(IEnumerable<FacturasEntity> ordenes)
        {
            try
            {
                var ordenesList = _mapper.Map<IEnumerable<FacturasEntity>, IEnumerable<Repositorio.Entities.FacturasEntity>>(ordenes);
                return _ordenDeDespachoRepositorio.AnularOrdenes(ordenesList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho)
        {
            if (usaAlegra) {
                var guids = ordenesDeDespacho.Select(x => x.Guid);
                if (_validadorGuidAFacturaElectronica.FacturasSiendoProceada(guids))
                {
                    return "Factura electrónica siendo procesada";
                }
                var ordenes = new List<Modelo.OrdenDeDespacho>();
                foreach (var guid in ordenesDeDespacho)
                {
                    var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorGuid(guid.Guid)).FirstOrDefault();
                    if (ordenDeDespachoEntity == null)
                    {
                        _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                        return $"Factura {guid.Guid} no existe";
                    }
                    if (ordenDeDespachoEntity.idFacturaElectronica != null)
                    {
                        _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                        return "Una orden ya tiene factura electrónica existente";
                    }
                    ordenes.Add(_mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity));
                }
                if (ordenes.GroupBy(x => x.Identificacion).Count() > 1)
                {
                    _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                    return $"Las ordenes deben pertenecer al mismo tercero";

                }
                var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenes.First().Identificacion)).FirstOrDefault();
                var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
                if (tercero.idFacturacion == null)
                {

                    _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                    return "Tercero no está apto para facturación electrónica";
                }

                var combustibles = new List<string>();
                foreach (var factura in ordenes)
                {
                    if (!combustibles.Contains(factura.Combustible))
                    {
                        combustibles.Add(factura.Combustible);
                    }
                }
                var items = combustibles.Select(x => _alegraFacade.GetItem(x).Result);

                try
                {
                    var idFacturaElectronica = await _alegraFacade.GenerarFacturaElectronica(ordenes, tercero, items);
                    var tasks = new List<Task>();
                    foreach (var orden in ordenes)
                    {
                        tasks.Add(_ordenDeDespachoRepositorio.SetIdFacturaElectronicaOrdenesdeDespacho(idFacturaElectronica, orden.guid));
                    }
                    await Task.WhenAll(tasks);
                    _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                    return "Ok";
                }
                catch (Exception e)
                {
                    _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                    return $"Fallo al crear factura electrónica Razón: {e.Message}";
                }
            }
            return "No maneja factura electrónica";
        }

        public async Task<Modelo.OrdenDeDespacho> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {
            var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion)).FirstOrDefault();
            if (ordenDeDespachoEntity == null)
            {
                return null;
            }
            return _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity);
        }
    }
}
