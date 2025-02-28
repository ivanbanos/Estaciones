using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Mongodb;
using FacturacionelectronicaCore.Repositorio.Recursos;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class FacturaCanastillaRepository : IFacturaCanastillaRepository
    {
        private readonly ISQLHelper _sqlHelper;
        private readonly IMongoHelper _mongoHelper;
        private readonly RepositorioConfig _repositorioConfig;

        public FacturaCanastillaRepository(IOptions<RepositorioConfig> repositorioConfig, ISQLHelper sqlHelper, IMongoHelper mongoHelper)
        {
            _sqlHelper = sqlHelper;
            _mongoHelper = mongoHelper;
            _repositorioConfig = repositorioConfig.Value;
        }
        public async Task<int> Add(FacturaCanastilla factura, IEnumerable<CanastillaFactura> detalleFactura, Guid estacion)
        {
            var filter = Builders<FacturaCanastilla>.Filter.Eq("FacturasCanastillaId", factura.FacturasCanastillaId);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            if (!facturasMongo.Any(x => x.IdEstacion == estacion))
            {
                factura.Guid = Guid.NewGuid().ToString();
                await _mongoHelper.CreateDocument(_repositorioConfig.Cliente, "facturasCanastillas", factura);
            }
            return 0;
        }

        public async Task<bool> FacturaGenerada(int facturasCanastillaId, Guid estacion)
        {
            var filter = Builders<FacturaCanastilla>.Filter.Eq("FacturasCanastillaId", facturasCanastillaId);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            return facturasMongo.Any(x => x.IdEstacion == estacion);
        }

        public Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(string idFactura)
        {
            var paramList = new DynamicParameters();
            paramList.Add("guid", idFactura);

            return _sqlHelper.GetsAsync<FacturaCanastillaDetalleResponse>(StoredProcedures.getFacturaCanatillaDetalle, paramList);
        }

        public async Task<IEnumerable<FacturaCanastilla>> GetFactura(string idFactura)
        {
            var filter = Builders<FacturaCanastilla>.Filter.Eq("Guid", idFactura);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            if (facturasMongo.Any())
            {
                return facturasMongo;
            }
            return null;
        }

        public async Task<IEnumerable<FacturaCanastilla>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion)
        {
            List<FilterDefinition<FacturaCanastilla>> filters = new List<FilterDefinition<FacturaCanastilla>>();

            var paramList = new DynamicParameters();
            if (fechaInicial != null)
            {
                paramList.Add("FechaInicial", fechaInicial);

                filters.Add(Builders<FacturaCanastilla>.Filter.Gte("Fecha", fechaInicial.Value.AddHours(-12)));
            }
            if (fechaFinal != null)
            {
                paramList.Add("FechaFinal", fechaFinal);
                filters.Add(Builders<FacturaCanastilla>.Filter.Lte("Fecha", fechaFinal.Value.AddDays(1).AddHours(-12)));
            }
            if (!string.IsNullOrEmpty(identificacionTercero))
            {
                paramList.Add("IdentificacionTercero", identificacionTercero);
                filters.Add(Builders<FacturaCanastilla>.Filter.Eq("Identificacion", identificacionTercero));
            }
            if (!string.IsNullOrEmpty(nombreTercero))
            {
                paramList.Add("NombreTercero", nombreTercero);
                filters.Add(Builders<FacturaCanastilla>.Filter.Eq("NombreTercero", nombreTercero));
            }


            var facturasMongo = await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, "facturasCanastillas", filters);
            //if (facturasMongo.Any(x=>x.EstacionGuid.ToLower() == estacion.ToString().ToLower()))
            //{
            return facturasMongo.Where(x => x.IdEstacion == estacion);
        }
        public async Task SetIdFacturaElectronicaFactura(string idFacturaElectronica, string guid)
        {
            var filter = Builders<FacturaCanastilla>.Filter.Eq("_id", guid.ToString());
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            if (facturasMongo.Any())
            {
                var facturaMongo = facturasMongo.First();
                var filterGuid = Builders<FacturaCanastilla>.Filter.Eq("_id", facturaMongo.Guid);
                var update = Builders<FacturaCanastilla>.Update
                    .Set(x => x.idFacturaElectronica, idFacturaElectronica)
                    .Set(x => x.estado, "Anulada");
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "facturasCanastillas", filterGuid, update);

            }
            //var paramList = new DynamicParameters();
            //paramList.Add("idFacturaElectronica", idFacturaElectronica);
            //paramList.Add("guid", guid);
            //await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturaElectronicaFactura, paramList);
        }
    }
}
