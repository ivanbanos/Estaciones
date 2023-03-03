using System;
using System.Threading.Tasks;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Negocio.Resolucion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
#if !DEBUG
    [Authorize]
#endif
    public class ResolucionController : ControllerBase
    {
        private readonly IResolucionNegocio _resolucionNegocio;

        public ResolucionController(IResolucionNegocio resolucionNegocio)
        {
            _resolucionNegocio = resolucionNegocio;
        }


        [HttpGet("{estacion}")]
        public async Task<ActionResult<Resolucion>> GetResolucionActiva(Guid estacion)
        {
            var result = await _resolucionNegocio.GetResolucionActiva(estacion);

            if (result == null) { return NotFound(); }

            return Ok(result);
        }


        [HttpPost("AddNuevaResolucion")]
        public async Task<ActionResult<int>> AddNuevaResolucion(CreacionResolucion resolucion)
        {

            return Ok(await _resolucionNegocio.AddNuevaResolucion(resolucion));
        }

        [HttpPost("HabilitarResolucion/{estacion}")]
        public async Task<ActionResult<int>> HabilitarResolucion(Guid estacion,[FromBody] DateTime fechaVencimiento)
        {

            return Ok(await _resolucionNegocio.HabilitarResolucion(estacion, fechaVencimiento));
        }
    }
}
