using AutoMapper;
using EstacionesServicio.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Turno
{
    public interface ITurnoNegocio {

        Task Add(Modelo.Turno turno);
        Task<IEnumerable<Modelo.Turno>> Get(DateTime fechaInicial, DateTime fechaFinal, string surtidor);
    }
    public class TurnoNegocio : ITurnoNegocio
    {
        private readonly ITurnoRepositorio _turnoRepositorio;
        private readonly IMapper _mapper;

        public TurnoNegocio(ITurnoRepositorio turnoRepositorio, IMapper mapper)
        {
            _turnoRepositorio = turnoRepositorio;
            _mapper = mapper;
        }

        public async Task Add(Modelo.Turno turno)
        {
            _turnoRepositorio.Add(_mapper.Map<Repositorio.Entities.Turno>(turno));
        }

        public async Task<IEnumerable<Modelo.Turno>> Get(DateTime fechaInicial, DateTime fechaFinal, string surtidor)
        {
            return _mapper.Map<IEnumerable<Modelo.Turno>>(_turnoRepositorio.Get(fechaInicial, fechaFinal, surtidor));
        }
    }
}
