
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public static class AlegraExtensions
    {
        public static IServiceCollection AddAlegra(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IFacturacionElectronicaFacade, AlegraFacade>();

            services.Configure<Alegra>(options => configuration.GetSection("Alegra").Bind(options));
            return services;
        }

        public static Invoice ConvertirAInvoice(this Modelo.Factura factura, Item item)
        {
            return new Invoice()
            {
                client = factura.Tercero.idFacturacion,
                date = factura.Fecha.ToString("yyyy-MM-dd"),
                dueDate = factura.Fecha.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm(factura.FormaDePago),
                paymentMethod = GetPaymentMethod(factura.FormaDePago),
                stamp = new Stamp() { generateStamp = true },
                items = new System.Collections.Generic.List<ItemInvoice>() {
                 new ItemInvoice()
                 {
                     id = item.id,
                     description = factura.Combustible,
                     price = (int)factura.Precio,
                     quantity = factura.Cantidad.ToString("0.##"),
                     discount = ((factura.Descuento  / factura.SubTotal) * 100m).ToString("0.##").Replace(",",".")

                 }
                 }
            };
        }
        public static Invoice ConvertirAInvoice(this Modelo.OrdenDeDespacho orden, Item item)
        {
            return new Invoice()
            {
                client = orden.Tercero.idFacturacion,
                date = orden.Fecha.ToString("yyyy-MM-dd"),
                dueDate = orden.Fecha.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm(orden.FormaDePago),
                paymentMethod = GetPaymentMethod(orden.FormaDePago),
                 stamp = new Stamp() { generateStamp = true},
                 items = new System.Collections.Generic.List<ItemInvoice>() { 
                 new ItemInvoice()
                 {
                     id = item.id,
                     description = orden.Combustible,
                     price = (int)orden.Precio,
                     quantity = orden.Cantidad.ToString("0.##"),
                     discount =  ((orden.Descuento / orden.SubTotal) * 100m).ToString("0.##").Replace(",", ".")

                 }
                 }
            };
        }

        public static Invoice ConvertirAInvoice(this List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var itemsToFactura = new System.Collections.Generic.List<ItemInvoice>();
            foreach (var orden in ordenes)
            {
                itemsToFactura.Add(new ItemInvoice()
                {
                    id = items.First(x=>x.name == orden.Combustible).id,
                    description = orden.Combustible,
                    price = (int)orden.Precio,
                    quantity = orden.Cantidad.ToString("0.##"),
                    discount = ((orden.Descuento / orden.SubTotal) * 100m).ToString("0.##").Replace(",", ".")

                });
                 
            }
            return new Invoice()
            {
                client = tercero.idFacturacion,
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                dueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm("Efectivo"),
                paymentMethod = GetPaymentMethod("Efectivo"),
                stamp = new Stamp() { generateStamp = true },
                items = itemsToFactura
            };
        }


        public static Invoice ConvertirAInvoice(this List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var itemsToFactura = new System.Collections.Generic.List<ItemInvoice>();
            foreach (var factura in facturas)
            {
                itemsToFactura.Add(new ItemInvoice()
                {
                    id = items.First(x => x.name == factura.Combustible).id,
                    description = factura.Combustible,
                    price = (int)factura.Precio,
                    quantity = factura.Cantidad.ToString("0.##"),
                    discount = ((factura.Descuento / factura.SubTotal) * 100m).ToString("0.##").Replace(",", ".")

                });

            }
            return new Invoice()
            {
                client = tercero.idFacturacion,
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                dueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm("Efectivo"),
                paymentMethod = GetPaymentMethod("Efectivo"),
                stamp = new Stamp() { generateStamp = true },
                items = itemsToFactura
            };
        }
        private static string GetPaymentMethod(string formaDePago)
        {
            if (formaDePago == "Efectivo")
            {
                return "INSTRUMENT_NOT_DEFINED";
            }
            return "INSTRUMENT_NOT_DEFINED";
        }

        private static string GetPaymentForm(string formaDePago)
        {
            if(formaDePago == "Efectivo")
            {
                return "CASH";
            }
            else
            {
                return "CASH";//return "CREDIT";
            }
        }

        public static Contacts ConvertirAContact(this Modelo.Tercero tercero)
        {
            return new Contacts()
            {
                name = new Name() { firstName = tercero.Nombre, lastName = tercero.Apellidos, secondName = tercero.Segundo },
                identificationObject = new Identification() { number = tercero.Identificacion, type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion) },
                address = new Address() { city = tercero .Municipio, country = tercero.Pais, department = tercero.Departamento, description = tercero.Direccion, zipCode = tercero.CodigoPostal },
                comments = tercero.Comentarios,
                email = tercero.Correo,
                kindOfPerson = GetKindOfPErson(tercero.TipoPersona),
                mobile = tercero.Celular,
                phonePrimary = tercero.Telefono,
                phoneSecondary = tercero.Telefono2,
                observations = tercero.Comentarios,
                regime = GetRegime(tercero.ResponsabilidadTributaria),
                seller = tercero.Vendedor,
                settings = new Settings() { sendElectronicDocuments = true},
                type = new System.Collections.Generic.List<string>() { "client" }
            };
        }

        private static string GetRegime(int responsabilidadTributaria)
        {
            switch (responsabilidadTributaria)
            {
                case 1:
                    return "COMMON_REGIME";
                case 2:
                    return "SIMPLIFIED_REGIME";
                case 3:
                    return "NOT_REPONSIBLE_FOR_CONSUMPTION";
                case 4:
                    return "SPECIAL_REGIME";
                case 5:
                    return "NATIONAL_CONSUMPTION_TAX";
                default:
                    return "COMMON_REGIME";
            }
        }

        private static string GetKindOfPErson(int tipoPersona)
        {
            switch (tipoPersona)
            {
                case 1:
                    return "LEGAL_ENTITY";
                case 2:
                    return "PERSON_ENTITY";
                default:
                    return "LEGAL_ENTITY";
            }
        }

        private static string GetTipoIdentificacion(string descripcionTipoIdentificacion)
        {
            if(descripcionTipoIdentificacion == "Nit")
            {
                return "NIT";
            }
            else
            {
                return "CC";
            }
        }
    }
}
