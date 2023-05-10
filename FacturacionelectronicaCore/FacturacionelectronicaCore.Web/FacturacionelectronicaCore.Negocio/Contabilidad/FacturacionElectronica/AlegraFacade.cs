using FacturacionelectronicaCore.Negocio.Modelo;
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
    public class AlegraFacade : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionesHandler resolucionesHandler;

        public AlegraFacade(IOptions<Alegra> alegra) {
            alegraOptions = alegra.Value;
            contactsHandler = new ContactsHandler();
            invoiceHandler = new InvoiceHandler();
            itemHandler = new ItemHandler();
            resolucionesHandler = new ResolucionesHandler();
        }

        public async Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion)
        {
            await contactsHandler.ActualizarCliente(idFacturacion, tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero)
        {
            var item = await GetItem(factura.Combustible);
            if (item == null)
            {
                return "Combustible no creado";
            }
            var invoice = await invoiceHandler.CrearFatura(factura.ConvertirAInvoice(item), alegraOptions);
                return invoice.numberTemplate.prefix+ invoice.numberTemplate.number + ":" + invoice.id;
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero)
        {
            var item = await GetItem(orden.Combustible);
            if (item == null)
            {
                return "Combustible no creado";
            }
            var invoice = await invoiceHandler.CrearFatura(orden.ConvertirAInvoice(item), alegraOptions);
            return invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;
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
            return resoluciones.FirstOrDefault(x=>x.isDefault);
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
