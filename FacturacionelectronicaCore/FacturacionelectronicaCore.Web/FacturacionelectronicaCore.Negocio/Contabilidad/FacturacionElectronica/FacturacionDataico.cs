using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturacionDataico : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionesHandler resolucionesHandler;
        private readonly ResolucionNumber _resolucionNumber;

        public FacturacionDataico(IOptions<Alegra> alegra, ResolucionNumber resolucionNumber)
        {
            alegraOptions = alegra.Value;

            _resolucionNumber = resolucionNumber;
            if (_resolucionNumber.number == 0)
            {
                _resolucionNumber.number= alegraOptions.Current;
            }
        }

        public async Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion)
        {
            await contactsHandler.ActualizarCliente(idFacturacion, tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero)
        {
            var invoice = GetFacturaDataico(factura, tercero);
            while (true)
            {

                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                    client.DefaultRequestHeaders.Add("auth-token", alegraOptions.Auth);
                    var path = $"{alegraOptions.Url}invoices";
                    var content = new StringContent(JsonConvert.SerializeObject(invoice));
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    var response = client.PostAsync(path, content).Result;
                    string responseBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                            if(respuestaError.errors.Any(x=>x.path.Any(y=>y== "invoice"))) {
                                var error = respuestaError.errors.First(x => x.path.Any(y => y == "invoice"));
                                if(error.error.Contains("Tiene que ser el siguiente"))
                                {
                                    var numberpos = error.error.IndexOf('\'');
                                    numberpos = error.error.IndexOf('\'', numberpos);
                                    numberpos = error.error.IndexOf('\'', numberpos);
                                    var fin = error.error.IndexOf('\'', numberpos);
                                    _resolucionNumber.number=Int32.Parse( error.error.Substring(numberpos+1, fin - numberpos-2));
                                }
                                else
                                {
                                    _resolucionNumber.number++;
                                }
                                invoice.invoice.number = _resolucionNumber.number;
                            }
                            else
                            {
                                throw new AlegraException(responseBody);

                            }
                        }
                        catch (Exception)
                        {
                            throw new AlegraException(responseBody);
                        }
                    }

                    var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                    return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe;
                }
            }
        }

        public FacturaDataico GetFacturaDataico(Modelo.Factura factura, Modelo.Tercero tercero)
        {
            return new FacturaDataico()
            {
                actions = new ActionsDataico() { send_dian = false, send_email = true },
                invoice = new InvoiceDataico()
                {
                    env = "PRODUCCION",
                    dataico_account_id = alegraOptions.DataicoAccountId,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = factura.DescripcionResolucion + factura.Consecutivo,
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = "CASH",
                    payment_means_type = "DEBITO",
                    number = 0,
                    numbering = new NumberingDataico()
                    {
                        resolution_number = alegraOptions.ResolutionNumber,
                        flexible = false,
                        prefix = alegraOptions.Prefix
                    },
                    customer = new CustomerDataico()
                    {
                        email = tercero.Correo,
                        phone = tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.TipoPersona),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        city = "001",
                        address_line = tercero.Direccion,
                        country_code = "CO",
                        first_name = tercero.Nombre,
                        family_name = tercero.Apellidos,
                    },
                    items = new List<ItemDataico>(){
                        new ItemDataico()
                        {
                            sku="0",
                            price=(double)factura.Precio,
                            description=factura.Combustible,
                            quantity=(double)factura.Cantidad
                        }
                    }
                }
            };
        }

        private static string GetRegime(int responsabilidadTributaria)
        {
            switch (responsabilidadTributaria)
            {
                case 1:
                    return "AGENTE_RETENCION_IVA";
                case 2:
                    return "SIMPLE";
                case 3:
                    return "AUTORRETENEDOR";
                case 4:
                    return "AGENTE_RETENCION_IVA";
                case 5:
                    return "GRAN_CONTRIBUYENTE";
                default:
                    return "ORDINARIO";
            }
        }

        private static string GetNivelTributario(int responsabilidadTributaria)
        {
            switch (responsabilidadTributaria)
            {
                case 1:
                    return "COMUN";
                case 2:
                    return "SIMPLIFICADO";
                case 3:
                    return "NO_RESPONSABLE_DE_IVA";
                case 4:
                    return "COMUN";
                case 5:
                    return "RESPONSABLE_DE_IVA";
                default:
                    return "COMUN";
            }
        }

        private string GetTipoIdentificacion(string descripcionTipoIdentificacion)
        {
            if (descripcionTipoIdentificacion == "Nit")
            {
                return "NIT";
            }
            else
            {
                return "CC";
            }
        }
        private string GetKindOfPErson(int tipoPersona)
        {
            switch (tipoPersona)
            {
                case 1:
                    return "PERSONA_JURIDICA";
                case 2:
                    return "PERSONA_NATURAL";
                default:
                    return "PERSONA_JURIDICA";
            }
        }
        public FacturaDataico GetFacturaDataico(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero)
        {
            return new FacturaDataico()
            {
                actions = new ActionsDataico() { send_dian = false, send_email = true },
                invoice = new InvoiceDataico()
                {
                    env = "PRODUCCION",
                    dataico_account_id = alegraOptions.DataicoAccountId,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = orden.IdFactura.ToString(),
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = "CASH",
                    payment_means_type = "DEBITO",
                    number = 0,
                    numbering = new NumberingDataico()
                    {
                        resolution_number = alegraOptions.ResolutionNumber,
                        flexible = false,
                        prefix = alegraOptions.Prefix
                    },
                    customer = new CustomerDataico()
                    {
                        email = tercero.Correo,
                        phone = tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.TipoPersona),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        city = "001",
                        address_line = tercero.Direccion,
                        country_code = "CO",
                        first_name = tercero.Nombre,
                        family_name = tercero.Apellidos,
                    },
                    items = new List<ItemDataico>(){
                        new ItemDataico()
                        {
                            sku="0",
                            price=(double)orden.Precio,
                            description=orden.Combustible,
                            quantity=(double)orden.Cantidad
                        }
                    }
                }
            };
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero)
        {
            var invoice = GetFacturaDataico(orden, tercero);

            while (true)
            {

                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                    client.DefaultRequestHeaders.Add("auth-token", alegraOptions.Auth);
                    var path = $"{alegraOptions.Url}invoices";
                    var content = new StringContent(JsonConvert.SerializeObject(invoice));
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    var response = client.PostAsync(path, content).Result;
                    string responseBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                            if (respuestaError.errors.Any(x => x.path.Any(y => y == "invoice")))
                            {
                                var error = respuestaError.errors.First(x => x.path.Any(y => y == "invoice"));
                                if (error.error.Contains("Tiene que ser el siguiente"))
                                {
                                    var numberpos = error.error.IndexOf('\'');
                                    numberpos = error.error.IndexOf('\'', numberpos);
                                    numberpos = error.error.IndexOf('\'', numberpos);
                                    var fin = error.error.IndexOf('\'', numberpos);
                                    _resolucionNumber.number = Int32.Parse(error.error.Substring(numberpos + 1, fin - numberpos - 2));
                                }
                                else
                                {
                                    _resolucionNumber.number++;
                                }
                                invoice.invoice.number = _resolucionNumber.number;
                            }
                            else
                            {
                                throw new AlegraException(responseBody);

                            }
                        }
                        catch (Exception)
                        {
                            throw new AlegraException(responseBody);
                        }
                    }

                    var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                    return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe;
                }
            }
        }

        public async Task<string> GenerarFacturaElectronica(List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var invoice = await invoiceHandler.CrearFatura(ordenes.ConvertirAInvoice(tercero, items), alegraOptions);
            return invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;
        }
        public async Task<string> GenerarFacturaElectronica(List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var invoice = await invoiceHandler.CrearFatura(facturas.ConvertirAInvoice(tercero, items), alegraOptions);
            return invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;
        }

        public async Task<int> GenerarTercero(Modelo.Tercero tercero)
        {
            return (await contactsHandler.CrearCliente(tercero.ConvertirAContact(), alegraOptions));
        }

        public async Task<ResponseInvoice> GetFacturaElectronica(string id)
        {
            return await invoiceHandler.GetFatura(id, alegraOptions);
        }

        public async Task<Item> GetItem(string name)
        {
            return await itemHandler.GetItem(name, alegraOptions);
        }

        public async Task<ResolucionElectronica> GetResolucionElectronica()
        {
            var resoluciones = await resolucionesHandler.GetResolucionesElectronica(alegraOptions);
            return resoluciones.FirstOrDefault(x => x.isDefault);
        }

        public async Task<IEnumerable<TerceroResponse>> GetTerceros(int start)
        {

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", alegraOptions.Auth);
                var path = $"{alegraOptions.Url}contacts/?start={start}";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    throw new AlegraException(responseBody);
                }

                return JsonConvert.DeserializeObject<IEnumerable<TerceroResponse>>(responseBody);
            }
        }
    }
}
