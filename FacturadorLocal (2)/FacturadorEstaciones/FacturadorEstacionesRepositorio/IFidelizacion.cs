using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public interface IFidelizacion
    {
        Task SubirPuntops(float total, string documentoFidelizado, string factura);
    }
}
