

using FacturacionelectronicaCore.Negocio.Modelo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.Alegra
{
    public interface IFacturacionElectronicaFacade
    {
        public Task<string> GenerarFacturaElectronica(Modelo.Factura factura);
        public Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho factura);
        public Task<int> GenerarTercero(Modelo.Tercero tercero);
        Task ActualizarTercero(Modelo.Tercero t, string idFacturacion);
        Task<Item> GetItem(string name);
        Task<IEnumerable<TerceroResponse>> GetTerceros(int start);
        Task<string> GenerarFacturaElectronica(List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items);
        Task<string> GenerarFacturaElectronica(List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items);
        Task<ResponseInvoice> GetFacturaElectronica(string id);
        Task<ResolucionElectronica> GetResolucionElectronica();
    }
}
