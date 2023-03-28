﻿using AutoMapper;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturacionelectronicaCore.Negocio.Contabilidad.Alegra;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal
{
    public class ManejadorInformacionLocalNegocio : IManejadorInformacionLocalNegocio
    {
        private readonly IFacturasRepository _facturasRepository;
        private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;
        private readonly ITipoIdentificacionRepositorio _tipoIdentificacionRepositorio;
        private readonly ITerceroRepositorio _terceroRepositorio;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private readonly IMapper _mapper;
        private readonly IApiContabilidad _apiContabilidad;
        private readonly IFacturacionElectronicaFacade _alegraFacade;
        private readonly bool usaAlegra;
        private readonly IFacturaCanastillaRepository _facturaCanastillaRepository;

        public ManejadorInformacionLocalNegocio(IFacturasRepository facturasRepository,
            ITerceroRepositorio tercerosRepositorio, IMapper mapper, IResolucionRepositorio resolucionRepositorio,
            IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio,
            IApiContabilidad apiContabilidad, ITipoIdentificacionRepositorio tipoIdentificacionRepositorio,
            IFacturacionElectronicaFacade alegraFacade, IOptions<Alegra> alegra, IFacturaCanastillaRepository facturaCanastillaRepository)
        {
            _facturasRepository = facturasRepository;
            _terceroRepositorio = tercerosRepositorio;
            _resolucionRepositorio = resolucionRepositorio;
            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
            _apiContabilidad = apiContabilidad;
            _mapper = mapper;
            usaAlegra = alegra.Value.UsaAlegra;
            _alegraFacade = alegraFacade;
            _tipoIdentificacionRepositorio = tipoIdentificacionRepositorio;
            _facturaCanastillaRepository = facturaCanastillaRepository;
        }
        public async Task EnviarFacturas(IEnumerable<Modelo.Factura> facturas, Guid estacion)
        {

            var facturasRepositorio = _mapper.Map<IEnumerable<Repositorio.Entities.Factura>>(facturas);

            await _facturasRepository.AddRange(facturasRepositorio, estacion);

            //_apiContabilidad.EnviarFacturas(facturas);
        }

        public async Task EnviarOrdenesDespacho(IEnumerable<Modelo.OrdenDeDespacho> ordenDeDespachos, Guid estacion)
        {
            await _ordenDeDespachoRepositorio.AddRange(ordenDeDespachos.Select(x => 
            new Repositorio.Entities.OrdenDeDespacho()
            {
                guid = x.guid,
                Cantidad = x.Cantidad,
                Cara = x.Cara,
                Combustible = x.Combustible,
                Descuento = x.Descuento,
                Estado = x.Estado,
                Fecha = x.Fecha,
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
                Vendedor = x.Vendedor,
            }), estacion);
        }

        public async Task EnviarResolucion(RequestEnvioResolucion requestEnvioResolucion)
        {
            var resolucion = requestEnvioResolucion.Resolucion;
            var guidsFacturasPendientes = requestEnvioResolucion.guidsFacturasPendientes;
            int consecutivoActual = resolucion.ConsecutivoActual - guidsFacturasPendientes.Count;
            foreach(var facturaGuid in guidsFacturasPendientes)
            {
                await _facturasRepository.setConsecutivoFacturaPendiente(facturaGuid, ++consecutivoActual);
            }
            await _resolucionRepositorio.UpdateConsecutivoResolucion(resolucion.ConsecutivoActual); 
        }

        public async Task EnviarTerceros(IEnumerable<Modelo.Tercero> terceros)
        {
            try
            {
                terceros = terceros.Where(x => !string.IsNullOrEmpty(x.Identificacion) && !string.IsNullOrEmpty(x.DescripcionTipoIdentificacion));
                await _terceroRepositorio.AddOrUpdate(_mapper.Map<IEnumerable<TerceroInput>>(terceros));
            }
            catch (Exception) { }
        }

        public async Task<IEnumerable<string>> GetGuidsFacturasPendientes(Guid estacion)
        {
            var facturasPendientes = await _facturasRepository.GetFacturaByEstado("Pendiente", estacion);
            return facturasPendientes.Select(f => f.Guid.ToString());
        }

        public async Task<IEnumerable<Modelo.Factura>> GetFacturasImprimir(Guid estacion)
        {
            try
            {
                var facturas = _mapper.Map<IEnumerable<Repositorio.Entities.Factura>, IEnumerable<Modelo.Factura>>(await _facturasRepository.GetFacturasImprimir(estacion));
                foreach (var factura in facturas)
                {
                    if(factura.Combustible == null)
                    {
                        var oredenes = await _ordenDeDespachoRepositorio.GetOrdenesDeDespachoByFactura(factura.Guid);
                        factura.Ordenes = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(oredenes);
                    }
                }
                return facturas;
            }
            catch (Exception)
            {
                throw;
            }
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
        public async Task  AgregarFechaReporteFactura(IEnumerable<Modelo.FacturaFechaReporte> facturaFechaReporte, Guid estacion)
        {
            var facturas = _mapper.Map<IEnumerable<Modelo.FacturaFechaReporte>, IEnumerable<Repositorio.Entities.FacturaFechaReporte>>(facturaFechaReporte);
            await _facturasRepository.AgregarFechaReporteFactura(facturas, estacion);
        }

        public async Task<IEnumerable<string>> GetTipos()
        {
            return await _tipoIdentificacionRepositorio.GetTipos();
        }

        public async Task<string> GetInfoFacturaElectronica(int idVentaLocal, Guid estacion)
        {
            var facturaEntity = (await _facturasRepository.ObtenerFacturaPorIdVentaLocal(idVentaLocal, estacion)).FirstOrDefault();
            if (facturaEntity != null)
            {
                if(facturaEntity.idFacturaElectronica != null)
                {
                    var factura = await _alegraFacade.GetFacturaElectronica(facturaEntity.idFacturaElectronica.Split(':')[1]);
                    return $"Factura electrónica {factura.numberTemplate.fullNumber}\n\rCUFE: {factura.stamp.cufe}";
                }
                return null;
            }
            var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion)).FirstOrDefault();
            if (ordenDeDespachoEntity != null)
            {
                if (ordenDeDespachoEntity.idFacturaElectronica != null)
                {
                    var factura = await _alegraFacade.GetFacturaElectronica(ordenDeDespachoEntity.idFacturaElectronica.Split(':')[1]);
                    return $"Factura electrónica {factura.numberTemplate.fullNumber}\n\rCUFE: {factura.stamp.cufe}";
                }
                return null;
            }
            return null;
        }

        public async Task<int> AddFacturaCanastilla(IEnumerable<Modelo.FacturaCanastilla> facturas, Guid estacion)
        {
            try
            {
                var facturasRepo = _mapper.Map<IEnumerable<Modelo.FacturaCanastilla>, IEnumerable<Repositorio.Entities.FacturaCanastilla>>(facturas);

                foreach (var factura in facturasRepo)
                {
                    await _facturaCanastillaRepository.Add(factura, factura.canastillas, estacion);
                }
                return 1;
            }
            catch(Exception ex)
            {
                return -1;
            }
        }

        public async Task<ResolucionElectronica> GetResolucionElectronica()
        {
           return await _alegraFacade.GetResolucionElectronica();
        }
    }
}
