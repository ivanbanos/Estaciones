using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesSevicio.Respositorio.Extention;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class ResolucionRepositorio : IResolucionRepositorio
    {
        private readonly ISQLHelper _sqlHelper;

        public ResolucionRepositorio(ISQLHelper sqlHelper) 
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<CreacionResolucion> AddNuevaResolucion(CreacionResolucion resolucionEntity)
        {
            resolucionEntity.Fecha = DateTime.Now;
            var list = new List<CreacionResolucion> { resolucionEntity };

            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AddNuevaResolucion,
                new { Resoluciones = list.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Resolucion) }).ConfigureAwait(false);

            return resolucionEntity;
        }


        public async Task<int> GetFacturaelectronicaPorPRefijo(string prefijo)
        {

            var paramList = new DynamicParameters();
            paramList.Add("prefijo", prefijo);
            return await _sqlHelper.InsertOrUpdateOrDeleteAsync("getNumeration", new { prefijo = prefijo });
        }
        public async Task SetFacturaelectronicaPorPRefijo(string prefijo, int numeroActual)
        {

            var paramList = new DynamicParameters();
            paramList.Add("prefijo", prefijo);
            paramList.Add("numeroActual", numeroActual);
            await _sqlHelper.InsertOrUpdateOrDeleteAsync("setNumeration", new { prefijo = prefijo, numeroActual = numeroActual });
        }

        public async Task AnularResolucion(Guid resolucion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("resolucion", resolucion);
            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AnularResolucion, paramList);
        }

        public async Task<IEnumerable<Resolucion>> GetResolucionActiva(Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("estacion", estacion);
            return (await _sqlHelper.GetsAsync<Resolucion>(StoredProcedures.GetResolucionActiva, paramList));
        }

        public async Task<Resolucion> HabilitarResolucion(Guid estacion, DateTime fechaVencimiento)
        {
            var paramList = new DynamicParameters();
            paramList.Add("fechaVencimiento", fechaVencimiento);
            paramList.Add("resolucion", estacion);
            return (await _sqlHelper.GetsAsync<Resolucion>(StoredProcedures.HabilitarResolucion, paramList)).FirstOrDefault();
           
        }

        public async Task UpdateConsecutivoResolucion(int consecutivo)
        {
            var paramList = new DynamicParameters();
            paramList.Add("consecutivo", consecutivo);

            await _sqlHelper.GetsAsync<int>(StoredProcedures.UpdateConsecutivoResolucion, paramList);
            
        }
    }
}
