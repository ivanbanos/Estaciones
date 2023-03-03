using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.FacturaCanastillaNegocio
{
    public class FacturaCanastillaNegocio : IFacturaCanastillaNegocio
    {
        private readonly IFacturaCanastillaRepository _facturaCanastillaRepository;
        private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;

        public FacturaCanastillaNegocio(IFacturaCanastillaRepository facturaCanastillaRepository, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica)
        {
            _facturaCanastillaRepository = facturaCanastillaRepository;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
        }
        public async Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(Guid idFactura)
        {
            return await _facturaCanastillaRepository.GetDetalleFactura(idFactura);
        }

        public async Task<FacturasCanastillaResponse> GetFactura(Guid idFactura)
        {
            return (await _facturaCanastillaRepository.GetFactura(idFactura)).FirstOrDefault();
        }

        public async Task<IEnumerable<FacturasCanastillaResponse>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion)
        {
            return await _facturaCanastillaRepository.GetFacturas(fechaInicial, fechaFinal, identificacionTercero, nombreTercero, estacion);
        }


        public void ColocarEspera(Guid guid, Guid idEstacion)
        {
            _validadorGuidAFacturaElectronica.AgregarAColaImpresionCanastilla(guid, idEstacion);
        }
        public async Task<int> ObtenerParaImprimir(Guid idEstacion)
        {
            var guid = _validadorGuidAFacturaElectronica.ObtenerColaImpresionCanastilla(idEstacion);
            if (guid.HasValue)
            {
                var factura = await GetFactura(guid.Value);
                factura.facturaCanastillaDetalles = await GetDetalleFactura(guid.Value);

                return factura.consecutivo;
            }
            return 0;
        }
    }
}
