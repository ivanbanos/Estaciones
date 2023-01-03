using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public interface IApiContabilidad
    {
        IEnumerable<int> EnviarFacturas(IEnumerable<Factura> facturas);
    }
}
