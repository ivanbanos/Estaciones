using EstacionesServicio.Modelo;
using System;
using System.Collections.Generic;

namespace EstacionesServicio.Negocio.Extention
{
    public interface IValidadorGuidAFacturaElectronica
    {
        bool FacturaSiendoProceada(Guid ordenGuid);
        void SacarFactura(Guid ordenGuid);
        bool FacturasSiendoProceada(IEnumerable<Guid> facturasGuids);
        void SacarFacturas(IEnumerable<Guid> facturasGuids);
        Guid? ObtenerColaImpresionCanastilla(Guid idEstacion);
        void AgregarAColaImpresionCanastilla(Guid guid, Guid idEstacion);
    }
}