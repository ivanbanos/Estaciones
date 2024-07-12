using EnviadorInformacionService.Models;
using EnviadorInformacionService.Models.Externos;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Negocio.Modelo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EnviadorInformacionService
{
    public class ConexionEstacionRemota 
    {
        private string url;
        private string user;
        private string password;
        private string estacion;

        public ConexionEstacionRemota()
        {
            url = ConfigurationManager.AppSettings["url"].ToString();
            user = ConfigurationManager.AppSettings["user"].ToString();
            password = ConfigurationManager.AppSettings["password"].ToString();
            estacion = ConfigurationManager.AppSettings["estacionFuente"].ToString();
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };
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


        public bool GetIsTerceroValidoPorIdentificacion(string  identificacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Terceros/GetIsTerceroValidoPorIdentificacion/{identificacion}";

                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return bool.Parse(responseBody);
            }
        }

        public string ObtenerFacturaPorIdVentaLocal(int idVentaLocal, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Factura/ObtenerFacturaPorIdVentaLocal/{idVentaLocal}/{estacion}";

                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                var responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public string ObtenerOrdenDespachoPorIdVentaLocal(int identificacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/OrdenesDeDespacho/ObtenerOrdenDespachoPorIdVentaLocal/{identificacion}/{estacion}";
                HttpResponseMessage response = null;
                try
                {
                    
                    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

                    response = client.GetAsync($"{url}{path}").Result;
                   // response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return responseBody;
                }
                catch (Exception ex) {
                    throw new Exception($"{url}{path}"+ response.ReasonPhrase+ response.Content.ReadAsStringAsync().Result);
                }
            }
        }
        public string CrearFacturaOrdenesDeDespacho(string guid, string token)
        {
            return "Ok";
            //        List<FacturasEntity> guids = new List<FacturasEntity>() { new FacturasEntity() { Guid = Guid.Parse(guid) } };


            //        using (var client = new HttpClient())
            //        {
            //            client.Timeout = new TimeSpan(0, 0, 10, 0, 0);
            //            client.DefaultRequestHeaders.Authorization =
            //new AuthenticationHeaderValue("Bearer", token);
            //            var path = $"/api/OrdenesDeDespacho/EnviarFacturacion/{guid}";
            //            var response = client.GetAsync($"{url}{path}").Result;
            //            string responseBody = response.Content.ReadAsStringAsync().Result;
            //            return responseBody;
            //        }
        }

        public string CrearFacturaFacturas(string guid, string token)
        {
            return "Ok";
            //        List<FacturasEntity> guids = new List<FacturasEntity>() { new FacturasEntity() { Guid = Guid.Parse(guid) } };

            //        using (var client = new HttpClient())
            //        {
            //            client.Timeout = new TimeSpan(0, 0, 10, 0, 0);
            //            client.DefaultRequestHeaders.Authorization =
            //new AuthenticationHeaderValue("Bearer", token);
            //            var path = $"/api/Factura/EnviarFacturacion/{guid}";
            //            var response = client.GetAsync($"{url}{path}").Result;
            //            //string responseBody = response.Content.ReadAsStringAsync().Result;
            //            return "Ok";
            //        }
        }
        public class FacturasEntity
        {
            public Guid Guid { get; set; }
        }
        public bool EnviarFacturas(IEnumerable<FactoradorEstacionesModelo.Objetos.Factura> facturas, IEnumerable<FormasPagos> formas, string token)
        {
            RequestEnviarFacturas request = new RequestEnviarFacturas();
            request.facturas = facturas.Where(x => x.Consecutivo != 0).Select(x => new FacturacionelectronicaCore.Negocio.Modelo.Factura(x, formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).Single()));
            request.ordenDeDespachos = facturas.Where(x => x.Consecutivo == 0).Select(x => new FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho(x, formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).Single()));
            request.Estacion = Guid.Parse(estacion);
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/EnviarFacturas";
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;

                try
                {
                    response.EnsureSuccessStatusCode();
                }catch(Exception ex)
                {
                    throw;
                }
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

        public IEnumerable<Canastilla> RecibirCanastilla(string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Canastilla";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<Canastilla>>(responseBody);
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

        internal string CrearFacturaFacturasyVenta(int ventaId, string token)
        {
            //        using (var client = new HttpClient())
            //        {
            //            client.Timeout = new TimeSpan(0, 0, 10, 0, 0);
            //            client.DefaultRequestHeaders.Authorization =
            //new AuthenticationHeaderValue("Bearer", token);
            //            var path = $"/api/Factura/EnviarFacturacion/{ventaId}/{estacion}";
            //            var response = client.GetAsync($"{url}{path}").Result;
            //            return "Ok";
            //        }

            return "Ok";
        }

        internal string CrearFacturaOrdenesDeDespachoByVenta(int ventaId, string token)
        {
            return "Ok";
            //        using (var client = new HttpClient())
            //        {
            //            client.Timeout = new TimeSpan(0, 0, 10, 0, 0);
            //            client.DefaultRequestHeaders.Authorization =
            //new AuthenticationHeaderValue("Bearer", token);
            //            var path = $"/api/OrdenesDeDespacho/EnviarFacturacion/{ventaId}/{estacion}";
            //            var response = client.GetAsync($"{url}{path}").Result;
            //            return "Ok";
            //        }
        }
    }
}
