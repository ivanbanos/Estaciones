using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AdditionalFieldsSiigo
    {
    }

    public class AddressSiigo
    {
        public string address { get; set; }
        public CitySiigo city { get; set; }
        public string postal_code { get; set; }
    }

    public class CitySiigo
    {
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string state_code { get; set; }
        public string state_name { get; set; }
        public string city_code { get; set; }
        public string city_name { get; set; }
    }

    public class ContactSiigo
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public PhoneSiigo phone { get; set; }
    }

    public class CurrencySiigo
    {
        public string code { get; set; }
        public double exchange_rate { get; set; }
    }

    public class CustomerSiigo
    {
        public string person_type { get; set; }
        public string id_type { get; set; }
        public string identification { get; set; }
        public int branch_office { get; set; }
        public List<string> name { get; set; }
        public AddressSiigo address { get; set; }
        public List<PhoneSiigo> phones { get; set; }
        public List<ContactSiigo> contacts { get; set; }
    }

    public class DocumentSiigo
    {
        public int id { get; set; }
    }

    public class ItemSiigo
    {
        public string code { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public double price { get; set; }
        public int discount { get; set; }
        public List<TaxisSiigo> taxes { get; set; }
    }

    public class MailSiigo
    {
        public bool send { get; set; }
    }

    public class PaymentSiigo
    {
        public int id { get; set; }
        public double value { get; set; }
        public string due_date { get; set; }
    }

    public class PhoneSiigo
    {
        public string indicative { get; set; }
        public string number { get; set; }
        public string extension { get; set; }
    }

    public class Phone2Siigo
    {
        public string indicative { get; set; }
        public string number { get; set; }
        public string extension { get; set; }
    }

    public class FacturaSiigo
    {
        public DocumentSiigo document { get; set; }
        public string date { get; set; }
        public CustomerSiigo customer { get; set; }
        public int cost_center { get; set; }
        public CurrencySiigo currency { get; set; }
        public int seller { get; set; }
        public StampSiigo stamp { get; set; }
        public MailSiigo mail { get; set; }
        public string observations { get; set; }
        public List<ItemSiigo> items { get; set; }
        public List<PaymentSiigo> payments { get; set; }
        public AdditionalFieldsSiigo additional_fields { get; set; }
    }

    public class StampSiigo
    {
        public bool send { get; set; }
    }

    public class TaxisSiigo
    {
        public int id { get; set; }
    }

}
