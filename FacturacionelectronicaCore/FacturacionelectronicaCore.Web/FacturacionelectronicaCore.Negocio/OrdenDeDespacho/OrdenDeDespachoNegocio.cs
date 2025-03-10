﻿using AutoMapper;
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
using FacturasEntity = EstacionesServicio.Modelo.FacturasEntity;

namespace FacturacionelectronicaCore.Negocio.OrdenDeDespacho
{
    public class OrdenDeDespachoNegocio : IOrdenDeDespachoNegocio
    {
        private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;
        private readonly ITerceroRepositorio _terceroRepositorio;
        private readonly IMapper _mapper;
        private readonly IFacturacionElectronicaFacade _alegraFacade;
        private readonly Alegra _alegra;
        private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;

        public OrdenDeDespachoNegocio(IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio,
                                       IMapper mapper, IFacturacionElectronicaFacade alegraFacade, IOptions<Alegra> alegra, ITerceroRepositorio terceroRepositorio, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica)
        {
            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
            _mapper = mapper;
            _alegraFacade = alegraFacade;
            _alegra = alegra.Value;
            _terceroRepositorio = terceroRepositorio;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
        }

        public async Task<string> EnviarAFacturacion(string ordenGuid)
        {

            //if (_validadorGuidAFacturaElectronica.FacturaSiendoProceada(ordenGuid))
            //{
            //    return "Factura electrónica siendo procesada";
            //}
            //var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorGuid(ordenGuid)).FirstOrDefault();
            //if(ordenDeDespachoEntity == null)
            //{
            //    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //    return "Factura no existe";
            //}
            //if (ordenDeDespachoEntity.idFacturaElectronica != null)
            //{
            //    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //    return "Factura electrónica existente";
            //}
            //ordenDeDespachoEntity.Fecha = ordenDeDespachoEntity.Fecha.ToLocalTime().AddHours(3);
            //var ordenDeDespacho = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity);
            //var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenDeDespacho.Identificacion)).FirstOrDefault();
            //var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            //if (_alegra.ValidaTercero && tercero.idFacturacion == null)
            //{

            //    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //    return "Tercero no está apto para facturación electrónica";
            //}
            //ordenDeDespacho.Tercero = tercero;
            
            //try
            //{
            //    var response = await _alegraFacade.GenerarFacturaElectronica(ordenDeDespacho, ordenDeDespacho.Tercero, Guid.Parse(((OrdenesMongo)ordenDeDespachoEntity).EstacionGuid));
            //    if (response != "Combustible no creado")
            //    {

            //        await _ordenDeDespachoRepositorio.SetIdFacturaElectronicaOrdenesdeDespacho(response, ordenDeDespacho.guid);
            //    }
            //    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid); 
            //    return "Ok";
            //}catch(Exception e)
            //{
            //    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //    return $"Fallo al crear factura electrónica Razón: {e.Message}";
            //}
            return "Ok";
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(FiltroBusqueda filtroOrdenDeDespacho)
        {
            try
            {
                var ordenesDeDespacho = await _ordenDeDespachoRepositorio.GetOrdenesDeDespacho(filtroOrdenDeDespacho.FechaInicial, 
                        filtroOrdenDeDespacho.FechaFinal, filtroOrdenDeDespacho.Identificacion, filtroOrdenDeDespacho.NombreTercero, filtroOrdenDeDespacho.Estacion);
                var ordenes = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenesDeDespacho);

                var nombresPorIdentificacion = new Dictionary<string, string>();
                foreach (var factura in ordenes)
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
                    if (factura.Precio > 20000)
                    {

                        factura.Precio /= 10;
                        factura.SubTotal /= 10;
                        factura.Total /= 10;
                        factura.Descuento /= 10;
                    }
                    factura.NombreTercero = nombresPorIdentificacion[factura.Identificacion];
                    factura.Fecha = factura.Fecha.ToLocalTime().AddHours(3);
                }
                return ordenes.OrderBy(x=>x.IdVentaLocal);
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
            var guids = ordenesDeDespacho.Select(x => x.Guid);
            if (_validadorGuidAFacturaElectronica.FacturasSiendoProceada(guids))
            {
                return "Factura electrónica siendo procesada";
            }
            var ordenes = new List<Modelo.OrdenDeDespacho>();
            foreach(var guid in ordenesDeDespacho)
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

                ordenDeDespachoEntity.Fecha = ordenDeDespachoEntity.Fecha.ToLocalTime().AddHours(3);
                ordenes.Add( _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity));
            }
            if(ordenes.GroupBy(x=>x.Identificacion).Count() > 1)
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
            var items = combustibles.Select(x => _alegraFacade.GetItem(x, null).Result);

            try
            {
                var idFacturaElectronica = "error";// await _alegraFacade.GenerarFacturaElectronica(ordenes, tercero, items);
                foreach (var orden in ordenes)
                {
                    await _ordenDeDespachoRepositorio.SetIdFacturaElectronicaOrdenesdeDespacho(idFacturaElectronica, orden.guid);
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

        public async Task<Modelo.OrdenDeDespacho> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {


            var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion)).FirstOrDefault();
            if (ordenDeDespachoEntity == null)
            {
                return null;
            }
            var factura = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity);
            if (factura.Precio > 20000)
            {

                factura.Precio /= 10;
                factura.SubTotal /= 10;
                factura.Total /= 10;
                factura.Descuento /= 10;
            }
            return factura;
        }

        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> ObtenerOrdenesPorTurno(Guid turno)
        {
            var facturasEntity = await _ordenDeDespachoRepositorio.ObtenerOrdenesPorTurno(turno);

            var ordenes = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(facturasEntity);

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
            return ordenes;
        }

        public async Task<string> EnviarAFacturacion(int idVentaLocal, Guid estacion)
        {
            //var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion)).FirstOrDefault();
            //if (ordenDeDespachoEntity == null)
            //{
            //    return null;
            //}
            //var ordenGuid = ordenDeDespachoEntity.guid;
            //if (_validadorGuidAFacturaElectronica.FacturaSiendoProceada(ordenGuid))
            //{
            //    return "Factura electrónica siendo procesada";
            //}
            //if (ordenDeDespachoEntity == null)
            //{
            //    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //    return "Factura no existe";
            //}
            //if (ordenDeDespachoEntity.idFacturaElectronica != null)
            //{
            //    _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //    return "Factura electrónica existente";
            //}
            //ordenDeDespachoEntity.Fecha = ordenDeDespachoEntity.Fecha.ToLocalTime().AddHours(-7); 
            //var ordenDeDespacho = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity);

            //if ((ordenDeDespacho.Fecha > DateTime.Now.AddDays(-1) || (_alegra.EnviaMes && DateTime.Now.Month == ordenDeDespacho.Fecha.Month)) && (_alegra.EnviaCreditos || !ordenDeDespacho.FormaDePago.ToLower().Contains("dito")))
            //{
                
            //    var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenDeDespacho.Identificacion)).FirstOrDefault();
            //    var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            //    if (_alegra.ValidaTercero && tercero.idFacturacion == null)
            //    {

            //        _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //        return "Tercero no está apto para facturación electrónica";
            //    }
            //    ordenDeDespacho.Tercero = tercero;

            //    try
            //    {
            //        var response = await _alegraFacade.GenerarFacturaElectronica(ordenDeDespacho, ordenDeDespacho.Tercero, Guid.Parse(((OrdenesMongo)ordenDeDespachoEntity).EstacionGuid));
            //        if (response != "Combustible no creado")
            //        {

            //            await _ordenDeDespachoRepositorio.SetIdFacturaElectronicaOrdenesdeDespacho(response, ordenDeDespacho.guid);
            //        }
            //        _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //        return "Ok";
            //    }
            //    catch (Exception e)
            //    {
            //        _validadorGuidAFacturaElectronica.SacarFactura(ordenGuid);
            //        return $"Fallo al crear factura electrónica Razón: {e.Message}";
            //    }
            //}
            return "Ok";
        }
    }
}
