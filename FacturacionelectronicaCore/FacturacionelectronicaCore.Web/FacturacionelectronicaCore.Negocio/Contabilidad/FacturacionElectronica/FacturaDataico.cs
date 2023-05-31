using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ActionsDataico
    {
        public bool send_dian { get; set; }
        public bool send_email { get; set; }
    }

    public class CustomerDataico
    {
        public string email { get; set; }
        public string phone { get; set; }
        public string party_identification_type { get; set; }
        public string party_identification { get; set; }
        public string party_type { get; set; }
        public string tax_level_code { get; set; }
        public string regimen { get; set; }
        public string department { get; set; }
        public string city { get; set; }
        public string address_line { get; set; }
        public string country_code { get; set; }
        public string company_name { get; set; }
        public string first_name { get; set; }
        public string family_name { get; set; }
    }

    public class InvoiceDataico
    {
        public string env { get; set; }
        public string dataico_account_id { get; set; }
        public int number { get; set; }
        public string issue_date { get; set; }
        public string payment_date { get; set; }
        public string order_reference { get; set; }
        public string invoice_type_code { get; set; }
        public string payment_means { get; set; }
        public string payment_means_type { get; set; }
        public NumberingDataico numbering { get; set; }
        public List<string> notes { get; set; }
        public CustomerDataico customer { get; set; }
        public List<ItemDataico> items { get; set; }
    }

    public class ItemDataico
    {
        public string sku { get; set; }
        public double price { get; set; }
        public double quantity { get; set; }
        public string description { get; set; }
        public List<TaxisDataico> taxes { get; set; }
        public List<RetentionDataico> retentions { get; set; }
        public string measuring_unit { get; set; }
    }

    public class NumberingDataico
    {
        public string resolution_number { get; set; }
        public string prefix { get; set; }
        public bool flexible { get; set; }
    }

    public class RetentionDataico
    {
        public string tax_category { get; set; }
        public string tax_rate { get; set; }
    }

    public class FacturaDataico
    {
        public ActionsDataico actions { get; set; }
        public InvoiceDataico invoice { get; set; }
    }

    public class TaxisDataico
    {
        public string tax_category { get; set; }
        public string tax_rate { get; set; }
    }


}
