﻿using FacturacionelectronicaCore.Negocio.Modelo;
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

        public FacturacionSilog(IOptions<Alegra> alegra, ResolucionNumber resolucionNumber, IResolucionRepositorio resolucionRepositorio)
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

                var invoice = await GetFacturaSilog(factura, tercero, estacionGuid);
                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                var str = regex.Replace(factura.Tercero.Nombre, @" ");
                RequestContabilidad request = new RequestContabilidad(invoice);

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
                           throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                        }
                        else
                        {
                            return  "Ok:" + respuestaSilog.datos_factura.FirstOrDefault()?.arr_facturas.FirstOrDefault()?.nro + ":" + respuestaSilog.uuid_cufe;

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

        public async Task<FacturaSilog> GetFacturaSilog(Modelo.Factura x, Modelo.Tercero tercero, Guid estacionGuid)
        {
            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(alegraOptions.Prefix);

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
            return new FacturaSilog()
            {
                Guid = estacionGuid,
                Consecutivo = x.Consecutivo,
                Combustible = x.Combustible,
                Cantidad = x.Cantidad,
                Precio = x.Precio,
                Total = x.Total,
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
                SubTotal = x.SubTotal,
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
        public async Task<FacturaSilog> GetFacturaSilog(Modelo.OrdenDeDespacho x, Modelo.Tercero tercero, Guid estacionGuid)
        {
            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(alegraOptions.Prefix);
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

            return new FacturaSilog()
            {
                Guid = estacionGuid,
            Consecutivo = x.IdVentaLocal,
            Combustible = x.Combustible,
            Cantidad = (decimal)x.Cantidad,
            Precio = (decimal)x.Precio,
            Total = (decimal)x.Total,
            Placa = x.Placa,
            Kilometraje = x.Kilometraje,
            Surtidor = x.Surtidor + "",
            Cara = x.Cara + "",
            Manguera = x.Manguera + "",
            FormaDePago = x.FormaDePago,
            Fecha = x.Fecha,
            Tercero = new TerceroSilog() {
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
            FechaProximoMantenimiento = x.FechaProximoMantenimiento ,
            SubTotal = x.SubTotal,
            Vendedor = x.Vendedor,
            Identificacion = x.Tercero.Identificacion,
            Prefijo =  x.IdVentaLocal+"",
            Cedula = tercero.Identificacion,
        };
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {

            try
            {

                var invoice = await GetFacturaSilog(orden, tercero, estacionGuid);
                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                var str = regex.Replace(orden.Tercero.Nombre, @" ");
                RequestContabilidad request = new RequestContabilidad(invoice);

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
                            throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                        }
                        else
                        {
                            return "Ok:" + respuestaSilog.datos_factura.FirstOrDefault()?.arr_facturas.FirstOrDefault()?.nro + ":" + respuestaSilog.uuid_cufe;

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

        public async Task<ResolucionElectronica> GetResolucionElectronica()
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
    }
}