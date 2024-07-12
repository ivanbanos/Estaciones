using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.FacturaCanastillaNegocio
{
    public interface IFacturaCanastillaNegocio
    {
        Task<IEnumerable<FacturasCanastillaResponse>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion);

        Task<FacturasCanastillaResponse> GetFactura(string idFactura);

        Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(string idFactura);
        void ColocarEspera(string guid, Guid idEstacion);
        Task<int> ObtenerParaImprimir(Guid idEstacion);
        Task<FacturaCanastillaReporte> GetFacturasReporte(DateTime? fechaInicial, DateTime? fechaFinal, string identificacion, string nombreTercero, Guid estacion);
    }
}
