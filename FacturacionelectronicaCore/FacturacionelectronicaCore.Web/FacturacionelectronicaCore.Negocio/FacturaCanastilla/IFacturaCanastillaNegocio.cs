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

        Task<FacturasCanastillaResponse> GetFactura(Guid idFactura);

        Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(Guid idFactura);
        void ColocarEspera(Guid guid, Guid idEstacion);
        Task<int> ObtenerParaImprimir(Guid idEstacion);
    }
}
