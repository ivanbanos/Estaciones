using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IFacturaCanastillaRepository
    {
        Task<int> Add(FacturaCanastilla factura, IEnumerable<CanastillaFactura> detalleFactura, Guid estacion);
        Task<IEnumerable<FacturasCanastillaResponse>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion);

        Task<IEnumerable<FacturasCanastillaResponse>> GetFactura(string idFactura);

        Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(string idFactura);

    }
}
