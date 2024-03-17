using EnviadorInformacionService.Models;
using EnviadorInformacionService.Models.Externos;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Web.Controllers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReporteFacturas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public class ConexionEstacionRemota : IConexionEstacionRemota
    {
        private string url;
        private string user;
        private string password;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ConexionEstacionRemota()
        {
            url = ConfigurationManager.AppSettings["url"].ToString();
            user = ConfigurationManager.AppSettings["user"].ToString();
            password = ConfigurationManager.AppSettings["password"].ToString();
        }
        public bool EnviarFacturas(IEnumerable<FactoradorEstacionesModelo.Objetos.Factura> facturas, IEnumerable<FormasPagos> formas, Guid estacion, string token)
        {
            RequestEnviarFacturas request = new RequestEnviarFacturas();
            request.facturas = facturas.Where(x => x.Consecutivo != 0).Select(x => new FacturacionelectronicaCore.Negocio.Modelo.Factura(x, formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).FirstOrDefault()));
            request.ordenDeDespachos = facturas.Where(x => x.Consecutivo == 0).Select(x => new FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho(x, formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).FirstOrDefault()));
            request.Estacion = estacion;
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/EnviarFacturas";
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                Logger.Info(JsonConvert.SerializeObject(request));
                var response = client.PostAsync($"{url}{path}", content).Result;
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Logger.Error($"{response.Content.ReadAsStringAsync().Result}");
                }
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        public bool EnviarTerceros(IEnumerable<FactoradorEstacionesModelo.Objetos.Tercero> terceros, string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/EnviarTerceros";
                var tercerosEnviar = new List<FacturacionelectronicaCore.Negocio.Modelo.Tercero>();
                foreach (FactoradorEstacionesModelo.Objetos.Tercero t in terceros)
                {
                    if (!tercerosEnviar.Any(x => x.Identificacion == t.identificacion))
                    {
                        tercerosEnviar.Add(new FacturacionelectronicaCore.Negocio.Modelo.Tercero(t));
                    }
                }
                var content = new StringContent(JsonConvert.SerializeObject(tercerosEnviar));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        public IEnumerable<string> GetGuidsFacturasPendientes(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetGuidsFacturasPendientes/{estacion}";

                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<string>>(responseBody);
            }
        }

        public string getToken()
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Usuarios/{user}/{password}";
                Console.WriteLine($"{url}{path}");
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                JObject token = JObject.Parse(responseBody);
                return token.Value<string>("token");
            }
        }

        public IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Factura> RecibirFacturasImprimir(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetFacturasImprimir/{estacion}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Factura>>(responseBody);
            }
        }


        public IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho> RecibirOrdenesImprimir(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetOrdenesDeDespachoImprimir/{estacion}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho>>(responseBody);
            }
        }

        public IEnumerable<FactoradorEstacionesModelo.Objetos.Tercero> RecibirTercerosActualizados(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetTercerosActualizados/{estacion}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var terceros = JsonConvert.DeserializeObject<IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Tercero>>(responseBody);
                return terceros.Select(x => new FactoradorEstacionesModelo.Objetos.Tercero(x));
            }
        }



        public IEnumerable<Canastilla> RecibirCanastilla(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/RecibirCanastilla/{estacion}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<Canastilla>>(responseBody);
            }
        }


        public IEnumerable<FactoradorEstacionesModelo.Objetos.Resolucion> GetResolucionEstacion(Guid estacionFuente, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Resolucion/{estacionFuente}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var resoluciones = JsonConvert.DeserializeObject< IEnumerable <FacturacionelectronicaCore.Negocio.Modelo.Resolucion> >(responseBody);
                var resolucionesLocal = new List<FactoradorEstacionesModelo.Objetos.Resolucion>();
                foreach (var resolucion in resoluciones) { resolucionesLocal.Add(new FactoradorEstacionesModelo.Objetos.Resolucion(resolucion)); }
                return resolucionesLocal;
            }
        }

        public InfoEstacion getInfoEstacion(Guid estacionFuente, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Estaciones/{estacionFuente}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var estacion = JsonConvert.DeserializeObject<Estacion>(responseBody);
                return new InfoEstacion(estacion);
            }
        }

        public bool AgregarFechaReporteFactura(List<FacturaFechaReporte> facturasFechas, Guid estacionFuente, string token)
        {
            RequestCambiarFechasReporte request = new RequestCambiarFechasReporte();
            request.facturas = facturasFechas;
            request.Estacion = estacionFuente;
            Logger.Info(JsonConvert.SerializeObject(request));
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/AgregarFechaReporteFactura";
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        public string GetInfoFacturaElectronica(int idVentaLocal, Guid estacionGuid, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetInfoFacturaElectronica/{idVentaLocal}/estacion/{estacionGuid}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public FactoradorEstacionesModelo.Objetos.Resolucion GetResolucionEstacionCanastilla(Guid estacionFuente, string token)
        {
            throw new NotImplementedException();
        }

        public bool EnviarFacturasCanastilla(IEnumerable<FacturaCanastilla> facturas, Guid estacionFuente, string token)
        {
            RequestfacturasCanastilla request = new RequestfacturasCanastilla();
            request.facturas = facturas;
            request.estacion = estacionFuente;
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("bearer", token);
                var path = $"/api/manejadorinformacionlocal/AddFacturaCanastilla";

                Logger.Info(JsonConvert.SerializeObject(request));
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responsebody = response.Content.ReadAsStringAsync().Result;
                return responsebody != "-1";
            }
            
        }

        public int ObtenerParaImprimir(Guid idEstacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/FacturasCanastilla/ObtenerParaImprimir/Estacion/{idEstacion}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                return int.Parse(response.Content.ReadAsStringAsync().Result);
            }
        }

        public ResolucionElectronica GetResolucionElectronica(string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetResolucionElectronica";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<ResolucionElectronica>(response.Content.ReadAsStringAsync().Result);
            }
        }

        public bool SetTurnoFactura(object ventaId, DateTime fechaApertura, string isla, int numero, Guid estacionFuente, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Factura/AgregarTurnoAFactura/{ventaId}/{fechaApertura}/{isla}/{numero}/{estacionFuente}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;

                return response.IsSuccessStatusCode;
            }
        }

        public void SubirTurno(Turno turno, Guid estacionFuente, string token)
        {
            turno.EstacionGuid = estacionFuente.ToString();
            turno.Id = Guid.NewGuid().ToString();
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("bearer", token);
                var path = $"/api/Turnos";

                Logger.Info(JsonConvert.SerializeObject(turno));
                var content = new StringContent(JsonConvert.SerializeObject(turno));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responsebody = response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
