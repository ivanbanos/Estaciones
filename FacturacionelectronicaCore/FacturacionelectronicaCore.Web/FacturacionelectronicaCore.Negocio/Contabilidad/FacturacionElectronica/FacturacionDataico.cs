using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturacionDataico : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionNumber _resolucionNumber;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private static readonly Semaphore _semaphore = new(initialCount: 1, maximumCount: 1);

        public FacturacionDataico(IOptions<Alegra> alegra, ResolucionNumber resolucionNumber, IResolucionRepositorio resolucionRepositorio)
        {
            alegraOptions = alegra.Value;

            _resolucionNumber = resolucionNumber;
            
           _resolucionRepositorio = resolucionRepositorio;
        }

        public async Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion)
        {
            await contactsHandler.ActualizarCliente(idFacturacion, tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            try
            {

                var invoice = await GetFacturaDataico(factura, tercero, estacionGuid.ToString());
                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                while (true)
                {

                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                        client.DefaultRequestHeaders.Add("auth-token", alegraOptions.Token);
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
                                if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                {
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos+1);
                                        numberpos = error.error.IndexOf('\'', numberpos+1);
                                        var fin = error.error.IndexOf('\'', numberpos+1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        _resolucionNumber.number = Int32.Parse(number);
                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                    }
                                    invoice.invoice.number = _resolucionNumber.number;
                                }
                                else
                                {
                                    throw new AlegraException(responseBody+ JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception ex)
                            {
                                throw new AlegraException(responseBody+ex.Message+ JsonConvert.SerializeObject(invoice));
                            }
                        }
                        if (responseBody.Contains("error"))
                        {

                            try
                            {
                                var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                                if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                {
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        _resolucionNumber.number = Int32.Parse(number);
                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                    }
                                    invoice.invoice.number = _resolucionNumber.number;
                                }
                                else
                                {
                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception ex)
                            {
                                return "error:"+responseBody + JsonConvert.SerializeObject(invoice);
                            }
                        }
                        else
                        {
                            Console.WriteLine(responseBody);
                            var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                            Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                            Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                            await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), invoice.invoice.number + 1);
                            return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe;
                        }
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

        public async Task<FacturaDataico> GetFacturaDataico(Modelo.Factura factura, Modelo.Tercero tercero, string estacion)
        {
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion);
            var numero = resolucion.numeroActual;

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.Contains("no informado"))
            {
                if(nombreCompleto.Split(' ').Count() > 1)
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
            return new FacturaDataico()
            {
                actions = new ActionsDataico() { send_dian = true, send_email = true },
                invoice = new InvoiceDataico()
                {
                    notes = new List<string>() { $"Placa: {factura.Placa}, Kilometraje : {factura.Kilometraje}, Nro Transaccion : NA" },

                    env = "PRODUCCION",
                    dataico_account_id = alegraOptions.DataicoAccountId,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = factura.DescripcionResolucion + factura.Consecutivo,
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = GetPaymentType(factura.FormaDePago),
                    payment_means_type = GetPaymentMeansType(factura.FormaDePago),
                    number = numero,
                    numbering = new NumberingDataico()
                    {
                        resolution_number = resolucion.resolucion,
                        flexible = true,
                        prefix = resolucion.prefijo
                    },
                    customer = new CustomerDataico()
                    {
                        email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.Contains("no informado") ? alegraOptions.Correo : tercero.Correo,
                        phone = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        address_line = string.IsNullOrEmpty(tercero.Direccion)?"0": tercero.Direccion,
                        country_code = "CO",
                        first_name = nombre,
                        family_name = apellido,
                        company_name = tercero.Nombre,
                        department = alegraOptions.Department ?? "73",
                        city = alegraOptions.City ?? "001",
                    },
                    items = new List<ItemDataico>(){
                        new ItemDataico()
                        {
                            sku=GetCodeCombustible(factura.Combustible),
                            price=(double)factura.Precio,
                            description=factura.Combustible,
                            quantity=(double)factura.Cantidad,
                            taxes = new List<TaxisDataico>(){ },
                            measuring_unit = "GL",
                            retentions = new List<RetentionDataico>(){ },
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
        private string GetKindOfPErson(string descripcionTipoIdentificacion)
        {
            switch (descripcionTipoIdentificacion)
            {
                case "Nit":
                    return "PERSONA_JURIDICA";
                default:
                    return "PERSONA_NATURAL";
            }
        }
        public async Task<FacturaDataico> GetFacturaDataico(Modelo.OrdenDeDespacho factura, Modelo.Tercero tercero, string estacion, ResolucionFacturaElectronica resolucion)
        {
            var numero = resolucion.numeroActual;

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.ToLower().Contains("no informado"))
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
            return new FacturaDataico()
            {
                actions = new ActionsDataico() { send_dian = true, send_email = true },
                invoice = new InvoiceDataico()
                {
                    notes = new List<string>() { $"Placa: {factura.Placa}, Kilometraje : {factura.Kilometraje}, Nro Transaccion : {factura.numeroTransaccion}" },

                    env = "PRODUCCION",
                    dataico_account_id = resolucion.idNumeracion,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = factura.IdVentaLocal.ToString(),
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = GetPaymentType(factura.FormaDePago),
                    payment_means_type = GetPaymentMeansType(factura.FormaDePago),
                    number = numero,
                    numbering = new NumberingDataico()
                    {
                        resolution_number = resolucion.resolucion,
                        flexible = true,
                        prefix = resolucion.prefijo
                    },
                    customer = new CustomerDataico()
                    {
                        email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower().Contains("no informado") ? alegraOptions.Correo : tercero.Correo,
                        phone = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        address_line = string.IsNullOrEmpty(tercero.Direccion) ? "0" : tercero.Direccion,
                        country_code = "CO",
                        first_name = nombre,
                        family_name = apellido,
                        company_name = tercero.Nombre,
                        department = alegraOptions.Department ?? "73",
                        city = alegraOptions.City ?? "001",
                    },
                    items = new List<ItemDataico>(){
                        new ItemDataico()
                        {
                            sku=GetCodeCombustible(factura.Combustible),
                            price=(double)factura.Precio,
                            description=factura.Combustible,
                            quantity=(double)factura.Cantidad,
                            taxes = new List<TaxisDataico>(){ },
                            measuring_unit = "GL",
                            retentions = new List<RetentionDataico>(){ },
                        }
                    }
                }
            };
        }

        public async Task<FacturaDataicoWithoutAddress> GetFacturaDataicoWithoutAddress(Modelo.OrdenDeDespacho factura, Modelo.Tercero tercero, string estacion, ResolucionFacturaElectronica resolucion)
        {
            var numero = resolucion.numeroActual;

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.ToLower().Contains("no informado"))
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
            return new FacturaDataicoWithoutAddress()
            {
                actions = new ActionsDataico() { send_dian = true, send_email = true },
                invoice = new InvoiceDataicoWithoutAddress()
                {
                    notes = new List<string>() { $"Placa: {factura.Placa}, Kilometraje : {factura.Kilometraje}, Nro Transaccion : {factura.numeroTransaccion}"},
                    env = "PRODUCCION",
                    dataico_account_id = resolucion.idNumeracion,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = factura.IdVentaLocal.ToString(),
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = GetPaymentType(factura.FormaDePago),
                    payment_means_type = GetPaymentMeansType(factura.FormaDePago),
                    number = numero,
                    numbering = new NumberingDataico()
                    {
                        resolution_number = resolucion.resolucion,
                        flexible = true,
                        prefix = resolucion.prefijo
                    },
                    customer = new CustomerDataicoWithoutAdress()
                    {
                        email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower().Contains("no informado") ? alegraOptions.Correo : tercero.Correo,
                        phone = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        first_name = nombre,
                        family_name = apellido,
                        company_name = tercero.Nombre,
                    },
                    
                    items = new List<ItemDataico>(){
                        new ItemDataico()
                        {
                            sku=GetCodeCombustible(factura.Combustible),
                            price=(double)factura.Precio,
                            description=factura.Combustible,
                            quantity=(double)factura.Cantidad,
                            taxes = new List<TaxisDataico>(){ },
                            measuring_unit = "GL",
                            retentions = new List<RetentionDataico>(){ },
                        }
                    }
                }
            };
        }
        private string GetCodeCombustible(string combustible)
        {
            if (combustible.ToLower().Contains("acpm") || combustible.ToLower().Contains("die"))
            {
                return alegraOptions.Acpm;
            }
            else if (combustible.ToLower().Contains("corri"))
            {
                return alegraOptions.Corriente;
            }
            else
            {
                return alegraOptions.Gas;
            }
        }

        private string GetPaymentType(string formaDePago)
        {
            if (formaDePago.ToLower().Contains("dé") && formaDePago.ToLower().Contains("tran"))
            {
                return "DEBIT_TRANSFER";
            }
            else if (formaDePago.ToLower().Contains("dé") && formaDePago.ToLower().Contains("tar"))
            {
                return "DEBIT_CARD";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("ban") && formaDePago.ToLower().Contains("tran"))
            {
                return "BANK_TRANSFER";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("tran"))
            {
                return "CREDIT_TRANSFER";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("tar"))
            {
                return "CREDIT_CARD";
            }
            else if (formaDePago.ToLower().Contains("ban") && formaDePago.ToLower().Contains("cons"))
            {
                return "DEBIT_BANK_TRANSFER";
            }
            else
            {
                return "CASH";
            }
        }


        private string GetPaymentMeansType(string formaDePago)
        {
            if (formaDePago.ToLower().Contains("dé") && formaDePago.ToLower().Contains("tran"))
            {
                return "DEBITO";
            }
            else if (formaDePago.ToLower().Contains("dé") && formaDePago.ToLower().Contains("tar"))
            {
                return "DEBITO";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("ban") && formaDePago.ToLower().Contains("tran"))
            {
                return "DEBITO";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("tran"))
            {
                return "CREDITO";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("tar"))
            {
                return "CREDITO";
            }
            else if (formaDePago.ToLower().Contains("ban") && formaDePago.ToLower().Contains("cons"))
            {
                return "DEBITO";
            }
            else
            {
                return "DEBITO";
            }
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {

            _semaphore.WaitOne();
            
            try
            {
                if (alegraOptions.ExcluirDireccion)
                {

                    Console.WriteLine(estacionGuid.ToString());
                    var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
                    var invoice = await GetFacturaDataicoWithoutAddress(orden, tercero, estacionGuid.ToString(), resolucion);
                    Console.WriteLine(JsonConvert.SerializeObject(invoice));
                    while (true)
                    {

                        using (var client = new HttpClient())
                        {
                            client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                            client.DefaultRequestHeaders.Add("auth-token", resolucion.token);
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
                                    if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                    {
                                        var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                        if (error.error.Contains("Tiene que ser el siguiente"))
                                        {
                                            var numberpos = error.error.IndexOf('\'');
                                            numberpos = error.error.IndexOf('\'', numberpos + 1);
                                            numberpos = error.error.IndexOf('\'', numberpos + 1);
                                            var fin = error.error.IndexOf('\'', numberpos + 1);
                                            var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                            resolucion.numeroActual = Int32.Parse(number);
                                            invoice.invoice.number = resolucion.numeroActual;
                                        }
                                        else if (error.error.Contains("modificar"))
                                        {

                                            resolucion.numeroActual++;
                                            invoice.invoice.number = resolucion.numeroActual;
                                        }
                                        else
                                        {
                                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                        }
                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                    }
                                }
                                catch (Exception ex)
                                {

                                    return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                                }
                            }
                            if (responseBody.Contains("error"))
                            {

                                try
                                {
                                    var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                                    if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                    {
                                        var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                        if (error.error.Contains("Tiene que ser el siguiente"))
                                        {
                                            var numberpos = error.error.IndexOf('\'');
                                            numberpos = error.error.IndexOf('\'', numberpos + 1);
                                            numberpos = error.error.IndexOf('\'', numberpos + 1);
                                            var fin = error.error.IndexOf('\'', numberpos + 1);
                                            var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                            resolucion.numeroActual = Int32.Parse(number);
                                        }
                                        else if (error.error.Contains("modificar"))
                                        {

                                            resolucion.numeroActual++;
                                        }
                                        else
                                        {
                                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                        }
                                        invoice.invoice.number = resolucion.numeroActual;
                                    }
                                    else
                                    {
                                        Console.WriteLine(responseBody + JsonConvert.SerializeObject(invoice));
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine(ex.StackTrace);
                                    return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                                }
                            }
                            else
                            {
                                var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                                Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                                Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                                await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), invoice.invoice.number + 1);
                                return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe;
                            }
                        }
                    }
                }
                else
                {

                    Console.WriteLine(estacionGuid.ToString());
                    var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
                    var invoice = await GetFacturaDataico(orden, tercero, estacionGuid.ToString(), resolucion);
                    Console.WriteLine(JsonConvert.SerializeObject(invoice));
                    while (true)
                    {

                        using (var client = new HttpClient())
                        {
                            client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                            client.DefaultRequestHeaders.Add("auth-token", resolucion.token);
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
                                    if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                    {
                                        var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                        if (error.error.Contains("Tiene que ser el siguiente"))
                                        {
                                            var numberpos = error.error.IndexOf('\'');
                                            numberpos = error.error.IndexOf('\'', numberpos + 1);
                                            numberpos = error.error.IndexOf('\'', numberpos + 1);
                                            var fin = error.error.IndexOf('\'', numberpos + 1);
                                            var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                            resolucion.numeroActual = Int32.Parse(number);
                                            invoice.invoice.number = resolucion.numeroActual;
                                        }
                                        else if (error.error.Contains("modificar"))
                                        {

                                            resolucion.numeroActual++;
                                            invoice.invoice.number = resolucion.numeroActual;
                                        }
                                        else if (error.error.ToLower().Contains("ciudad"))
                                        {

                                            invoice.invoice.customer.city = null;
                                            invoice.invoice.customer.department = null;
                                            invoice.invoice.customer.address_line = null;
                                            invoice.invoice.customer.country_code = null;
                                        }
                                        else
                                        {
                                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                        }
                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                    }
                                }
                                catch (Exception ex)
                                {

                                    return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                                }
                            }
                            if (responseBody.Contains("error"))
                            {

                                try
                                {
                                    var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                                    if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                    {
                                        var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                        if (error.error.Contains("Tiene que ser el siguiente"))
                                        {
                                            var numberpos = error.error.IndexOf('\'');
                                            numberpos = error.error.IndexOf('\'', numberpos + 1);
                                            numberpos = error.error.IndexOf('\'', numberpos + 1);
                                            var fin = error.error.IndexOf('\'', numberpos + 1);
                                            var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                            resolucion.numeroActual = Int32.Parse(number);
                                        }
                                        else if (error.error.Contains("modificar"))
                                        {

                                            resolucion.numeroActual++;
                                        }
                                        else if (error.error.ToLower().Contains("ciudad"))
                                        {

                                            invoice.invoice.customer.city = null;
                                            invoice.invoice.customer.department = null;
                                            invoice.invoice.customer.address_line = null;
                                            invoice.invoice.customer.country_code = null;
                                        }
                                        else
                                        {
                                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                        }
                                        invoice.invoice.number = resolucion.numeroActual;
                                    }
                                    else
                                    {
                                        Console.WriteLine(responseBody + JsonConvert.SerializeObject(invoice));
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine(ex.StackTrace);
                                    return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                                }
                            }
                            else
                            {
                                var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                                Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                                Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                                await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), invoice.invoice.number + 1);
                                return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new AlegraException(ex.Message);
            }
            finally
            {
                _semaphore.Release();
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
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion);
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                client.DefaultRequestHeaders.Add("auth-token", resolucion.token);
                var path = $"{alegraOptions.Url}numberings/invoice";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    return new ResolucionElectronica(new Numbering() { prefix=resolucion.prefijo, dian_resolutions = new List<DianResolution>() { new DianResolution { number = resolucion.resolucion } } });
                }

                var respuesta = JsonConvert.DeserializeObject<ResolucionesDataico>(responseBody);
                Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                return new ResolucionElectronica(respuesta.numberings.First(x=>x.prefix == resolucion.prefijo));
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

        public async Task<string> getJson(Modelo.OrdenDeDespacho orden, Guid estacion)
        {
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion.ToString());

            var factura = await GetFacturaDataico(orden, orden.Tercero, estacion.ToString(), resolucion);

            Console.WriteLine(JsonConvert.SerializeObject(factura));
            return JsonConvert.SerializeObject(factura);

        }

        public async Task<string> GenerarFacturaElectronica(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            try
            {
                Console.WriteLine(estacionGuid.ToString());
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
                var invoice = await GetFacturaDataico(factura, tercero, estacionGuid.ToString(), resolucion);
                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                while (true)
                {

                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                        client.DefaultRequestHeaders.Add("auth-token", resolucion.token);
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
                                if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                {
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        resolucion.numeroActual = Int32.Parse(number);
                                        invoice.invoice.number = resolucion.numeroActual;
                                    }
                                    else if (error.error.Contains("modificar"))
                                    {

                                        resolucion.numeroActual++;
                                        invoice.invoice.number = resolucion.numeroActual;
                                    }
                                    else if (error.error.ToLower().Contains("ciudad"))
                                    {

                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                    }
                                }
                                else
                                {
                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception ex)
                            {

                                return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                            }
                        }
                        if (responseBody.Contains("error"))
                        {

                            try
                            {
                                var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                                if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                {
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        resolucion.numeroActual = Int32.Parse(number);
                                    }
                                    else if (error.error.Contains("modificar"))
                                    {

                                        resolucion.numeroActual++;
                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                    }
                                    invoice.invoice.number = resolucion.numeroActual;
                                }
                                else
                                {
                                    Console.WriteLine(responseBody + JsonConvert.SerializeObject(invoice));
                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.StackTrace);
                                return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                            }
                        }
                        else
                        {
                            var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                            Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                            Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                            await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), invoice.invoice.number + 1);
                            return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new AlegraException(ex.Message);
            }
        }

        


        private async Task<FacturaDataicoWithoutAddress> GetFacturaDataico(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, string v, ResolucionFacturaElectronica resolucion)
        {
            var numero = resolucion.numeroActual;

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.ToLower().Contains("no informado"))
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
            var items = new List<ItemDataico>();
            foreach (var articulo in factura.canastillas)
            {
                var taxes = new List<TaxisDataico>();
                if( articulo.iva>0)
                {
                    taxes.Add(new TaxisDataico() { tax_rate="19"});
                }
                var item = new ItemDataico()
                {
                    sku = "C"+articulo.Canastilla.CanastillaId.ToString(),
                    price = (double)articulo.precio,
                    description = articulo.Canastilla.descripcion,
                    quantity = (double)articulo.cantidad,
                    taxes = taxes,
                    measuring_unit = "GL",
                    retentions = new List<RetentionDataico>() { },
                };
                items.Add(item);
            }
                return new FacturaDataicoWithoutAddress()
            {
                actions = new ActionsDataico() { send_dian = true, send_email = true },
                invoice = new InvoiceDataicoWithoutAddress()
                {
                    notes = new List<string>() { $"Placa: , Kilometraje : , Nro Transaccion : " },
                    env = "PRODUCCION",
                    dataico_account_id = resolucion.idNumeracion,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = factura.FacturasCanastillaId.ToString(),
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = GetPaymentType(factura.codigoFormaPago.Descripcion),
                    payment_means_type = GetPaymentMeansType(factura.codigoFormaPago.Descripcion),
                    number = numero,
                    numbering = new NumberingDataico()
                    {
                        resolution_number = resolucion.resolucion,
                        flexible = true,
                        prefix = resolucion.prefijo
                    },
                    customer = new CustomerDataicoWithoutAdress()
                    {
                        email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower().Contains("no informado") ? alegraOptions.Correo : tercero.Correo,
                        phone = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        first_name = nombre,
                        family_name = apellido,
                        company_name = tercero.Nombre,
                    },

                    items =items
                }
            };
        }

        private float GetTasa(Modelo.CanastillaFactura articulo)
        {
            return articulo.iva > 0 ? 19 : 0;
        }



        public Task<Item> GetItem(string name, Alegra options)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFacturaElectronica(string id, Guid estacionGuid)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReenviarFactura(Repositorio.Entities.OrdenDeDespacho orden, Guid estacion)
        {
            throw new NotImplementedException();
        }
    }
}
