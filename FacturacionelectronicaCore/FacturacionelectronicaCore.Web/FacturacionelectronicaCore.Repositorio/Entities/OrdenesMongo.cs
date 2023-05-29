using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class OrdenesMongo : OrdenDeDespacho
    {
        public OrdenesMongo() { }

        public DateTime FechaReporte { get; set; }
        public Guid EstacionGuid { get; set; }
    }
}
