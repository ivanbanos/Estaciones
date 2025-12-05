using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class AlegraFacade : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionesHandler resolucionesHandler;
        private readonly IResolucionRepositorio _resolucionRepositorio;

        public AlegraFacade(IOptions<Alegra> alegra, IResolucionRepositorio resolucionRepositorio)
        {
            alegraOptions = alegra.Value;
            contactsHandler = new ContactsHandler();
            invoiceHandler = new InvoiceHandler();
            itemHandler = new ItemHandler();
            resolucionesHandler = new ResolucionesHandler();
            _resolucionRepositorio = resolucionRepositorio;
        }

        public async Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion)
        {
            await contactsHandler.ActualizarCliente(idFacturacion, tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            var item = await GetItem(factura.Combustible, null);
            if (item == null)
            {
                return "Combustible no creado";
            }

            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
            var option = new Alegra() { Url = alegraOptions.Url, Token = resolucion?.token ?? alegraOptions.Token, Correo = resolucion?.correo ?? alegraOptions.Correo };
           
            var responseBody = await invoiceHandler.CrearFatura(factura.ConvertirAInvoice(item), option);
            var invoice = JsonConvert.DeserializeObject<ResponseInvoice>(responseBody);
            return invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;// + ":"+JsonConvert.SerializeObject(invoice);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {
            logger.Info($"[GenerarFacturaElectronica] Starting invoice generation for Order: {orden?.Combustible}, Estacion: {estacionGuid}, Tercero: {tercero?.Identificacion}");

            try
            {
                logger.Debug($"[GenerarFacturaElectronica] Getting resolution for estacion: {estacionGuid}");
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
                logger.Debug($"[GenerarFacturaElectronica] Resolution found: {resolucion != null}, Token available: {!string.IsNullOrEmpty(resolucion?.token)}");

                var option = new Alegra() { Url = alegraOptions.Url, Token = resolucion?.token ?? alegraOptions.Token, Correo = resolucion?.correo ?? alegraOptions.Correo };
                logger.Debug($"[GenerarFacturaElectronica] Alegra options configured - URL: {option.Url}, Email: {option.Correo}");

                logger.Info($"[GenerarFacturaElectronica] Getting item for combustible: {orden.Combustible}");
                var item = await GetItem(orden.Combustible, option);
                if (item == null)
                {
                    logger.Warn($"[GenerarFacturaElectronica] Item not found for combustible: {orden.Combustible}");
                    return $"error:Combustible no creado:{orden.Combustible}";
                }
                logger.Debug($"[GenerarFacturaElectronica] Item found - ID: {item.id}, Name: {item.name}");

                logger.Info($"[GenerarFacturaElectronica] Looking up tercero by identification: {tercero.Identificacion?.Trim()}");
                var id = 0;
                var contacts = await GetTerceroByIdentification(tercero.Identificacion.Trim(), estacionGuid);
                logger.Debug($"[GenerarFacturaElectronica] Found {contacts?.Count()} existing contacts");

                if (!contacts.Any())
                {
                    try
                    {
                        logger.Info($"[GenerarFacturaElectronica] Creating new contact for tercero: {tercero.Identificacion}");
                        var contact = tercero.ConvertirAContact();
                        logger.Debug($"[GenerarFacturaElectronica] Contact object: {JsonConvert.SerializeObject(contact)}");
                        Console.WriteLine(JsonConvert.SerializeObject(contact));

                        id = await contactsHandler.CrearCliente(contact, option);
                        logger.Info($"[GenerarFacturaElectronica] Contact created successfully with ID: {id}");

                        contacts = await GetTerceroByIdentification(tercero.Identificacion.Trim(), estacionGuid);
                        logger.Debug($"[GenerarFacturaElectronica] Re-fetched contacts count: {contacts?.Count()}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"[GenerarFacturaElectronica] Error creating contact for tercero {tercero.Identificacion}: {ex.Message}");
                        return "error:" + JsonConvert.SerializeObject(contacts) + ":" + JsonConvert.SerializeObject(orden.ConvertirAInvoice(item)) + ":" + JsonConvert.SerializeObject(tercero.ConvertirAContact()) + ":" + ex.Message + ":" + ex.StackTrace;
                    }
                }

                id = contacts?.First()?.id ?? 0;
                logger.Debug($"[GenerarFacturaElectronica] Using contact ID: {id}");

                var invoice = null as ResponseInvoice;
                logger.Info($"[GenerarFacturaElectronica] Creating invoice with contact ID: {id}");
                var invoiceRequest = orden.ConvertirAInvoice(item, id);
                var responseBody = string.Empty;
                try
                {
                    logger.Debug($"[GenerarFacturaElectronica] Invoice request data: {JsonConvert.SerializeObject(invoiceRequest)}");

                    responseBody = await invoiceHandler.CrearFatura(invoiceRequest, option);
                    invoice = JsonConvert.DeserializeObject<ResponseInvoice>(responseBody);
                    logger.Info($"[GenerarFacturaElectronica] Invoice created successfully - Number: {invoice?.numberTemplate?.prefix}{invoice?.numberTemplate?.number}, CUFE: {invoice?.stamp?.cufe}");

                    return "Ok:" + invoice?.numberTemplate?.prefix + invoice?.numberTemplate?.number + ":" + invoice?.stamp?.cufe + ":"+responseBody+":" + JsonConvert.SerializeObject(invoice) + ":" + JsonConvert.SerializeObject(orden.ConvertirAInvoice(item)) + ":" + JsonConvert.SerializeObject(tercero.ConvertirAContact());
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"[GenerarFacturaElectronica] Error creating invoice: {ex.Message}");
                    return "error:" + JsonConvert.SerializeObject(invoiceRequest) + ":" + responseBody + ":" + JsonConvert.SerializeObject(orden.ConvertirAInvoice(item, id)) + ":" + JsonConvert.SerializeObject(tercero.ConvertirAContact()) + JsonConvert.SerializeObject(contacts) + ":" + ex.Message + ":" + ex.StackTrace;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[GenerarFacturaElectronica] Unexpected error in invoice generation: {ex.Message}");
                return $"error:Unexpected error:{ex.Message}:{ex.StackTrace}";
            }
        }

        private async Task<IEnumerable<TerceroResponse>> GetTerceroByIdentification(string identificacion, Guid estacionGuid)
        {
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
            var option = new Alegra() { Url = alegraOptions.Url, Token = resolucion?.token ?? alegraOptions.Token, Correo = resolucion?.correo ?? alegraOptions.Correo };

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", option.Auth);
                var path = $"{option.Url}contacts?order_direction=ASC&identification={identificacion}";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {

                }
                try
                {

                    return JsonConvert.DeserializeObject<IEnumerable<TerceroResponse>>(responseBody);
                }
                catch (Exception ex)
                {
                    return new List<TerceroResponse>()
                    {
                        JsonConvert.DeserializeObject<TerceroResponse>(responseBody)
                    };
                }
            }
        }

        public async Task<string> GenerarFacturaElectronica(List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items)
        {

            //var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion);
            //var option = new Alegra() { Url = alegraOptions.Url, Token = resolucion?.token ?? alegraOptions.Token, Correo= resolucion?.correo ?? alegraOptions.Correo };
            
            var responseBody = await invoiceHandler.CrearFatura(ordenes.ConvertirAInvoice(tercero, items), alegraOptions);
            var invoice = JsonConvert.DeserializeObject<ResponseInvoice>(responseBody);
            return invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;
        }
        public async Task<string> GenerarFacturaElectronica(List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var responseBody = await invoiceHandler.CrearFatura(facturas.ConvertirAInvoice(tercero, items), alegraOptions);
            var invoice = JsonConvert.DeserializeObject<ResponseInvoice>(responseBody);
            return invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;
        }

        public async Task<int> GenerarTercero(Modelo.Tercero tercero)
        {
            return await contactsHandler.CrearCliente(tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<ResponseInvoice> GetFacturaElectronica(string id)
        {
            return await invoiceHandler.GetFatura(id, alegraOptions);
        }

        public async Task<Item> GetItem(string name, Alegra options)
        {
            return await itemHandler.GetItem(name, options);
        }

        public Task<string> getJson(Modelo.OrdenDeDespacho ordenDeDespachoEntity, Guid estacio)
        {
            throw new NotImplementedException();
        }

        public async Task<ResolucionElectronica> GetResolucionElectronica(string estacion)
        {
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion);
            var resoluciones = await resolucionesHandler.GetResolucionesElectronica(alegraOptions, resolucion);
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

        public async Task<string> GenerarFacturaElectronica(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            logger.Info($"[GenerarFacturaElectronica] Starting canastilla invoice generation for Factura: {factura?.consecutivo}, Estacion: {estacionGuid}, Tercero: {tercero?.Identificacion}");

            try
            {
                // Check if payment form should be excluded (similar to Silog implementation)
                if (factura.codigoFormaPago != null &&
                    (factura.codigoFormaPago.Descripcion.ToLower().Contains("cali") ||
                     factura.codigoFormaPago.Descripcion.ToLower().Contains("puntos") ||
                     factura.codigoFormaPago.Descripcion.ToLower().Contains("asumidos")))
                {
                    logger.Warn($"[GenerarFacturaElectronica] Canastilla invoice skipped due to payment form: {factura.codigoFormaPago.Descripcion}");
                    return null;
                }

                logger.Debug($"[GenerarFacturaElectronica] Getting resolution for estacion: {estacionGuid}");
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
                logger.Debug($"[GenerarFacturaElectronica] Resolution found: {resolucion != null}, Token available: {!string.IsNullOrEmpty(resolucion?.token)}");

                var option = new Alegra() { Url = alegraOptions.Url, Token = resolucion?.token ?? alegraOptions.Token, Correo = resolucion?.correo ?? alegraOptions.Correo };
                logger.Debug($"[GenerarFacturaElectronica] Alegra options configured - URL: {option.Url}, Email: {option.Correo}");

                // Get items for all canastilla products
                logger.Info($"[GenerarFacturaElectronica] Getting items for {factura.canastillas?.Count} canastilla products");
                var items = new List<Item>();

                foreach (var canastillaItem in factura.canastillas)
                {
                    logger.Debug($"[GenerarFacturaElectronica] Getting item for canastilla product: {canastillaItem.Canastilla.descripcion}");
                    var item = await GetItem(canastillaItem.Canastilla.descripcion, option);
                    if (item == null)
                    {
                        logger.Warn($"[GenerarFacturaElectronica] Item not found for canastilla product: {canastillaItem.Canastilla.descripcion}");
                        return $"error:Producto canastilla no creado:{canastillaItem.Canastilla.descripcion}";
                    }
                    items.Add(item);
                    logger.Debug($"[GenerarFacturaElectronica] Item found - ID: {item.id}, Name: {item.name}");
                }

                logger.Info($"[GenerarFacturaElectronica] Looking up tercero by identification: {tercero.Identificacion?.Trim()}");
                var id = 0;
                var contacts = await GetTerceroByIdentification(tercero.Identificacion.Trim(), estacionGuid);
                logger.Debug($"[GenerarFacturaElectronica] Found {contacts?.Count()} existing contacts");

                if (!contacts.Any())
                {
                    try
                    {
                        logger.Info($"[GenerarFacturaElectronica] Creating new contact for tercero: {tercero.Identificacion}");
                        var contact = tercero.ConvertirAContact();
                        logger.Debug($"[GenerarFacturaElectronica] Contact object: {JsonConvert.SerializeObject(contact)}");

                        id = await contactsHandler.CrearCliente(contact, option);
                        logger.Info($"[GenerarFacturaElectronica] Contact created successfully with ID: {id}");

                        contacts = await GetTerceroByIdentification(tercero.Identificacion.Trim(), estacionGuid);
                        logger.Debug($"[GenerarFacturaElectronica] Re-fetched contacts count: {contacts?.Count()}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"[GenerarFacturaElectronica] Error creating contact for tercero {tercero.Identificacion}: {ex.Message}");
                        return "error:" + JsonConvert.SerializeObject(contacts) + ":" + JsonConvert.SerializeObject(factura.ConvertirAInvoice(tercero, items)) + ":" + JsonConvert.SerializeObject(tercero.ConvertirAContact()) + ":" + ex.Message + ":" + ex.StackTrace;
                    }
                }

                id = contacts?.First()?.id ?? 0;
                logger.Debug($"[GenerarFacturaElectronica] Using contact ID: {id}");

                var invoice = null as ResponseInvoice;

                logger.Info($"[GenerarFacturaElectronica] Creating canastilla invoice with contact ID: {id} and {items.Count} items");
                var invoiceRequest = factura.ConvertirAInvoice(tercero, items, id);
                var responseBody = string.Empty;
                try
                {
                    logger.Debug($"[GenerarFacturaElectronica] Invoice request data: {JsonConvert.SerializeObject(invoiceRequest)}");

                    responseBody = await invoiceHandler.CrearFatura(invoiceRequest, option);
                    invoice = JsonConvert.DeserializeObject<ResponseInvoice>(responseBody);
                    logger.Info($"[GenerarFacturaElectronica] Canastilla invoice created successfully - Number: {invoice?.numberTemplate?.prefix}{invoice?.numberTemplate?.number}, CUFE: {invoice?.stamp?.cufe}");

                    return "Ok:" + invoice?.numberTemplate?.prefix + invoice?.numberTemplate?.number + ":" + invoice?.stamp?.cufe + ":" + JsonConvert.SerializeObject(invoice) + ":" + JsonConvert.SerializeObject(factura.ConvertirAInvoice(tercero, items)) + ":" + JsonConvert.SerializeObject(tercero.ConvertirAContact());
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"[GenerarFacturaElectronica] Error creating canastilla invoice: {ex.Message}");
                    return "error:" + JsonConvert.SerializeObject(invoiceRequest) + ":" + JsonConvert.SerializeObject(factura.ConvertirAInvoice(tercero, items, id)) + ":" + JsonConvert.SerializeObject(tercero.ConvertirAContact()) + JsonConvert.SerializeObject(contacts) + ":" + ex.Message + ":" + ex.StackTrace;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[GenerarFacturaElectronica] Unexpected error in canastilla invoice generation: {ex.Message}");
                return $"error:Unexpected error:{ex.Message}:{ex.StackTrace}";
            }
        }

        public Task<string> GetFacturaElectronica(string id, Guid estacionGuid)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReenviarFactura(Repositorio.Entities.OrdenDeDespacho orden, Guid estacion)
        {
            throw new NotImplementedException();
        }

        public async Task<string> getJsonCanastilla(Modelo.FacturaCanastilla facturaCanastilla, Guid estacio)
        {
            logger.Info($"[getJsonCanastilla] Generating JSON for canastilla factura: {facturaCanastilla?.consecutivo}, Estacion: {estacio}");

            try
            {
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacio.ToString());
                var option = new Alegra() { Url = alegraOptions.Url, Token = resolucion?.token ?? alegraOptions.Token, Correo = resolucion?.correo ?? alegraOptions.Correo };

                // Get items for all canastilla products
                var items = new List<Item>();
                foreach (var canastillaItem in facturaCanastilla.canastillas)
                {
                    var item = await GetItem(canastillaItem.Canastilla.descripcion, option);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }

                // Use the tercero from the factura
                var tercero = facturaCanastilla.terceroId;
                var invoiceRequest = facturaCanastilla.ConvertirAInvoice(tercero, items);

                logger.Debug($"[getJsonCanastilla] Generated JSON for canastilla factura");
                return JsonConvert.SerializeObject(invoiceRequest, Formatting.Indented);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[getJsonCanastilla] Error generating JSON for canastilla factura: {ex.Message}");
                return $"error:Error generating JSON:{ex.Message}:{ex.StackTrace}";
            }
        }
    }
}
