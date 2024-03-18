using FacturacionelectronicaCore.Negocio.Tercero;
using FacturacionelectronicaCore.Negocio.Turno;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using FacturacionelectronicaCore.Negocio.Modelo;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
#if !DEBUG
    [Authorize]
#endif
    public class TurnosController : ControllerBase
    {
        private readonly ITurnoNegocio _turnoNegocio;

        public TurnosController(ITurnoNegocio turnoNegocio)
        {
            _turnoNegocio = turnoNegocio;
        }

        [HttpGet("{fechaInicial}/{fechaFinal}/{estacion}")]
        public async Task<ActionResult<IEnumerable<TurnoReporte>>> Get(DateTime fechaInicial, DateTime fechaFinal, string estacion)
        {

            var result = await _turnoNegocio.Get(fechaInicial, fechaFinal, estacion);

            if (result == null) { return NotFound(); }

            return Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult<int>> AddOrUpdate(Turno turno)
        {
            await _turnoNegocio.Add(turno);
            return Ok();
        }
    }
}
