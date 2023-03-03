using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.Alegra
{
    public class Category
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Price
    {
        public int idPriceList { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string price { get; set; }
    }

    public class Inventory
    {
        public string unit { get; set; }
    }

    public class Item
    {
        public int id { get; set; }
        public Category category { get; set; }
        public bool hasNoIvaDays { get; set; }
        public string name { get; set; }
        public object description { get; set; }
        public object reference { get; set; }
        public string status { get; set; }
        public List<Price> price { get; set; }
        public Inventory inventory { get; set; }
        public List<object> tax { get; set; }
        public List<object> customFields { get; set; }
        public object productKey { get; set; }
        public string type { get; set; }
        public string itemType { get; set; }
    }
}
