using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.Factura;
using FacturacionelectronicaCore.Negocio.Modelo;
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
    public class FacturaController : ControllerBase
    {
        private readonly IFacturaNegocio _facturaNegocio;

        public FacturaController(IFacturaNegocio facturaNegocio)
        {
            _facturaNegocio = facturaNegocio;
        }

        

        [HttpPost("GetFactura")]
        public async Task<IEnumerable<Factura>> BuscarFacturas(FiltroBusqueda filtroFactura)
        => await _facturaNegocio.GetFacturas(filtroFactura);


        [HttpPost("AddFacturasImprimir")]
        public async Task<ActionResult> AddFacturasImprimir(IEnumerable<FacturasEntity> facturas)
        {

            await _facturaNegocio.AddFacturasImprimir(facturas);
            return Ok();
        }
        

        [HttpPost("AnularFacturas")]
        public async Task<ActionResult<int>> AnularFacturas(IEnumerable<FacturasEntity> facturas)
        {
            if (!facturas.Any())
            {
                return BadRequest();
            }

            var response = await _facturaNegocio.AnularFacturas(facturas);

            return Ok(response);
        }

        [HttpPost("GetConsolidado")]
        public async Task<ActionResult<ReporteFiscal>> GetConsolidado(FiltroBusqueda filtroFactura) 
        {
            var reporteFiscal = await _facturaNegocio.GetReporteFiscal(filtroFactura);

            if (reporteFiscal == null)
            {
                return NotFound();
            }

            return Ok(reporteFiscal);
        }
        [HttpGet("EnviarFacturacion/{ordenGuid}")]
        public async Task<ActionResult> EnviarFacturacion(string ordenGuid)
        {
            var result = await _facturaNegocio.EnviarAFacturacion(ordenGuid);
            return Ok(result);
        }
        [HttpGet("EnviarFacturacion/{idVentaLocal}/{estacion}")]
        public async Task<ActionResult> EnviarFacturacion(int idVentaLocal, Guid estacion)
        {
            var result = await _facturaNegocio.EnviarAFacturacion(idVentaLocal, estacion);
            return Ok(result);
        }
        

        [HttpPost("CrearFacturaFacturas")]
        public async Task<ActionResult<string>> CrearFacturaFacturas(IEnumerable<FacturasEntity> facturasGuis)
        {

            if (!facturasGuis.Any())
            {
                return BadRequest();
            }

            var response = await _facturaNegocio.CrearFacturaFacturas(facturasGuis);

            return Ok(response);
        }

        [HttpGet("ObtenerFacturaPorIdVentaLocal/{idVentaLocal}/{estacion}")]
        public async Task<ActionResult<Guid>> ObtenerFacturaPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {


            var response = await _facturaNegocio.ObtenerFacturaPorIdVentaLocal(idVentaLocal, estacion);

            return Ok(response.Guid);
        }


        [HttpGet("ObtenerFacturasPorTurno/{turno}")]
        public async Task<ActionResult<IEnumerable<Factura>>> ObtenerFacturasPorTurno(Guid turno)
        {
            var response = await _facturaNegocio.ObtenerFacturasPorTurno(turno);
            return Ok(response);
        }

        [HttpPost("AgregarTurnoAFactura")]
        public async Task<ActionResult<Guid>> AgregarTurnoAFactura(RequestFacturaTurno request)
        {
            await _facturaNegocio.AgregarTurnoAFactura(request.idVentaLocal, request.fecha, request.isla, request.numero, request.estacion);

            return Ok();
        }
    }
}
