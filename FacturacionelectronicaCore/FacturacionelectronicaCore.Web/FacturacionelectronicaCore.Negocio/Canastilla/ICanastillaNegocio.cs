using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Canastilla
{
    public interface ICanastillaNegocio
    {
        Task<int> AddOrUpdate(IEnumerable<Modelo.Canastilla> terceros);
        Task<IEnumerable<Modelo.Canastilla>> GetCanastillas();
        Task<Modelo.Canastilla> GetCanastilla(Guid guid);
    }
}
