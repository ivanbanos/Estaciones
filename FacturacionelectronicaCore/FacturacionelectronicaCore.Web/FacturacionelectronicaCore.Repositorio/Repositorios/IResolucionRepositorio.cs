using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IResolucionRepositorio
    {
        Task UpdateConsecutivoResolucion(int consecutivo);
        Task<CreacionResolucion> AddNuevaResolucion(CreacionResolucion resolucionEntity);
        Task<Resolucion> GetResolucionActiva(Guid estacion);
        Task<Resolucion> HabilitarResolucion(Guid estacion, DateTime fechaVencimiento);
    }
}
