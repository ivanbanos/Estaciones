using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesServicio.Repositorio.Entities;
using EstacionesSevicio.Respositorio.Extention;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Recursos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class FacturasRepository : IFacturasRepository
    {
        private readonly ISQLHelper _sqlHelper;
        private readonly string _facturaOrdenesDeDespachoProcedimiento;
        private readonly string _setConsecutivoFacturaPendiente;
        private readonly string _getFacturaByEstado;
        private readonly string _anularFacturas;

        public FacturasRepository(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
            _facturaOrdenesDeDespachoProcedimiento = StoredProcedures.FacturaOrdenesDeDespacho;
            _setConsecutivoFacturaPendiente = StoredProcedures.SetConsecutivoFacturaPendiente;
            _getFacturaByEstado = StoredProcedures.GetFacturaByEstado;
            _anularFacturas = StoredProcedures.AnularFacturas;
        }

        public async Task AddRange(IEnumerable<Factura> facturas, Guid estacion)
        {
            var dataTable = facturas.ToDataTable();
            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AgregarFactura,
                new { facturas = dataTable.AsTableValuedParameter(UserDefinedTypes.Factura), estacion }).ConfigureAwait(false);
        }

        public async Task AddFacturasImprimir(IEnumerable<FacturasEntity> facturas)
        {
            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AddFacturasImprimir,
                new { Facturas = facturas.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }

        public Task<IEnumerable<Factura>> GetFacturasImprimir(Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("estacion", estacion);

            return _sqlHelper.GetsAsync<Factura>(StoredProcedures.GetFacturasImprimir, paramList);
        }

        public Task<IEnumerable<Factura>> ObtenerFacturaPorGuid(Guid facturaGuid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("guid", facturaGuid);

            return _sqlHelper.GetsAsync<Factura>(StoredProcedures.ObtenerFacturaPorGuid, paramList);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Factura>> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho)
        {
            var paramList = new DynamicParameters();
            paramList.Add("OrdenesDeDespacho", ordenesDeDespacho.ToDataTable<OrdenesDeDespachoGuids>().AsTableValuedParameter(UserDefinedTypes.EntityTable));
            return _sqlHelper.GetsAsync<Factura>(_facturaOrdenesDeDespachoProcedimiento, paramList);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Factura>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion)
        {
            var paramList = new DynamicParameters();
            if (fechaInicial != null) { paramList.Add("FechaInicial", fechaInicial); }
            if (fechaFinal != null) { paramList.Add("FechaFinal", fechaFinal); }
            if (!String.IsNullOrEmpty(identificacionTercero)) { paramList.Add("IdentificacionTercero", identificacionTercero); }
            if (!String.IsNullOrEmpty(nombreTercero)) { paramList.Add("NombreTercero", nombreTercero); }
            if (Guid.Empty != estacion) { paramList.Add("Estacion", estacion); }

            return _sqlHelper.GetsAsync<Factura>(StoredProcedures.ListarFactura, paramList);
        }

        public async Task setConsecutivoFacturaPendiente(string facturaGuid, int consecutivo)
        {
            var paramList = new DynamicParameters();
            paramList.Add("facturaGuid", facturaGuid);
            paramList.Add("consecutivoActual", consecutivo);

            await _sqlHelper.GetsAsync<Factura>(_setConsecutivoFacturaPendiente, paramList);
        }
        public Task<IEnumerable<Factura>> GetFacturaByEstado(string estado, Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("estado", estado);
            paramList.Add("estacion", estacion);
            return _sqlHelper.GetsAsync<Factura>(_getFacturaByEstado, paramList);
        }

        /// <inheritdoc />
        public Task<int> AnularFacturas(IEnumerable<FacturasEntity> facturas)
        {
            return _sqlHelper.InsertOrUpdateOrDeleteAsync(_anularFacturas,
                new { Facturas = facturas.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }

        public async Task AgregarFechaReporteFactura(IEnumerable<FacturaFechaReporte> facturas, Guid estacion)
        {
            var dataTable = facturas.ToDataTable();
            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AgregarFechaReporteFactura,
                new { facturas = dataTable.AsTableValuedParameter(UserDefinedTypes.FacturaFechaReporte), estacion }).ConfigureAwait(false);

        }

        public async Task SetIdFacturaElectronicaFactura(string idFacturaElectronica, Guid guid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("idFacturaElectronica", idFacturaElectronica);
            paramList.Add("guid", guid);
            await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturaElectronicaFactura, paramList);
        }
        public async Task SetIdFacturaElectronicaOrdenesdeDespacho(string idFacturaElectronica, Guid guid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("idFacturaElectronica", idFacturaElectronica);
            paramList.Add("guid", guid);
            await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturaElectronicaOrdenesdeDespacho, paramList);
        }

        public Task<IEnumerable<Factura>> ObtenerFacturaPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("idVentaLocal", idVentaLocal);
            paramList.Add("estacion", estacion);

            return _sqlHelper.GetsAsync<Factura>(StoredProcedures.ObtenerFacturaPorIdVentaLocal, paramList);
        }
    }
}
