using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesSevicio.Respositorio.Extention;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Recursos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class OrdenDeDespachoRepositorio : IOrdenDeDespachoRepositorio
    {
        private readonly ISQLHelper _sqlHelper;

        public OrdenDeDespachoRepositorio(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task AddRange(IEnumerable<OrdenDeDespacho> lists, Guid estacion)
        {
            var dataTable = lists.ToDataTable();
            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AgregarOrdenDespacho,
                new { ordenes = dataTable.AsTableValuedParameter(UserDefinedTypes.OrdenesDeDespachoType), estacion }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespacho(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, 
                                                                       string nombreTercero, Guid estacion)
        {
            var paramList = new DynamicParameters();
            if (fechaInicial != null) { paramList.Add("FechaInicial", fechaInicial); }
            if (fechaFinal != null) { paramList.Add("FechaFinal", fechaFinal); }
            if (!String.IsNullOrEmpty(identificacionTercero)) { paramList.Add("IdentificacionTercero", identificacionTercero); }
            if (!String.IsNullOrEmpty(nombreTercero)) { paramList.Add("NombreTercero", nombreTercero); }
            if (Guid.Empty != estacion) { paramList.Add("Estacion", estacion); }

            return _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.GetOrdenesDeDespacho, paramList);
        }

        public Task<int> AddOrdenesImprimir(IEnumerable<FacturasEntity> lists)
        {
            return _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AddOrdenesImprimir,
                new { ordenes = lists.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }

        /// <inheritdoc />
        public Task<IEnumerable<OrdenDeDespacho>> GetOrdenesImprimir(Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("estacion", estacion);
            return _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.GetOrdenesImprimir, paramList);
        }
        /// <inheritdoc />
        public Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenDespachoPorGuid(Guid guid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("guid", guid);
            return _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.ObtenerOrdenDespachoPorGuid, paramList);
        }

        public Task<int> AnularOrdenes(IEnumerable<FacturasEntity> ordenes)
        {
            return _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AnularOrdenesDeDespacho,
                new { Ordenes = ordenes.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }


        /// <inheritdoc />
        public Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespachoByFactura(Guid facturaGuid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("facturaGuid", facturaGuid);
            return _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.GetOrdenesDeDespachoByFactura, paramList);
        }

        public async Task SetIdFacturaElectronicaOrdenesdeDespacho(string idFacturaElectronica, Guid guid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("idFacturaElectronica", idFacturaElectronica);
            paramList.Add("guid", guid);
            await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturaElectronicaOrdenesdeDespacho, paramList);
        }

        public Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("idVentaLocal", idVentaLocal);
            paramList.Add("estacion", estacion);
            return _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.ObtenerOrdenDespachoPorIdVentaLocal, paramList);
        }
    }
}
