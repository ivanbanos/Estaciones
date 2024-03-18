using AutoMapper;
using EstacionesServicio.Repositorio.Entities;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Turno
{
    public interface ITurnoNegocio {

        Task Add(Modelo.Turno turno);
        Task<IEnumerable<Modelo.TurnoReporte>> Get(DateTime fechaInicial, DateTime fechaFinal, string surtidor);
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

        public async Task<IEnumerable<Modelo.TurnoReporte>> Get(DateTime fechaInicial, DateTime fechaFinal, string surtidor)
        {
            var turnos = _mapper.Map<IEnumerable<Modelo.Turno>>(await _turnoRepositorio.Get(fechaInicial, fechaFinal, surtidor));
            var turnosreporte = new List<Modelo.TurnoReporte>();
            var diferenciaGeneral = 0d;
            var totalGeneral = 0d;
            foreach (var turno in turnos)
            {
                var turnoDesc = $"{turno.FechaApertura.ToString("dd-MM-yyyy")}-{turno.Isla}-{turno.Numero}";
                var diferenciaTotal = 0d;
                var totalTotal = 0d;
                foreach(var turnosur in turno.turnoSurtidores)
                {
                    var totalReporte = new Modelo.TurnoReporte {
                        Apertura = turnosur.Apertura,
                        Cierre = turnosur.Cierre.Value,
                        Combustible = turnosur.Combustible,
                        Diferencia = turnosur.Cierre.Value-turnosur.Apertura,
                        Manguera = turnosur.Manguera,
                        Precio = turnosur.precioCombustible,
                        Surtidor = turnosur.Surtidor,
                        Total = (turnosur.Cierre.Value - turnosur.Apertura) * turnosur.precioCombustible,
                        turno = turnoDesc
                    };
                    diferenciaTotal += totalReporte.Diferencia;
                    totalTotal += totalReporte.Total;
                    turnosreporte.Add(totalReporte);
                }
                diferenciaGeneral += diferenciaTotal;
                totalGeneral += totalTotal;

                turnosreporte.Add(new TurnoReporte()
                {
                    Diferencia = diferenciaTotal,
                    Total = totalTotal,
                    turno = turnoDesc
                 });
            }

            turnosreporte.Add(new TurnoReporte()
            {
                Diferencia = diferenciaGeneral,
                Total = totalGeneral,
                turno = "Total"
            });
            return turnosreporte;
        }
    }
}
