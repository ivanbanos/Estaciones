
using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturadorEstacionesPOSWinForm;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EnviadorInformacionService
{
    public class ConexionEstacionRemota : IConexionEstacionRemota
    {
        private string url;

        public ConexionEstacionRemota(IOptions<InfoEstacion> options)
        {
            url = options.Value.UrlLocalService;
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };
        }


        public bool GetIsTerceroValidoPorIdentificacion(string  identificacion)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Ventas/GetIsTerceroValidoPorIdentificacion/{identificacion}";

                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return bool.Parse(responseBody);
            }
        }

        public string ObtenerFacturaPorIdVentaLocal(int idVentaLocal)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Ventas/ObtenerFacturaPorIdVentaLocal/{idVentaLocal}";

                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public string ObtenerOrdenDespachoPorIdVentaLocal(int identificacion)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Ventas/ObtenerOrdenDespachoPorIdVentaLocal/{identificacion}";

                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }
        public string CrearFacturaOrdenesDeDespacho(string guid)
        {
            List<string> guids = new List<string>() { guid };
            
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/CrearFacturaOrdenesDeDespacho";
                var content = new StringContent(JsonConvert.SerializeObject(guids));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public string CrearFacturaFacturas(string guid)
        {
            List<string> guids = new List<string>() { guid };

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/CrearFacturaFacturas";
                var content = new StringContent(JsonConvert.SerializeObject(guids));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }
        public string EnviarFactura(Factura factura)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                var path = $"/api/Ventas/EnviarFacturaElectronica";
                var content = new StringContent(JsonConvert.SerializeObject(factura));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public List<Canastilla> GetCanastilla()
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Ventas/GetCanastilla";

                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<Canastilla>>(responseBody);
            }
        }

        public int GenerarFacturaCanastilla(FacturaCanastilla factura)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/GenerarFacturaCanastilla";
                var content = new StringContent(JsonConvert.SerializeObject(factura));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return int.Parse(responseBody);
            }
        }
    }
}
