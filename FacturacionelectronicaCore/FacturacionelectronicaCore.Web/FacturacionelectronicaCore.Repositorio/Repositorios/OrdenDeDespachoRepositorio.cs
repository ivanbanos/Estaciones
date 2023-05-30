using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesSevicio.Respositorio.Extention;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Mongodb;
using FacturacionelectronicaCore.Repositorio.Recursos;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class OrdenDeDespachoRepositorio : IOrdenDeDespachoRepositorio
    {
        private readonly ISQLHelper _sqlHelper;
        private readonly RepositorioConfig _repositorioConfig;
        private readonly IMongoHelper _mongoHelper;

        public OrdenDeDespachoRepositorio(ISQLHelper sqlHelper, IOptions<RepositorioConfig> repositorioConfig, IMongoHelper mongoHelper)
        {
            _sqlHelper = sqlHelper;
            _repositorioConfig = repositorioConfig.Value;
            _mongoHelper = mongoHelper;
        }

        private async Task AgregarAMongo(Guid estacion, OrdenDeDespacho factura)
        {
            var filter = Builders<FacturaMongo>.Filter.Eq("IdLocal", factura.IdLocal);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaMongo>(_repositorioConfig.Cliente, "ordenes", filter);
            if (!facturasMongo.Any(x => x.EstacionGuid == estacion.ToString()))
            {
                var facturaMongo = new OrdenesMongo(factura);
                facturaMongo.guid = Guid.NewGuid().ToString();
                facturaMongo.EstacionGuid = estacion.ToString();
                facturaMongo.Estado = "Creada";
                await _mongoHelper.CreateDocument(_repositorioConfig.Cliente, "ordenes", facturaMongo);
            }
            else
            {
                var facturaMongo = facturasMongo.First(x => x.EstacionGuid == estacion.ToString());
                var filterGuid = Builders<OrdenesMongo>.Filter.Eq("Guid", facturaMongo.Guid);
                var update = Builders<OrdenesMongo>.Update
                    .Set(x => x.IdentificacionTercero, factura.IdentificacionTercero)
                    .Set(x => x.NombreTercero, factura.NombreTercero)
                    .Set(x => x.Placa, factura.Placa)
                    .Set(x => x.Kilometraje, factura.Kilometraje);
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "ordenes", filterGuid, update);

            }
        }

        public async Task AddRange(IEnumerable<OrdenDeDespacho> lists, Guid estacion)
        {
            var tasks = new List<Task>();
            foreach (var factura in lists)
            {
                tasks.Add(AgregarAMongo(estacion, factura));
            }

            await Task.WhenAll(tasks);
            //var dataTable = lists.ToDataTable();
            //await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AgregarOrdenDespacho,
            //    new { ordenes = dataTable.AsTableValuedParameter(UserDefinedTypes.OrdenesDeDespachoType), estacion }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespacho(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, 
                                                                       string nombreTercero, Guid estacion)
        {
            List<FilterDefinition<OrdenesMongo>> filters = new List<FilterDefinition<OrdenesMongo>>();

            var paramList = new DynamicParameters();
            if (fechaInicial != null)
            {
                paramList.Add("FechaInicial", fechaInicial);

                filters.Add(Builders<OrdenesMongo>.Filter.Gte("Fecha", fechaInicial));
            }
            if (fechaFinal != null)
            {
                paramList.Add("FechaFinal", fechaFinal);
                filters.Add(Builders<OrdenesMongo>.Filter.Lte("Fecha", fechaFinal));
            }
            if (!string.IsNullOrEmpty(identificacionTercero))
            {
                paramList.Add("IdentificacionTercero", identificacionTercero);
                filters.Add(Builders<OrdenesMongo>.Filter.Eq("Identificacion", identificacionTercero));
            }
            if (!string.IsNullOrEmpty(nombreTercero))
            {
                paramList.Add("NombreTercero", nombreTercero);
                filters.Add(Builders<OrdenesMongo>.Filter.Eq("NombreTercero", nombreTercero));
            }
            if (Guid.Empty != estacion)
            {
                paramList.Add("Estacion", estacion);
                filters.Add(Builders<OrdenesMongo>.Filter.Eq("EstacionGuid", estacion.ToString()));
            }


            var facturasMongo = await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, "ordenes", filters);
            if (facturasMongo.Any())
            {
                return facturasMongo;
            }
            else
            {
                var facturas = await _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.GetOrdenesDeDespacho, paramList);
                var tasks = new List<Task>();
                foreach (var factura in facturas)
                {
                    tasks.Add(AgregarAMongo(estacion, factura));
                }
                await Task.WhenAll(tasks);
                return facturas;
            }
        }

        public Task<int> AddOrdenesImprimir(IEnumerable<FacturasEntity> lists)
        {
            return _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AddOrdenesImprimir,
                new { ordenes = lists.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrdenDeDespacho>> GetOrdenesImprimir(Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("estacion", estacion);

            var list = await _sqlHelper.GetsAsync<FacturasEntity>(StoredProcedures.GetOrdenesImprimir, paramList);
            var listFacturas = new List<OrdenDeDespacho>();
            var tasks = new List<Task>();
            foreach (var facturaEntity in list)
            {
                var filter = Builders<OrdenesMongo>.Filter.Eq("Guid", facturaEntity.Guid.ToString());
                var facturasMongo = await _mongoHelper.GetFilteredDocuments<OrdenesMongo>(_repositorioConfig.Cliente, "ordenes", filter);
                if (!facturasMongo.Any())
                {
                    var paramListfac = new DynamicParameters();
                    paramListfac.Add("guid", facturaEntity.Guid);
                    var factura = (await _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.ObtenerOrdenDespachoPorGuid, paramListfac)).First();

                    tasks.Add(AgregarAMongo(estacion, factura));
                    listFacturas.Add(factura);
                }
                else
                {

                    listFacturas.AddRange(facturasMongo);
                }
            }

            await Task.WhenAll(tasks);
            return listFacturas;

        }
        /// <inheritdoc />
        public async Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenDespachoPorGuid(Guid guid)
        {
            var filter = Builders<OrdenesMongo>.Filter.Eq("Guid", guid.ToString());
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<OrdenesMongo>(_repositorioConfig.Cliente, "ordenes", filter);
            if (!facturasMongo.Any())
            {
                var paramList = new DynamicParameters();
                paramList.Add("guid", guid);

                var facturas = await _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.ObtenerOrdenDespachoPorGuid, paramList);
                return facturas;
            }
            return facturasMongo;
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
            var filter = Builders<OrdenesMongo>.Filter.Eq("guid", guid.ToString());
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<OrdenesMongo>(_repositorioConfig.Cliente, "ordenes", filter);
            if (facturasMongo.Any())
            {
                var facturaMongo = facturasMongo.First();
                var filterGuid = Builders<OrdenesMongo>.Filter.Eq("Guid", facturaMongo.guid.ToString());
                var update = Builders<OrdenesMongo>.Update
                    .Set(x => x.idFacturaElectronica, idFacturaElectronica);
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "ordenes", filterGuid, update);

            }
            var paramList = new DynamicParameters();
            paramList.Add("idFacturaElectronica", idFacturaElectronica);
            paramList.Add("guid", guid);
            await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturaElectronicaOrdenesdeDespacho, paramList);
        }

        public async Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {
            List<FilterDefinition<OrdenesMongo>> filters = new List<FilterDefinition<OrdenesMongo>>();

            var paramList = new DynamicParameters();

            paramList.Add("Estacion", estacion);
            paramList.Add("idVentaLocal", idVentaLocal);
            filters.Add(Builders<OrdenesMongo>.Filter.Eq("idVentaLocal", idVentaLocal));

            var facturasMongo = await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, "ordenes", filters);
            if (facturasMongo.Any())
            {
                return facturasMongo.Where(x => x.EstacionGuid == estacion.ToString()); 
            }
            else
            {
                var facturas = await _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.GetOrdenesDeDespacho, paramList);
                var tasks = new List<Task>();
                foreach (var factura in facturas)
                {
                    tasks.Add(AgregarAMongo(estacion, factura));
                }
                await Task.WhenAll(tasks);
                return facturas;
            }
        }
    }
}
