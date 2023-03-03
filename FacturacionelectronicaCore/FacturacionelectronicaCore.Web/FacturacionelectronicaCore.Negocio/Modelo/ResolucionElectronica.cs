using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class ResolucionElectronica
    {
        public string id { get; set; }
        public string name { get; set; }
        public string prefix { get; set; }
        public string invoiceText { get; set; }
        public bool isDefault { get; set; }
    }
}
