using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Numerics;
using FacturacionelectronicaCore.Repositorio.Entities;
using Amazon.Runtime.Internal;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturacionSilog : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionesHandler resolucionesHandler;
        private readonly ResolucionNumber _resolucionNumber;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private readonly IEmpleadoRepositorio _empleadoRepositorio;

        public FacturacionSilog(IOptions<Alegra> alegra, ResolucionNumber resolucionNumber, IResolucionRepositorio resolucionRepositorio, IEmpleadoRepositorio empleadoRepositorio)
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

                var invoice = await GetFacturaSilog(factura, tercero, estacionGuid.ToString());
                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                var str = regex.Replace(factura.Tercero.Nombre, @" ");
                Console.WriteLine(factura.Vendedor);
                var cedula = await _empleadoRepositorio.GetEmpleadoByName(factura.Vendedor.Trim());
                RequestContabilidad request = new RequestContabilidad(invoice, cedula?.Trim(), alegraOptions, estacionGuid.ToString());

                Console.WriteLine(JsonConvert.SerializeObject(request));
                using (var client = new HttpClient())
                {
                    try
                    {

                        MultipartFormDataContent form = new MultipartFormDataContent();

                        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                        var facturaBase64 = System.Convert.ToBase64String(plainTextBytes);
                        form.Add(new StringContent("generar_factura"), "function_name");
                        form.Add(new StringContent(facturaBase64), "parameter");
                        var response = await client.PostAsync(alegraOptions.Url, form);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        var respuestaSilog = JsonConvert.DeserializeObject<REspuestaSilog>(responseBody);
                        if (responseBody.ToLower().Contains("error"))
                        {
                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(request));

                        }
                        else
                        {
                            return "Ok:" + respuestaSilog.datos_factura.FirstOrDefault()?.arr_facturas.FirstOrDefault()?.prefijo_dian + respuestaSilog.datos_factura.FirstOrDefault()?.arr_facturas.FirstOrDefault()?.nro_dian + ":" + respuestaSilog.uuid_cufe;

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

        public async Task<FacturaSilog> GetFacturaSilog(Modelo.Factura x, Modelo.Tercero tercero, string estacionGuid)
        {
            //var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());

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

            var subtotal = x.SubTotal + x.Descuento;
            var total = subtotal;
            return new FacturaSilog()
            {
                Guid = Guid.Parse(estacionGuid),
                Consecutivo = x.Consecutivo,
                Combustible = x.Combustible,
                Cantidad = x.Cantidad,
                Precio = x.Precio,
                Total = total,
                Placa = x.Placa,
                Kilometraje = x.Kilometraje,
                Surtidor = x.Surtidor + "",
                Cara = x.Cara + "",
                Manguera = x.Manguera + "",
                FormaDePago = x.FormaDePago,
                Fecha = x.Fecha,
                Tercero = new TerceroSilog()
                {
                    Nombre = string.IsNullOrEmpty(tercero.Nombre) ? "No informado" : tercero.Nombre,
                    Direccion = string.IsNullOrEmpty(tercero.Direccion) ? "No informado" : tercero.Direccion,
                    Telefono = string.IsNullOrEmpty(tercero.Telefono) ? "No informado" : tercero.Telefono,
                    Correo = string.IsNullOrEmpty(tercero.Correo) ? "No informado" : tercero.Correo,
                    DescripcionTipoIdentificacion = string.IsNullOrEmpty(tercero.DescripcionTipoIdentificacion) ? "No especificada" : tercero.DescripcionTipoIdentificacion,
                    Identificacion = string.IsNullOrEmpty(tercero.Identificacion) ? "No informado" : tercero.Identificacion,
                    IdLocal = tercero.IdLocal
                },
                Descuento = x.Descuento,
                IdLocal = x.IdLocal,
                IdVentaLocal = x.IdVentaLocal,
                IdTerceroLocal = x.Tercero.IdLocal,
                FechaProximoMantenimiento = x.FechaProximoMantenimiento,
                SubTotal = subtotal,
                Vendedor = x.Vendedor,
                Identificacion = x.Tercero.Identificacion,
                Prefijo = x.DescripcionResolucion + x.Consecutivo,
                Cedula = tercero.Identificacion,
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
        public async Task<FacturaSilog> GetFacturaSilog(Modelo.OrdenDeDespacho x, Modelo.Tercero tercero, string estacionGuid)
        {
            // var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
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

            var subtotal = x.SubTotal + x.Descuento;
            var total = subtotal ;
            return new FacturaSilog()
            {
                Guid = Guid.Parse(estacionGuid),
                Consecutivo = x.IdVentaLocal,
                Combustible = x.Combustible,
                Cantidad = (decimal)x.Cantidad,
                Precio = (decimal)x.Precio,
                Total = (decimal)total,
                Placa = x.Placa,
                Kilometraje = x.Kilometraje,
                Surtidor = x.Surtidor + "",
                Cara = x.Cara + "",
                Manguera = x.Manguera + "",
                FormaDePago = x.FormaDePago,
                Fecha = x.Fecha,
                numeroTransaccion = x.numeroTransaccion,
                Tercero = new TerceroSilog()
                {
                    Nombre = string.IsNullOrEmpty(tercero.Nombre) ? "No informado" : tercero.Nombre,
                    Direccion = string.IsNullOrEmpty(tercero.Direccion) ? "No informado" : tercero.Direccion,
                    Telefono = string.IsNullOrEmpty(tercero.Telefono) ? "No informado" : tercero.Telefono,
                    Correo = string.IsNullOrEmpty(tercero.Correo) ? "No informado" : tercero.Correo,
                    DescripcionTipoIdentificacion = string.IsNullOrEmpty(tercero.DescripcionTipoIdentificacion) ? "No especificada" : tercero.DescripcionTipoIdentificacion,
                    Identificacion = string.IsNullOrEmpty(tercero.Identificacion) ? "No informado" : tercero.Identificacion,
                    IdLocal = tercero.IdLocal
                },
                Descuento = x.Descuento,
                IdLocal = x.IdLocal,
                IdVentaLocal = x.IdVentaLocal,
                IdTerceroLocal = x.Tercero.IdLocal,
                FechaProximoMantenimiento = x.FechaProximoMantenimiento,
                SubTotal = (decimal)subtotal,
                Vendedor = x.Vendedor,
                Identificacion = x.Tercero.Identificacion,
                Prefijo = alegraOptions.Prefix,
                Cedula = tercero.Identificacion,
            };
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {

            try
            {

                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
                var invoice = await GetFacturaSilog(orden, tercero, estacionGuid.ToString());

                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                var str = regex.Replace(orden.Tercero.Nombre, @" ");
                Console.WriteLine(orden.Vendedor);
                var cedula = await _empleadoRepositorio.GetEmpleadoByName(orden.Vendedor.Trim());
                RequestContabilidad request = new RequestContabilidad(invoice, cedula?.Trim(), alegraOptions, estacionGuid.ToString());
                if (resolucion != null && !string.IsNullOrEmpty(resolucion.token))
                {

                    invoice = await GetFacturaSilog(orden, tercero, resolucion.token);

                    request = new RequestContabilidad(invoice, cedula?.Trim(), alegraOptions, resolucion.token);
                }


                using (var client = new HttpClient())
                {
                    try
                    {

                        MultipartFormDataContent form = new MultipartFormDataContent();

                        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                        var facturaBase64 = System.Convert.ToBase64String(plainTextBytes);
                        form.Add(new StringContent("generar_factura"), "function_name");
                        form.Add(new StringContent(facturaBase64), "parameter");
                        var response = await client.PostAsync(alegraOptions.Url, form);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        var respuestaSilog = JsonConvert.DeserializeObject<REspuestaSilog>(responseBody);
                        if (responseBody.ToLower().Contains("error"))
                        {
                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(request));

                        }
                        else
                        {
                            return "Ok:" + respuestaSilog.datos_factura.FirstOrDefault()?.arr_facturas.FirstOrDefault()?.prefijo_dian + respuestaSilog.datos_factura.FirstOrDefault()?.arr_facturas.FirstOrDefault()?.nro_dian + ":" + respuestaSilog.uuid_cufe;

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
            try
            {
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion);
                return new ResolucionElectronica
                {
                    invoiceText = $"Resolución electronica {resolucion.prefijo} - {resolucion.resolucion} desde 1 hasta 1000000. ",
                    prefix = resolucion.prefijo

                };
            }
            catch (Exception ex)
            {
                return new ResolucionElectronica
                {
                    invoiceText = $"Resolución electronica - {alegraOptions.ResolutionNumber} desde 1 hasta 1000000. ",
                    prefix = alegraOptions.Prefix

                };
            }


        }

        public async Task<string> getJson(Modelo.OrdenDeDespacho orden, Guid estacion)
        {
            var factura = await GetFacturaSilog(orden, orden.Tercero, estacion.ToString());

            Console.WriteLine(JsonConvert.SerializeObject(factura));
            Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
            var str = regex.Replace(orden.Tercero.Nombre, @" ");
            Console.WriteLine(orden.Vendedor);
            var cedula = await _empleadoRepositorio.GetEmpleadoByName(orden.Vendedor.Trim());
            return JsonConvert.SerializeObject(new RequestContabilidad(factura, cedula?.Trim(), alegraOptions, estacion.ToString()));

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
            try
            {
                var silogRequest = GetSilogRequest(factura, tercero, estacionGuid.ToString());
                Console.WriteLine(JsonConvert.SerializeObject(silogRequest));

                using (var client = new HttpClient())
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(silogRequest);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        
                        var response = await client.PostAsync(alegraOptions.Url, content);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        
                        if (responseBody.ToLower().Contains("error"))
                        {
                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(silogRequest));
                        }
                        else
                        {
                            // Parsear la respuesta según la estructura que retorne Silog
                            return "Ok:" + responseBody;
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

        public SilogRequest GetSilogRequest(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, string estacionGuid)
        {
            var productList = new List<ProductInformation>();
            
            // Agregar los productos de canastilla
            if (factura.canastillas != null)
            {
                foreach (var item in factura.canastillas)
                {
                    productList.Add(new ProductInformation
                    {
                        ProductId = Int32.Parse(item.Canastilla?.campoextra),
                        Quantity = (decimal)item.cantidad,
                        Discunt = 0 // Ajustar según sea necesario
                    });
                }
            }

            return new SilogRequest
            {
                UserInformation = new UserInformation
                {
                    SucursalId = 1,
                    UserIdent = alegraOptions.Usuario, // Necesitarás configurar estos valores
                    UserPassword = alegraOptions.Token // Asegúrate de que estos valores estén configurados correctamente
                },
                StationInformation = new StationInformation
                {
                    Dispenser = "1",
                    Island = "1", 
                    Hose = "1"
                },
                InvoiceInformation = new InvoiceInformation
                {
                    PosConsecutive = factura.consecutivo,
                    InvoiceDate = factura.fecha.ToString("yyyy-MM-dd"),
                    Details = $"Factura Canastilla: {factura.consecutivo}",
                    VehicleInformation = new VehicleInformation
                    {
                        Plate = "N/A",
                        LastMaintenanceDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        NextMaintenanceDate = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd"),
                        Mileage = "0.00"
                    },
                    InvoiceHolderInformation = new InvoiceHolderInformation
                    {
                        TypeId = GetTipoIdentificacionId(tercero.DescripcionTipoIdentificacion),
                        Id = tercero.Identificacion ?? "No informado",
                        Name = tercero.Nombre ?? "No informado",
                        FirstLastName = tercero.Apellidos ?? "",
                        SecondLastName = "",
                        Adress = tercero.Direccion ?? "No informado",
                        Email = tercero.Correo ?? "No informado",
                        PhoneNumber = tercero.Telefono ?? "No informado"
                    },
                    ProductInformation = productList,
                    PaymentInformation = new PaymentInformation
                    {
                        PaymentFormId = GetPaymentFormId(factura.codigoFormaPago?.Descripcion),
                        PaymentMethodId = 1,
                        PaymentMeandId = 1,
                        CardId = 1,
                        TransaccionNumber = factura.consecutivo
                    }
                }
            };
        }

        private int GetTipoIdentificacionId(string descripcionTipoIdentificacion)
        {
            switch (descripcionTipoIdentificacion?.ToLower())
            {
                case "nit":
                    return 6;
                case "cedula":
                case "cc":
                    return 1;
                case "cedula extranjeria":
                case "ce":
                    return 2;
                default:
                    return 1; // Por defecto cedula
            }
        }

        private int GetPaymentFormId(string formaDePago)
        {
            switch (formaDePago?.ToLower())
            {
                case "efectivo":
                    return 1;
                case "tarjeta":
                case "credito":
                    return 2;
                default:
                    return 1; // Por defecto efectivo
            }
        }

        public Task<string> GetFacturaElectronica(string id, Guid estacionGuid)
        {
            // Implementar según necesidades específicas de Silog para obtener factura por ID y estación
            return Task.FromResult($"Factura {id} para estacion {estacionGuid}");
        }

        public Task<Item> GetItem(string name, Alegra options)
        {
            // Implementar según necesidades específicas de Silog para obtener items
            return Task.FromResult(new Item { name = name });
        }

        public Task<string> ReenviarFactura(Repositorio.Entities.OrdenDeDespacho orden, Guid estacion)
        {
            // Implementar según necesidades específicas de Silog para reenviar facturas
            return Task.FromResult("Factura reenviada");
        }
    }
}