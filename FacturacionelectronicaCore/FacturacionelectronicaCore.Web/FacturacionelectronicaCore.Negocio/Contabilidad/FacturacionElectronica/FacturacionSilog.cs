using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
                var cedula = await _empleadoRepositorio.GetEmpleadoByName(factura.Vendedor.Trim());

                var request = new RequestContabilidad(invoice, cedula?.Trim(), alegraOptions, estacionGuid.ToString());

                Console.WriteLine(JsonConvert.SerializeObject(request));

                using (var client = new HttpClient())
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(request);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(alegraOptions.Url + "output", content);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        var respuestaSilog = JsonConvert.DeserializeObject<RespuestaSilog>(responseBody);

                        if (responseBody.ToLower().Contains("error"))
                        {
                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(request));
                        }
                        else
                        {
                            return "Ok:" + respuestaSilog.message.currentOutput.prefijoResolucion + respuestaSilog.message.currentOutput.numeroResolucion + ":" + respuestaSilog.cufe+ ":" + responseBody;
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

        public Task<FacturaSilog> GetFacturaSilog(Modelo.Factura x, Modelo.Tercero tercero, string estacionGuid)
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
            var result = new FacturaSilog()
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

            return Task.FromResult(result);
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

        private string GetCombustible(string combustible, string estacion)
        {
            if (combustible.ToLower().Contains("corriente"))
            {
                return alegraOptions.Estaciones?.ContainsKey(estacion) == true ?
                     alegraOptions.Estaciones[estacion].Corriente ?? alegraOptions.Corriente :
                     alegraOptions.Corriente;
            }
            else if (combustible.ToLower().Contains("diesel") || combustible.ToLower().Contains("bioacem") || combustible.ToLower().Contains("acpm"))
            {
                return alegraOptions.Estaciones?.ContainsKey(estacion) == true ?
                    alegraOptions.Estaciones[estacion].Acpm ?? alegraOptions.Acpm :
                    alegraOptions.Acpm;
            }
            else if (combustible.ToLower().Contains("extra"))
            {
                return alegraOptions.Estaciones?.ContainsKey(estacion) == true ?
                   alegraOptions.Estaciones[estacion].Extra ?? alegraOptions.Extra :
                   alegraOptions.Extra;
            }
            else
            {
                return alegraOptions.Estaciones?.ContainsKey(estacion) == true ?
                    alegraOptions.Estaciones[estacion].Gas ?? alegraOptions.Gas :
                    alegraOptions.Gas;
            }
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {
            if (orden.FormaDePago.ToLower().Contains("cali")
           || orden.FormaDePago.ToLower().Contains("puntos")
            || orden.FormaDePago.ToLower().Contains("asumidos")
            )
            {
                return null;
            }
            try
            {
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
                var silogRequest = await GetSilogRequest(orden, tercero, resolucion, estacionGuid.ToString());


                Console.WriteLine(JsonConvert.SerializeObject(silogRequest));
                var responseBody = "";
                using (var client = new HttpClient())
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(silogRequest);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(alegraOptions.Url + "output", content);

                        responseBody = await response.Content.ReadAsStringAsync();
                        var responsesilog = JsonConvert.DeserializeObject<RespuestaSilog>(responseBody);
                        // Verificar si la respuesta contiene "Factura generada exitosamente"
                        if (responseBody.Contains("Factura generada exitosamente"))
                        {
                            return "Ok:" + responsesilog.message.currentOutput.prefijoResolucion + responsesilog.message.currentOutput.numeroResolucion + ":" + responsesilog.cufe+ ":" + responseBody;
                        }

                        response.EnsureSuccessStatusCode();

                        if (responseBody.ToLower().Contains("error"))
                        {
                            return "Error:" + responseBody + ":" + JsonConvert.SerializeObject(silogRequest);
                        }
                        else
                        {
                            // Parsear la respuesta según la estructura que retorne Silog
                            return "Ok:" + responsesilog.message.currentOutput.prefijoResolucion + responsesilog.message.currentOutput.numeroResolucion + ":" + responsesilog.cufe+ ":" + responseBody;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Verificar si la respuesta contiene "Factura generada exitosamente" incluso en caso de excepción
                        if (responseBody.Contains("Factura generada exitosamente"))
                        {
                            return "Ok:" + responseBody;
                        }

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        return "Error:" + responseBody + ":" + JsonConvert.SerializeObject(silogRequest) + ":" + ex.Message;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                return "Error:" + ex.Message;
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
                    invoiceText = $"Resolución Dian {alegraOptions.Prefix} {alegraOptions.ResolutionNumber} de {alegraOptions.Desde} hasta {alegraOptions.Hasta} - vigencia del {alegraOptions.DesdeFecha} al {alegraOptions.HastaFecha}",
                    prefix = resolucion.prefijo

                };
            }
            catch
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
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion.ToString());
            var silogRequest = await GetSilogRequest(orden, orden.Tercero, resolucion, estacion.ToString());
            return JsonConvert.SerializeObject(silogRequest);
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
            if (factura.codigoFormaPago.Descripcion.ToLower().Contains("cali")
           || factura.codigoFormaPago.Descripcion.ToLower().Contains("puntos")
            || factura.codigoFormaPago.Descripcion.ToLower().Contains("asumidos")
            )
            {
                return null;
            }
            try
            {
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());

                var silogRequest = GetSilogRequest(factura, tercero, resolucion);
                Console.WriteLine(JsonConvert.SerializeObject(silogRequest));
                var responseBody = "";
                using (var client = new HttpClient())
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(silogRequest);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(alegraOptions.Url + "output", content);
                        responseBody = await response.Content.ReadAsStringAsync();
                        var responsesilog = JsonConvert.DeserializeObject<RespuestaSilog>(responseBody);
                        // Verificar si la respuesta contiene "Factura generada exitosamente"
                        if (responseBody.Contains("Factura generada exitosamente"))
                        {
                            return "Ok:" + responsesilog.message.currentOutput.prefijoResolucion + responsesilog.message.currentOutput.numeroResolucion + ":" + responsesilog.cufe + ":" + responseBody;
                        }

                        response.EnsureSuccessStatusCode();

                        if (responseBody.ToLower().Contains("error"))
                        {
                            return "Error:" + responseBody + ":" + JsonConvert.SerializeObject(silogRequest);
                        }
                        else
                        {
                            // Parsear la respuesta según la estructura que retorne Silog
                            return "Ok:" + responsesilog.message.currentOutput.prefijoResolucion + responsesilog.message.currentOutput.numeroResolucion + ":" + responsesilog.cufe + ":" + responseBody;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Verificar si la respuesta contiene "Factura generada exitosamente" incluso en caso de excepción
                        if (responseBody.Contains("Factura generada exitosamente"))
                        {
                            return "Ok:" + responseBody;
                        }

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        return "Error:" + responseBody + ":" + JsonConvert.SerializeObject(silogRequest) + ":" + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                return "Error:" + ex.Message;
                   
            }
        }

        public SilogRequest GetSilogRequest(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, ResolucionFacturaElectronica resolucion)
        {
            var productList = new List<ProductInformation>();

            // Agregar los productos de canastilla con IVA si aplica
            if (factura.canastillas != null)
            {
                foreach (var item in factura.canastillas)
                {
                    var product = new ProductInformation
                    {
                        ProductId = Int32.Parse(item.Canastilla?.campoextra ?? "0"),
                        Quantity = (decimal)item.cantidad,
                        Discunt = 0,
                        Price = (decimal)item.precio,
                        TotalPrice = (decimal)item.total,
                        SkipAuditWarehouseValues = true
                    };
                    // Si hay IVA, agregarlo como campo adicional (puedes crear un campo IVA en ProductInformation si lo necesitas)
                    if (item.iva > 0)
                    {
                        // Si ProductInformation tiene campo IVA, descomenta la siguiente línea:
                        // product.IVA = (decimal)item.iva;
                        // Si no, puedes agregarlo como un campo extra en el diccionario o en la request final
                    }
                    productList.Add(product);
                }
            }

            // FacturaCanastilla no tiene campo vendedor, usar valores por defecto
            return new SilogRequest
            {
                UserInformation = new UserInformation
                {
                    SucursalId = Int32.Parse(resolucion.idNumeracion),
                    UserIdent = alegraOptions.Usuario,
                    UserPassword = alegraOptions.Token
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
                        FirstLastName = "",
                        SecondLastName = "",
                        Adress = tercero.Direccion ?? "No informado",
                        Email = tercero.Correo ?? alegraOptions.Correo,
                        PhoneNumber = GetValidPhoneNumber(tercero.Telefono)
                    },
                    ProductInformation = productList,
                    PaymentInformation = new PaymentInformation
                    {
                        PaymentFormId = GetPaymentFormId(factura.codigoFormaPago?.Descripcion,resolucion.idNumeracion),
                        PaymentMethodId = 1,
                        PaymentMeanId = 1,
                        CardId = 1,
                        TransaccionNumber = factura.consecutivo
                    }
                }
            };
        }

        public async Task<SilogRequest> GetSilogRequest(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, ResolucionFacturaElectronica resolucion, string estacion)
        {
            var productList = new List<ProductInformation>
            {
                // Agregar el combustible como producto
                new ProductInformation
                {
                    ProductId = int.Parse(GetCombustible(orden.Combustible, estacion)), // ID del combustible - ajustar según sea necesario
                    Quantity = (decimal)orden.Cantidad,
                    Discunt = orden.Descuento,
                    Price = (decimal)orden.Precio,
                    TotalPrice = (decimal)orden.Total,
                    SkipAuditWarehouseValues = true
                }
            };

            // Determinar si VehicleInformation debe ser null
            VehicleInformation vehicleInfo = null;
            if (!string.IsNullOrEmpty(orden.Placa) && tercero.Identificacion != "222222222222")
            {
                vehicleInfo = new VehicleInformation
                {
                    Plate = orden.Placa ?? "N/A",
                    LastMaintenanceDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    NextMaintenanceDate = orden.FechaProximoMantenimiento.ToString("yyyy-MM-dd"),
                    Mileage = orden.Kilometraje?.ToString() ?? "0.00"
                };
            }

            // Buscar información del empleado
            string empleadoIdentificacion = null;
            if (!alegraOptions.AutenticaPorDefecto && !string.IsNullOrEmpty(orden.Vendedor))
            {
                empleadoIdentificacion = await _empleadoRepositorio.GetEmpleadoByName(orden.Vendedor.Trim());
            }

            return new SilogRequest
            {
                UserInformation = new UserInformation
                {
                    SucursalId = Int32.Parse(resolucion.idNumeracion),
                    UserIdent = !string.IsNullOrEmpty(empleadoIdentificacion) ? empleadoIdentificacion : alegraOptions.Usuario,
                    UserPassword = !string.IsNullOrEmpty(empleadoIdentificacion) ? empleadoIdentificacion : alegraOptions.Token
                },
                StationInformation = new StationInformation
                {
                    Dispenser = orden.Surtidor?.ToString() ?? "1",
                    Island = "1",
                    Hose = orden.Cara?.ToString() ?? "1"
                },
                InvoiceInformation = new InvoiceInformation
                {
                    PosConsecutive = orden.IdVentaLocal,
                    InvoiceDate = orden.Fecha.ToString("yyyy-MM-dd"),
                    Details = $"Orden de Despacho: {orden.Combustible.Trim()} - {orden.Cantidad}GL - {orden.IdVentaLocal} - Transaccion {orden.numeroTransaccion}",
                    VehicleInformation = vehicleInfo,
                    InvoiceHolderInformation = new InvoiceHolderInformation
                    {
                        TypeId = GetTipoIdentificacionId(tercero.DescripcionTipoIdentificacion),
                        Id = tercero.Identificacion ?? "No informado",
                        Name = tercero.Nombre ?? "No informado",
                        FirstLastName = "",
                        SecondLastName = "",
                        Adress = tercero.Direccion ?? "No informado",
                        Email = tercero.Correo ?? alegraOptions.Correo,
                        PhoneNumber = GetValidPhoneNumber(tercero.Telefono)
                    },
                    ProductInformation = productList,
                    PaymentInformation = new PaymentInformation
                    {
                        PaymentFormId = GetPaymentFormId(orden.FormaDePago, resolucion.idNumeracion),
                        PaymentMethodId = GetMethodFormId(orden.FormaDePago, resolucion.idNumeracion),
                        PaymentMeanId = GetMeandFormId(orden.FormaDePago, resolucion.idNumeracion),
                        CardId = 1,
                        TransaccionNumber = string.IsNullOrEmpty(orden.numeroTransaccion) ? orden.IdVentaLocal : int.Parse(orden.numeroTransaccion)
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
                case "cédula ciudadanía":
                case "cc":
                    return 3;
                case "cedula extranjeria":
                case "ce":
                    return 5;
                default:
                    return 3; // Por defecto cedula
            }
        }

        private int GetPaymentFormId(string formaDePago, string sucursal)
        {
            if (sucursal == "603")
            {
                if (formaDePago.ToLower().Contains("directo"))
                {
                    return 3;
                }
                else
                {
                    return 1;
                }
            }
            if (formaDePago.ToLower().Contains("dito") || formaDePago.ToLower().Contains("cred") || formaDePago.ToLower().Contains("urbano")
            || formaDePago.ToLower().Contains("vip")
            || formaDePago.ToLower().Contains("doble")
            || formaDePago.ToLower().Contains("micro"))
            {
                return 3;
            }
            else if (formaDePago.ToLower().Contains("tarjeta"))
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
        private int GetMethodFormId(string formaDePago, string sucursal)
        {
            if (sucursal == "603")
            {

                if (formaDePago.ToLower().Contains("directo"))
                {
                    return 3;
                }
                else if (formaDePago.ToLower().Contains("efe") )
                {
                    return 1;
                }
                else if (formaDePago.ToLower().Contains("tarjeta"))
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            if (formaDePago.ToLower().Contains("dito"))
            {
                return 3;
            }
            else if (formaDePago.ToLower().Contains("efe") || formaDePago.ToLower().Contains("dat"))
            {
                return 1;
            }
            else if (formaDePago.ToLower().Contains("cons"))
            {
                return 4;
            }
            else
            {
                return 3;
            }
        }
        private int GetMeandFormId(string formaDePago, string sucursal)
        {
            if(sucursal == "603")
            {
                if (formaDePago.ToLower().Contains("bito"))
                {
                    return 8;
                }
                if (formaDePago.ToLower().Contains("tarjeta cr"))
                {
                    return 5;
                }
                else if (formaDePago.ToLower().Contains("directo") )
                {
                    return 3;
                }
                else
                {
                    return 1;
                }
            }
            if (formaDePago.ToLower().Contains("efectivo urbano"))
            {
                return 3;
            }
            if (formaDePago.ToLower().Contains("cred.buses.escalera"))
            {
                return 12;
            }
            if (formaDePago.ToLower().Contains("cred.comb.fur"))
            {
                return 14;
            }
            if (formaDePago.ToLower().Contains("cred.camionetas.mixed"))
            {
                return 3;
            }
            if (formaDePago.ToLower().Contains("cred.comb.colect.intermun"))
            {
                return 10;
            }
            if (formaDePago.ToLower().Contains("cred.comb.serv.especial"))
            {
                return 13;
            }
            if (formaDePago.ToLower().Contains("arriendo"))
            {
                return 22;
            }
            if (formaDePago.ToLower().Contains("cred.comb.camperos"))
            {
                return 23;
            }
            if (formaDePago.ToLower().Contains("vip"))
            {
                return 24;
            }
            if (formaDePago.ToLower().Contains("doble yo"))
            {
                return 25;
            }
            if (formaDePago.ToLower().Contains("microbus"))
            {
                return 26;
            }
            if (formaDePago.ToLower().Contains("dito"))
            {
                return 3;
            }
            else if (formaDePago.ToLower().Contains("efe"))
            {
                return 1;
            }
            else if (formaDePago.ToLower().Contains("dat") && sucursal.ToLower() == "968")
            {
                return 29;
            }
            else if (formaDePago.ToLower().Contains("dat") && sucursal.ToLower() == "970")
            {
                return 28;
            }
            else if (formaDePago.ToLower().Contains("dat"))
            {
                return 27;
            }
            else if (formaDePago.ToLower().Contains("cons"))
            {
                return 4;
            }
            else
            {
                return 3;
            }
        }

        private string GetValidPhoneNumber(string telefono)
        {
            // Si está vacío, es null, "No informado" o no es numérico, retornar null
            if (string.IsNullOrEmpty(telefono) ||
                telefono == "No informado" ||
                !telefono.All(char.IsDigit))
            {
                return null;
            }

            // Si es válido y numérico, aplicar padding si es necesario
            return telefono.Length < 10 ? telefono.PadLeft(10, '0') : telefono;
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

        public Task<string> getJsonCanastilla(Modelo.FacturaCanastilla facturaCanastilla, Guid estacio)
        {
            throw new NotImplementedException();
        }
    }
}