﻿using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesServicio.Repositorio.Entities;
using EstacionesSevicio.Respositorio.Extention;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Mongodb;
using FacturacionelectronicaCore.Repositorio.Recursos;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class FacturasRepository : IFacturasRepository
    {
        private readonly ISQLHelper _sqlHelper;
        private readonly IMongoHelper _mongoHelper;
        private readonly string _facturaOrdenesDeDespachoProcedimiento;
        private readonly string _setConsecutivoFacturaPendiente;
        private readonly string _getFacturaByEstado;
        private readonly string _anularFacturas;
        private readonly RepositorioConfig _repositorioConfig;

        public FacturasRepository(IOptions<RepositorioConfig> repositorioConfig, ISQLHelper sqlHelper, IMongoHelper mongoHelper)
        {
            _repositorioConfig = repositorioConfig.Value;
            _sqlHelper = sqlHelper;
            _facturaOrdenesDeDespachoProcedimiento = StoredProcedures.FacturaOrdenesDeDespacho;
            _setConsecutivoFacturaPendiente = StoredProcedures.SetConsecutivoFacturaPendiente;
            _getFacturaByEstado = StoredProcedures.GetFacturaByEstado;
            _anularFacturas = StoredProcedures.AnularFacturas;
            _mongoHelper = mongoHelper ?? throw new ArgumentNullException(nameof(mongoHelper));
        }

        public async Task AddRange(IEnumerable<Factura> facturas, Guid estacion)
        {
            var tasks = new List<Task>();
            foreach (var factura in facturas)
            {
                tasks.Add(AgregarAMongo(estacion, factura));
            }

            await Task.WhenAll(tasks);
            //var dataTable = facturas.ToDataTable();
            //await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AgregarFactura,
            //    new { facturas = dataTable.AsTableValuedParameter(UserDefinedTypes.Factura), estacion }).ConfigureAwait(false);
        }

        private async Task AgregarAMongo(Guid estacion, Factura factura)
        {
            var filter = Builders<FacturaMongo>.Filter.Eq("IdLocal", factura.IdLocal);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaMongo>(_repositorioConfig.Cliente, "factuas", filter);
            if (!facturasMongo.Any(x => x.EstacionGuid == estacion))
            {
                var facturaMongo = new FacturaMongo(factura);
                facturaMongo.Guid = new Guid();
                facturaMongo.EstacionGuid = estacion;
                facturaMongo.Estado = "Creada";
                await _mongoHelper.CreateDocument(_repositorioConfig.Cliente, "factuas", facturaMongo);
            }
            else
            {
                var facturaMongo = facturasMongo.First(x => x.EstacionGuid == estacion);
                var filterGuid = Builders<FacturaMongo>.Filter.Eq("Guid", facturaMongo.Guid);
                var update = Builders<FacturaMongo>.Update
                    .Set(x => x.IdentificacionTercero, factura.IdentificacionTercero)
                    .Set(x => x.NombreTercero, factura.NombreTercero)
                    .Set(x => x.Placa, factura.Placa)
                    .Set(x => x.Kilometraje, factura.Kilometraje);
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "factuas", filterGuid, update);

            }
        }

        public async Task AddFacturasImprimir(IEnumerable<FacturasEntity> facturas)
        {
            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AddFacturasImprimir,
                new { Facturas = facturas.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }

        public async Task<IEnumerable<Factura>> GetFacturasImprimir(Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("estacion", estacion);

            var list = await _sqlHelper.GetsAsync<FacturasEntity>(StoredProcedures.GetFacturasImprimir, paramList);
            var listFacturas = new List<Factura>();
            var tasks = new List<Task>();
            foreach (var facturaEntity in list)
            {
                var filter = Builders<FacturaMongo>.Filter.Eq("Guid", facturaEntity.Guid);
                var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaMongo>(_repositorioConfig.Cliente, "factuas", filter);
                if (!facturasMongo.Any())
                {
                    var paramListfac = new DynamicParameters();
                    paramListfac.Add("guid", facturaEntity.Guid);
                    var factura = (await _sqlHelper.GetsAsync<Factura>(StoredProcedures.ObtenerFacturaPorGuid, paramListfac)).First();

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

        public async Task<IEnumerable<Factura>> ObtenerFacturaPorGuid(Guid facturaGuid)
        {

            var filter = Builders<FacturaMongo>.Filter.Eq("Guid", facturaGuid);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaMongo>(_repositorioConfig.Cliente, "factuas", filter);
            if (!facturasMongo.Any())
            {
                var paramList = new DynamicParameters();
                paramList.Add("guid", facturaGuid);

                var facturas = await _sqlHelper.GetsAsync<Factura>(StoredProcedures.ObtenerFacturaPorGuid, paramList);
                return facturas;
            }
            return facturasMongo;
        }

        /// <inheritdoc />
        public Task<IEnumerable<Factura>> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho)
        {
            var paramList = new DynamicParameters();
            paramList.Add("OrdenesDeDespacho", ordenesDeDespacho.ToDataTable<OrdenesDeDespachoGuids>().AsTableValuedParameter(UserDefinedTypes.EntityTable));
            return _sqlHelper.GetsAsync<Factura>(_facturaOrdenesDeDespachoProcedimiento, paramList);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Factura>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion)
        {

            List<FilterDefinition<FacturaMongo>> filters = new List<FilterDefinition<FacturaMongo>>();

            var paramList = new DynamicParameters();
            if (fechaInicial != null) { 
                paramList.Add("FechaInicial", fechaInicial);

                filters.Add(Builders<FacturaMongo>.Filter.Gte("Fecha", fechaInicial));
            }
            if (fechaFinal != null) { 
                paramList.Add("FechaFinal", fechaFinal);
                filters.Add(Builders<FacturaMongo>.Filter.Lte("Fecha", fechaFinal));
            }
            if (!string.IsNullOrEmpty(identificacionTercero)) { 
                paramList.Add("IdentificacionTercero", identificacionTercero);
                filters.Add(Builders<FacturaMongo>.Filter.Eq("Identificacion", identificacionTercero));
            }
            if (!string.IsNullOrEmpty(nombreTercero)) { 
                paramList.Add("NombreTercero", nombreTercero);
                filters.Add(Builders<FacturaMongo>.Filter.Lte("NombreTercero", nombreTercero));
            }
            if (Guid.Empty != estacion) { 
                paramList.Add("Estacion", estacion);
                filters.Add(Builders<FacturaMongo>.Filter.Lte("EstacionGuid", estacion));
            }


            var facturasMongo = await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, "factuas", filters);
            if (facturasMongo.Any())
            {
                var facturas = await _sqlHelper.GetsAsync<Factura>(StoredProcedures.ListarFactura, paramList);
                var tasks = new List<Task>();
                foreach (var factura in facturas)
                {
                    tasks.Add(AgregarAMongo(estacion, factura));
                }
                await Task.WhenAll(tasks);
                return facturas;
            }
            else
            {
                var facturas = await _sqlHelper.GetsAsync<Factura>(StoredProcedures.ListarFactura, paramList);
                var tasks = new List<Task>();
                foreach(var factura in facturas)
                {
                    tasks.Add(AgregarAMongo(estacion, factura));
                }
                await Task.WhenAll(tasks);
                return facturas;
            }
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
            foreach(var factura in facturas)
            {
                var filter = Builders<FacturaMongo>.Filter.Eq("IdVentaLocal", factura.IdVentaLocal);
                var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaMongo>(_repositorioConfig.Cliente, "factuas", filter);
                if (facturasMongo.Any(x => x.EstacionGuid == estacion))
                {
                    var facturaMongo = facturasMongo.First(x => x.EstacionGuid == estacion);
                    var filterGuid = Builders<FacturaMongo>.Filter.Eq("Guid", facturaMongo.Guid);
                    var update = Builders<FacturaMongo>.Update
                        .Set(x => x.FechaReporte, factura.FechaReporte);
                    await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "factuas", filterGuid, update);

                }
                var dataTable = facturas.ToDataTable();
                await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AgregarFechaReporteFactura,
                    new { facturas = dataTable.AsTableValuedParameter(UserDefinedTypes.FacturaFechaReporte), estacion }).ConfigureAwait(false);

            }



        }

        public async Task SetIdFacturaElectronicaFactura(string idFacturaElectronica, Guid guid)
        {
            var filter = Builders<FacturaMongo>.Filter.Eq("guid", guid);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaMongo>(_repositorioConfig.Cliente, "factuas", filter);
            if (facturasMongo.Any())
            {
                var facturaMongo = facturasMongo.First();
                var filterGuid = Builders<FacturaMongo>.Filter.Eq("Guid", facturaMongo.Guid);
                var update = Builders<FacturaMongo>.Update
                    .Set(x => x.idFacturaElectronica, idFacturaElectronica);
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "factuas", filterGuid, update);

            }
            var paramList = new DynamicParameters();
            paramList.Add("idFacturaElectronica", idFacturaElectronica);
            paramList.Add("guid", guid);
            await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturaElectronicaFactura, paramList);
        }
        public async Task SetIdFacturaElectronicaOrdenesdeDespacho(string idFacturaElectronica, Guid guid)
        {
            var filter = Builders<OrdenesMongo>.Filter.Eq("guid", guid);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<OrdenesMongo>(_repositorioConfig.Cliente, "ordenes", filter);
            if (facturasMongo.Any())
            {
                var facturaMongo = facturasMongo.First();
                var filterGuid = Builders<OrdenesMongo>.Filter.Eq("Guid", facturaMongo.guid);
                var update = Builders<OrdenesMongo>.Update
                    .Set(x => x.idFacturaElectronica, idFacturaElectronica);
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "ordenes", filterGuid, update);

            }
            var paramList = new DynamicParameters();
            paramList.Add("idFacturaElectronica", idFacturaElectronica);
            paramList.Add("guid", guid);
            await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturaElectronicaOrdenesdeDespacho, paramList);
        }

        public async Task<IEnumerable<Factura>> ObtenerFacturaPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {
            List<FilterDefinition<FacturaMongo>> filters = new List<FilterDefinition<FacturaMongo>>
            {
                Builders<FacturaMongo>.Filter.Lte("EstacionGuid", estacion),
                Builders<FacturaMongo>.Filter.Eq("idVentaLocal", idVentaLocal)
            };
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaMongo>(_repositorioConfig.Cliente, "factuas", filters);
            if (facturasMongo.Any())
            {
                return facturasMongo;

            }
            var paramList = new DynamicParameters();
            paramList.Add("idVentaLocal", idVentaLocal);
            paramList.Add("estacion", estacion);

            return await _sqlHelper.GetsAsync<Factura>(StoredProcedures.ObtenerFacturaPorIdVentaLocal, paramList);
        }
    }
}
