using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Recursos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class FacturaCanastillaRepository : IFacturaCanastillaRepository
    {
        private readonly ISQLHelper _sqlHelper;

        public FacturaCanastillaRepository(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }
        public async Task<int> Add(FacturaCanastilla factura, IEnumerable<CanastillaFactura> detalleFactura, Guid estacion)
        {
            var detalle = new DataTable();
            detalle.Columns.Add(new DataColumn("Guid", typeof(string)));
            detalle.Columns.Add(new DataColumn("cantidad", typeof(float)));
            detalle.Columns.Add(new DataColumn("precio", typeof(float)));
            detalle.Columns.Add(new DataColumn("subtotal", typeof(float)));
            detalle.Columns.Add(new DataColumn("iva", typeof(float)));
            detalle.Columns.Add(new DataColumn("total", typeof(float)));
            foreach (var t in detalleFactura)
            {
                var row = detalle.NewRow();
                row["Guid"] = t.Canastilla.guid;
                row["cantidad"] = t.cantidad;
                row["precio"] = t.precio;
                row["subtotal"] = t.subtotal;
                row["iva"] = t.iva;
                row["total"] = t.total;
                detalle.Rows.Add(row);
            }
            return await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.CrearFacturaCanastilla,
                new {
                    factura.fecha,
                    resolucion = factura.resolucion.Autorizacion.Trim(),
                    factura.consecutivo,
                    tercero = factura.terceroId.Identificacion,
                    forma = factura.codigoFormaPago.Descripcion.Trim(),
                    factura.subtotal,
                    factura.descuento,
                    factura.iva,
                    factura.total,
                    estacion,
                    detalle
                }).ConfigureAwait(false);
        }

        public Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(Guid idFactura)
        {
            var paramList = new DynamicParameters();
            paramList.Add("guid", idFactura);

            return _sqlHelper.GetsAsync<FacturaCanastillaDetalleResponse>(StoredProcedures.getFacturaCanatillaDetalle, paramList);
        }

        public Task<IEnumerable<FacturasCanastillaResponse>> GetFactura(Guid idFactura)
        {
            var paramList = new DynamicParameters();
            paramList.Add("guid", idFactura);

            return _sqlHelper.GetsAsync<FacturasCanastillaResponse>(StoredProcedures.getFacturaCanastilla, paramList);
        }

        public Task<IEnumerable<FacturasCanastillaResponse>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion)
        {
            var paramList = new DynamicParameters();
            if (fechaInicial != null) { paramList.Add("FechaInicial", fechaInicial); }
            if (fechaFinal != null) { paramList.Add("FechaFinal", fechaFinal); }
            if (!String.IsNullOrEmpty(identificacionTercero)) { paramList.Add("IdentificacionTercero", identificacionTercero); }
            if (!String.IsNullOrEmpty(nombreTercero)) { paramList.Add("NombreTercero", nombreTercero); }
            if (Guid.Empty != estacion) { paramList.Add("Estacion", estacion); }

            return _sqlHelper.GetsAsync<FacturasCanastillaResponse>(StoredProcedures.getFacturasCanastilla, paramList);
        }
    }
}
