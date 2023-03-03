using System;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Resolucion
{
    public interface IResolucionNegocio
    {
        Task<Modelo.Resolucion> HabilitarResolucion(Guid estacion, DateTime fechaVencimiento);
        Task<Modelo.CreacionResolucion> AddNuevaResolucion(Modelo.CreacionResolucion resolucion);
        Task<Modelo.Resolucion> GetResolucionActiva(Guid estacion);
    }
}
