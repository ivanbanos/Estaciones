using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturacionSiigo : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionesHandler resolucionesHandler;
        private readonly ResolucionNumber _resolucionNumber;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private readonly IEmpleadoRepositorio _empleadoRepositorio;

        public FacturacionSiigo(IOptions<Alegra> alegra, ResolucionNumber resolucionNumber, IResolucionRepositorio resolucionRepositorio, IEmpleadoRepositorio empleadoRepositorio)
        {
            alegraOptions = alegra.Value;

            _resolucionNumber = resolucionNumber;

            _resolucionRepositorio = resolucionRepositorio;
            _empleadoRepositorio = empleadoRepositorio;
        }

        public async Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion)
        {
            await contactsHandler.ActualizarCliente(idFacturacion, tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            try
            {

                var invoice = await GetFacturaSiigo(factura, tercero, estacionGuid);
                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                using (var client = new HttpClient())
                {
                    try
                    {
                        var token = await GetToken();
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Add("Partner-Id", "SIGES");
                        var content = new StringContent(JsonConvert.SerializeObject(invoice));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        Console.WriteLine($"{alegraOptions.Url}v1/invoices");
                        var response = await client.PostAsync($"{alegraOptions.Url}v1/invoices", content);
                        string responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseBody);
                        response.EnsureSuccessStatusCode();

                        var respuestaSiigo = JsonConvert.DeserializeObject<FacturaSiigoResponse>(responseBody);
                        if (responseBody.ToLower().Contains("error"))
                        {
                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                        }
                        else
                        {
                            Console.WriteLine("Ok:" + factura.IdVentaLocal + ":" + respuestaSiigo.stamp.cufe);
                            return "Ok:" + respuestaSiigo.prefix + respuestaSiigo.number + ":" + (respuestaSiigo.stamp.cufe ?? respuestaSiigo.prefix + respuestaSiigo.number);

                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
        private string GetTipoIdentificacion(string descripcionTipoIdentificacion)
        {
            if (descripcionTipoIdentificacion == "Nit")
            {
                return "Company";
            }
            else
            {
                return "Person";
            }
        }
        public async Task<FacturaSiigo> GetFacturaSiigo(Modelo.Factura x, Modelo.Tercero tercero, Guid estacionGuid)
        {
            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.Contains("no informado"))
            {
                if (nombreCompleto.Split(' ').Count() > 1)
                {
                    nombre = nombreCompleto.Substring(0, nombreCompleto.LastIndexOf(" "));
                    apellido = nombreCompleto.Split(' ').Last();
                }
                else
                {
                    nombre = nombreCompleto;
                    apellido = "no informado";
                }
            }
            else
            {
                nombre = nombreCompleto;
                apellido = tercero.Apellidos;
            }
            return new FacturaSiigo()
            {

                stamp = new StampSiigoRequest { send = true },
                document = new DocumentSiigoRequest { id = alegraOptions.Documento },
                date = x.Fecha.ToString("yyyy-MM-dd"),
                customer = new CustomerSiigoRequest
                {
                    person_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                    name = new List<string>() { nombre, apellido },
                    id_type = GetTipoIdentificacionNumero(tercero.DescripcionTipoIdentificacion),
                    identification = tercero.Identificacion,
                    branch_office = 0,
                    address = new AddressSiigoRequest
                    {
                        address = "Cra. 18 #79A - 42",
                        city = new CitySiigoRequest
                        {
                            country_code = "Co",
                            state_code = "11",
                            city_code = "11001"
                        }
                    },
                    phones = new List<PhoneSiigoRequest> { new PhoneSiigoRequest { number = string.IsNullOrEmpty(tercero.Telefono) || tercero.Telefono == "no informado" ? "3000000000" : tercero.Telefono, } },
                    contacts = new List<ContactSiigoRequest> { new ContactSiigoRequest {
                        first_name = nombre,
                        last_name = apellido, email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo == "no informado" ? alegraOptions.Correo : tercero.Correo, } }
                },
                seller = alegraOptions.Seller,
                payments = new List<PaymentSiigoRequest>
                {
                    new PaymentSiigoRequest{ id = GetPaymentType(x.FormaDePago), value =Math.Round( Convert.ToDouble(Math.Round(x.Precio,2)*Math.Round(x.Cantidad,2)),2)}
                },
                items = new List<ItemSiigoRequest>()
                {
                    new ItemSiigoRequest
                    {
                        code = GetCodeCombustible(x.Combustible),
                        description = x.Combustible,
                        discount = Convert.ToInt32(x.Descuento),
                        price =  Math.Round(Convert.ToDouble(x.Precio), 2),
                        quantity = Math.Round(Convert.ToDouble(x.Cantidad),2),

                    }
                },
                cost_center = 85
            };
        }

        private string GetCodeCombustible(string combustible)
        {
            if (combustible.ToLower().Contains("acpm"))
            {
                return "111";
            }
            else if (combustible.ToLower().Contains("corri"))
            {
                return "112";
            }
            else
            {
                return "113";
            }
        }

        private int GetPaymentType(string formaDePago)
        {
            if (formaDePago.ToLower().Contains("efectivo"))
            {
                return 7765;
            }
            else
            {
                return 4225;
            }
        }

        private string GetTipoIdentificacionNumero(string descripcionTipoIdentificacion)
        {
            if (descripcionTipoIdentificacion == "Nit")
            {
                return "31";
            }
            else
            {
                return "13";
            }
        }

        public async Task<FacturaSiigo> GetFacturaSiigo(Modelo.OrdenDeDespacho x, Modelo.Tercero tercero, Guid estacionGuid)
        {
            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.Contains("no informado"))
            {
                if (nombreCompleto.Split(' ').Count() > 1)
                {
                    nombre = nombreCompleto.Substring(0, nombreCompleto.LastIndexOf(" "));
                    apellido = nombreCompleto.Split(' ').Last();
                }
                else
                {
                    nombre = nombreCompleto;
                    apellido = "no informado";
                }
            }
            else
            {
                nombre = nombreCompleto;
                apellido = tercero.Apellidos;
            }
            return new FacturaSiigo()
            {

                stamp = new StampSiigoRequest { send = true },
                document = new DocumentSiigoRequest { id = alegraOptions.Documento },
                date = x.Fecha.ToString("yyyy-MM-dd"),
                
                customer = new CustomerSiigoRequest
                {
                    person_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                    name = new List<string>() { nombre, apellido },
                    id_type = GetTipoIdentificacionNumero(tercero.DescripcionTipoIdentificacion),
                    identification = tercero.Identificacion,
                    branch_office = 0,
                    address = new AddressSiigoRequest
                    {
                        address = "Cra. 18 #79A - 42",
                        city = new CitySiigoRequest
                        {
                            country_code = "Co",
                            state_code = "11",
                            city_code = "11001"
                        }
                    },
                    phones = new List<PhoneSiigoRequest> { new PhoneSiigoRequest { number = string.IsNullOrEmpty(tercero.Telefono) || tercero.Telefono == "no informado" ? "3000000000" : tercero.Telefono, } },
                    contacts = new List<ContactSiigoRequest> { new ContactSiigoRequest {
                        first_name = nombre,
                        last_name = apellido, email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo == "no informado" ? alegraOptions.Correo : tercero.Correo, } }
                },
                seller = alegraOptions.Seller,
                payments = new List<PaymentSiigoRequest>
                {
                    new PaymentSiigoRequest{ id = GetPaymentType(x.FormaDePago), value =Math.Round( Convert.ToDouble(Math.Round(x.Precio,2)*Math.Round(x.Cantidad,2)),2)}
                },
                items = new List<ItemSiigoRequest>()
                {
                    new ItemSiigoRequest
                    {
                        code = GetCodeCombustible(x.Combustible),
                        description = x.Combustible,
                        discount = Convert.ToInt32(x.Descuento),
                        price =  Math.Round(Convert.ToDouble(x.Precio), 2),
                        quantity = Math.Round(Convert.ToDouble(x.Cantidad),2),

                    }
                },
                cost_center = 85
            };
        }

        public async Task<string> GetToken()
        {

            try
            {
                var tokenrequets = new SiigoTokenRequest
                {
                    access_key = alegraOptions.AccessKey,
                    username = alegraOptions.Correo

                };
                Console.WriteLine(JsonConvert.SerializeObject(tokenrequets));

                using (var client = new HttpClient())
                {
                    try
                    {

                        var content = new StringContent(JsonConvert.SerializeObject(tokenrequets));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        Console.WriteLine($"{alegraOptions.Url}auth");
                        var response = await client.PostAsync($"{alegraOptions.Url}auth", content);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        var respuestaSiigo = JsonConvert.DeserializeObject<SiigoTokenResponse>(responseBody);

                        return respuestaSiigo.access_token;
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }


        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {

            try
            {

                var invoice = await GetFacturaSiigo(orden, tercero, estacionGuid);

                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                using (var client = new HttpClient())
                {
                    try
                    {
                        var token = await GetToken();
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Add("Partner-Id", "SIGES");
                        var content = new StringContent(JsonConvert.SerializeObject(invoice));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        Console.WriteLine($"{alegraOptions.Url}v1/invoices");
                        var response = await client.PostAsync($"{alegraOptions.Url}v1/invoices", content);
                        string responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseBody);
                        response.EnsureSuccessStatusCode();

                        var respuestaSiigo = JsonConvert.DeserializeObject<FacturaSiigoResponse>(responseBody);
                        if (responseBody.ToLower().Contains("error"))
                        {
                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                        }
                        else
                        {
                            Console.WriteLine("Ok:" + orden.IdVentaLocal + ":" + respuestaSiigo.stamp.cufe);
                            return "Ok:" + respuestaSiigo.prefix+respuestaSiigo.number + ":" + (respuestaSiigo.stamp.cufe ?? respuestaSiigo.prefix + respuestaSiigo.number);

                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                throw;
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

        public async Task<ResolucionElectronica> GetResolucionElectronica(string estacion)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                client.DefaultRequestHeaders.Add("auth-token", alegraOptions.Token);
                var path = $"{alegraOptions.Url}numberings/invoice";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {

                }

                var respuesta = JsonConvert.DeserializeObject<ResolucionesDataico>(responseBody);
                Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                return new ResolucionElectronica(respuesta.numberings.First());
            }
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
        public async Task<string> getJson(Modelo.OrdenDeDespacho ordenDeDespachoEntity, Guid estacion)
        {
            throw new NotImplementedException();
        }
    }
}