﻿using FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal;
using FacturacionelectronicaCore.Negocio.Modelo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManejadorInformacionLocalController : ControllerBase
    {
        private readonly IManejadorInformacionLocalNegocio _manejadorInformacionLocalNegocio;

        public ManejadorInformacionLocalController(IManejadorInformacionLocalNegocio manejadorInformacionLocalNegocio)
        {
            _manejadorInformacionLocalNegocio = manejadorInformacionLocalNegocio;
        }

        [HttpGet("GetGuidsFacturasPendientes/{estacion}")]
        public async Task<ActionResult> GetGuidsFacturasPendientes(Guid estacion)
        {
            return Ok(null);
        }


        [HttpPost("EnviarResolucion")]
        public async Task<ActionResult> EnviarResolucion(RequestEnvioResolucion requestEnvioResolucion)
        {

            // await _manejadorInformacionLocalNegocio.EnviarResolucion(requestEnvioResolucion);

            return Ok();
        }

        [HttpPost("EnviarTerceros")]
        public async Task<ActionResult> EnviarTerceros(IEnumerable<Negocio.Modelo.Tercero> terceros)
        {

            await _manejadorInformacionLocalNegocio.EnviarTerceros(terceros);

            return Ok();
        }

        [HttpPost("EnviarFacturas")]
        public async Task<ActionResult> EnviarFacturas(RequestEnviarFacturas requestEnviarFacturas)
        {

            await _manejadorInformacionLocalNegocio.EnviarOrdenesDespacho(requestEnviarFacturas.ordenDeDespachos, requestEnviarFacturas.Estacion);
            return Ok();
        }


        [HttpPost("AgregarFechaReporteFactura")]
        public async Task<ActionResult> AgregarFechaReporteFactura(RequestCambiarFechasReporte requestCambiarFechasReporte)
        {
            await _manejadorInformacionLocalNegocio.AgregarFechaReporteFactura(requestCambiarFechasReporte.facturas, requestCambiarFechasReporte.Estacion);

            return Ok();
        }

        [HttpGet("GetOrdenesDeDespachoImprimir/{estacion}")]
        public async Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespachoImprimir(Guid estacion)
        => await _manejadorInformacionLocalNegocio.GetOrdenesDeDespacho(estacion);

        [HttpGet("GetFacturasImprimir/{estacion}")]
        public async Task<IEnumerable<Factura>> GetFacturasImprimir(Guid estacion)
        {
            
            return new List<Factura>();
        }

        [HttpGet("GetTercerosActualizados/{estacion}")]
        public async Task<ActionResult<IEnumerable<Negocio.Modelo.Tercero>>> GetTercerosActualizados(Guid estacion)
        => Ok(await _manejadorInformacionLocalNegocio.GetTercerosActualizados(estacion));

        [HttpGet("GetTercerosActualizados")]
        public async Task<ActionResult<IEnumerable<Negocio.Modelo.Tercero>>> GetTercerosActualizados()
        => Ok(await _manejadorInformacionLocalNegocio.GetTercerosActualizados());

        [HttpGet("GetTipos")]
        public async Task<ActionResult<IEnumerable<string>>> GetTipos()
        => Ok(await _manejadorInformacionLocalNegocio.GetTipos());


        [HttpGet("GetInfoFacturaElectronica/{idVentaLocal}/estacion/{estacion}")]
        public async Task<ActionResult<string>> GetInfoFacturaElectronica(int idVentaLocal, Guid estacion)
        => Ok(await _manejadorInformacionLocalNegocio.GetInfoFacturaElectronica(idVentaLocal, estacion));

        [HttpGet("GetInfoFacturaElectronicaCanastilla/{consecutivo}/estacion/{estacion}")]
        public async Task<ActionResult<string>> GetInfoFacturaElectronicaCanastilla(int consecutivo, Guid estacion)
        => Ok(await _manejadorInformacionLocalNegocio.GetInfoFacturaElectronicaCanastilla(consecutivo, estacion));

        [HttpGet("JsonFacturaElectronica/{idVentaLocal}/estacion/{estacion}")]
        public async Task<ActionResult<string>> JsonFacturaElectronica(int idVentaLocal, Guid estacion)
        => Ok(await _manejadorInformacionLocalNegocio.JsonFacturaElectronica(idVentaLocal, estacion));


        [HttpPost("AddFacturaCanastilla")]
        public async Task<ActionResult<int>> AddFacturaCanastilla(RequestfacturasCanastilla requestCambiarFechasReporte)
        => Ok(await _manejadorInformacionLocalNegocio.AddFacturaCanastilla(requestCambiarFechasReporte.facturas, requestCambiarFechasReporte.estacion));


        [HttpGet("GetResolucionElectronica")]
        public async Task<ActionResult<ResolucionElectronica>> GetResolucionElectronica()
        => Ok(await _manejadorInformacionLocalNegocio.GetResolucionElectronica());
        
    }
}