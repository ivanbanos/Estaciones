using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Factura
{
    public interface IFacturaNegocio
    {
        Task<Modelo.Factura> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho);

        /// <summary>
        /// Obtiene una lista de ordenes de despacho por Fecha inicial o Fecha final o Identiciación o Nombre de tercero
        /// </summary>
        /// <param name="fechaInicial">usuario</param>
        /// <param name="fechaFinal">fechaFinal</param>
        /// <param name="identificacionTercero">usuario</param>
        /// <param name="nombreTercero">fechaFinal</param>
        /// <returns>Coleccion de Ordenes de desapacho con Identificacion y Nombre de tercero</returns>
        Task<IEnumerable<Modelo.Factura>> GetFacturas(FiltroBusqueda filtroOrdenDeDespacho);
        /// <summary>
        /// Anula las facturas de la lista suministrada
        /// </summary>
        /// <param name="facturas">Lista de facturas a anular</param>
        /// <returns></returns>
        Task<int> AnularFacturas(IEnumerable<FacturasEntity> facturas);
        Task AddFacturasImprimir(IEnumerable<FacturasEntity> facturas);

        /// <summary>
        /// Calcula los valores del reporte fiscal
        /// </summary>
        /// <param name="filtroFactura">Filtro para seleccionar las facturas del reporte fiscal</param>
        /// <returns>Reporte Fiscal</returns>
        Task<ReporteFiscal> GetReporteFiscal(FiltroBusqueda filtroFactura);
        Task<string> EnviarAFacturacion(string ordenGuid);
        Task<string> CrearFacturaFacturas(IEnumerable<FacturasEntity> facturasGuids);
        Task<Modelo.Factura> ObtenerFacturaPorIdVentaLocal(int idVentaLocal, Guid estacion);
        Task AgregarTurnoAFactura(int idVentaLocal, DateTime fecha, string Isla, int numero, Guid estacion);
        Task<IEnumerable<Modelo.Factura>> ObtenerFacturasPorTurno(Guid turno);
        Task<string> EnviarAFacturacion(int idVentaLocal, Guid estacion);
    }
}