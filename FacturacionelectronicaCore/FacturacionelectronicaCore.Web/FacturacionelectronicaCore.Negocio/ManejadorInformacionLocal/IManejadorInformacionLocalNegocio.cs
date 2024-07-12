using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal
{
    public interface IManejadorInformacionLocalNegocio
    {
        Task<IEnumerable<string>> GetGuidsFacturasPendientes(Guid estacion);
        Task EnviarResolucion(RequestEnvioResolucion requestEnvioResolucion);
        Task EnviarTerceros(IEnumerable<Modelo.Tercero> terceros);
        Task EnviarFacturas(IEnumerable<Modelo.Factura> facturas, Guid estacion);
        Task EnviarOrdenesDespacho(IEnumerable<Modelo.OrdenDeDespacho> ordenDeDespachos, Guid estacion);
        Task<IEnumerable<Modelo.Factura>> GetFacturasImprimir(Guid estacion);

        Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(Guid estacion);

        Task<IEnumerable<Modelo.Tercero>> GetTercerosActualizados(Guid estacion);
        Task<IEnumerable<Modelo.Tercero>> GetTercerosActualizados();
        Task AgregarFechaReporteFactura(IEnumerable<Modelo.FacturaFechaReporte> facturaFechaReporte, Guid estacion);
        Task<IEnumerable<string>> GetTipos();
        Task<string> GetInfoFacturaElectronica(int idVentaLocal, Guid estacion);


        Task<int> AddFacturaCanastilla(IEnumerable<FacturaCanastilla> facturas, Guid estacion);


        Task<ResolucionElectronica> GetResolucionElectronica();
        Task<string> JsonFacturaElectronica(int idVentaLocal, Guid estacion);
    }
}
