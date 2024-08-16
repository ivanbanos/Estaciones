using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Negocio.OrdenDeDespacho;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenesDeDespachoController : ControllerBase
    {
        private readonly IOrdenDeDespachoNegocio _ordenDeDespachoNegocio;

        public OrdenesDeDespachoController(IOrdenDeDespachoNegocio ordenDeDespachoNegocio)
        {
            _ordenDeDespachoNegocio = ordenDeDespachoNegocio;
        }
        /// <summary>
        /// Obtiene una lista de ordenes de despacho por Fecha inicial o Fecha final o Identiciación o Nombre de tercero
        /// </summary>
        /// <param name="fechaInicial">usuario</param>
        /// <param name="fechaFinal">fechaFinal</param>
        /// <param name="identificacionTercero">usuario</param>
        /// <param name="nombreTercero">fechaFinal</param>
        /// <returns>Coleccion de Ordenes de desapacho con Identificacion y Nombre de tercero</returns>
        [HttpPost]
        public async Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespacho(FiltroBusqueda filtroOrdenDeDespacho)
        => await _ordenDeDespachoNegocio.GetOrdenesDeDespacho(filtroOrdenDeDespacho);

        [HttpPost("AddOrdenesImprimir")]
        public async Task<ActionResult<int>> AddOrdenesImprimir(IEnumerable<FacturasEntity> ordenes)
        {
            if (!ordenes.Any())
            {
                return BadRequest();
            }

            await _ordenDeDespachoNegocio.AddOrdenesImprimir(ordenes);

            return Ok();
        }

        [HttpPost("AnularOrdenes")]
        public async Task<ActionResult<int>> AnularOrdenes(IEnumerable<FacturasEntity> ordenes)
        {
            if (!ordenes.Any())
            {
                return BadRequest();
            }

            await _ordenDeDespachoNegocio.AnularOrdenes(ordenes);
            return Ok();
        }

        [HttpGet("EnviarFacturacion/{ordenGuid}")]
        public async Task<ActionResult<string>> EnviarFacturacion(string ordenGuid)
        {
            var result = await _ordenDeDespachoNegocio.EnviarAFacturacion(ordenGuid);
            return Ok(result);
        }
        [HttpGet("EnviarFacturacion/{idVentaLocal}/{estacion}")]
        public async Task<ActionResult<string>> EnviarFacturacion(int idVentaLocal, Guid estacion)
        {
            var result = await _ordenDeDespachoNegocio.EnviarAFacturacion(idVentaLocal, estacion);
            return Ok(result);
        }

        [HttpPost("CrearFacturaOrdenesDeDespacho")]
        public async Task<ActionResult<string>> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho)
        {

            if (!ordenesDeDespacho.Any())
            {
                return BadRequest();
            }

            var response = await _ordenDeDespachoNegocio.CrearFacturaOrdenesDeDespacho(ordenesDeDespacho);

            return Ok(response);
        }

        [HttpGet("ObtenerOrdenDespachoPorIdVentaLocal/{idVentaLocal}/{estacion}")]
        public async Task<ActionResult<Guid>> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {


            var response = await _ordenDeDespachoNegocio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion);

            return Ok(response.guid);
        }


        [HttpGet("ObtenerOrdenesPorTurno/{turno}")]
        public async Task<ActionResult<IEnumerable<OrdenDeDespacho>>> ObtenerOrdenesPorTurno(Guid turno)
        {
            var response = await _ordenDeDespachoNegocio.ObtenerOrdenesPorTurno(turno);
            return Ok(response);
        }
    }
}
