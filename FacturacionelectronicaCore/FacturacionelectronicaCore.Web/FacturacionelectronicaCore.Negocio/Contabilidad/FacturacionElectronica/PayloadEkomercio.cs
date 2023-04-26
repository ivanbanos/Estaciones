using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class PayloadEkomercio
    {
        public List<Factura> factura { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Factura
    {
        public string nro { get; set; }
        public string fecha { get; set; }
        public string id_cliente { get; set; }
        public string nombre_cliente { get; set; }
        public string vendedor { get; set; }
        public string forma_pago { get; set; }
        public string retencion { get; set; }
        public string rete_iva { get; set; }
        public string rete_ica { get; set; }
        public List<Item> items { get; set; }
    }

    public class Item
    {
        public string grupo { get; set; }
        public string codigo { get; set; }
        public string clase { get; set; }
        public string cantidad { get; set; }
        public string vlr_unit { get; set; }
        public string vlr_tot { get; set; }
        public string tarifa_iva { get; set; }
        public string vlr_iva { get; set; }
        public string vlr_dscto { get; set; }
    }



}
