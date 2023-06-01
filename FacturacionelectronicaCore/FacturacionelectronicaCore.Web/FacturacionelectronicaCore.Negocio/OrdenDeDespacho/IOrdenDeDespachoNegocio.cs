using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.OrdenDeDespacho
{
    public interface IOrdenDeDespachoNegocio
    {
        /// <summary>
        /// Obtiene una lista de ordenes de despacho por Fecha inicial o Fecha final o Identiciación o Nombre de tercero
        /// </summary>
        /// <param name="fechaInicial">usuario</param>
        /// <param name="fechaFinal">fechaFinal</param>
        /// <param name="identificacionTercero">usuario</param>
        /// <param name="nombreTercero">fechaFinal</param>
        /// <returns>Coleccion de Ordenes de desapacho con Identificacion y Nombre de tercero</returns>
        Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(FiltroBusqueda filtroOrdenDeDespacho);

        Task<int> AddOrdenesImprimir(IEnumerable<FacturasEntity> ordenDeDespachos);
        Task AnularOrdenes(IEnumerable<FacturasEntity> ordenes);
        Task<string> EnviarAFacturacion(string ordenGuid);
        Task<string> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho);
        Task<Modelo.OrdenDeDespacho> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion);
    }
}