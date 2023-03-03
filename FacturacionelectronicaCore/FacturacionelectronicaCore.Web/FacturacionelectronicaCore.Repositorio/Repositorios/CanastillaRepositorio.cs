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
    public class CanastillaRepositorio : ICanastillaRepositorio
    {
        private readonly ISQLHelper _sqlHelper;

        public CanastillaRepositorio(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        /// <inheritdoc />
        public async Task AddRange(IEnumerable<Canastilla> lists)
        {
            var dataTable = lists.ToDataTable();
            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.UpdateOrCreateCanastilla,
                new { canastillas = dataTable.AsTableValuedParameter(UserDefinedTypes.Canastilla) }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Canastilla>> GetCanastillas()
        {
            return await _sqlHelper.GetsAsync<Canastilla>(StoredProcedures.GetCanastilla);
        }

    }
}
