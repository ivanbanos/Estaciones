using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class FacturaMongo:Factura
    {
        public FacturaMongo() { }

        public DateTime FechaReporte { get; set; }
        public Guid EstacionGuid { get; set; }
    }
}
