using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ManejadorSurtidor.SICOM
{
    public class SicomConection : ISicomConection
    {
        private Sicom sicom;

        private readonly Logger _logger;
        public SicomConection(IOptions<Sicom> options, ILogger<SicomConection> logger)
        {
            sicom = options.Value;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };
        }

        public async Task<bool> validateIButton(string iButton)
        {
            using (var client = new HttpClient())
            {
                try
                {

                    var path = $"/chip/{iButton}";

                    Uri uri = new Uri(sicom.Url + path);
                    var authenticationString = $"{sicom.Usuario}:{sicom.Contrasena}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                    client.DefaultRequestHeaders.Add("APIKey", sicom.APIKey);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                    var response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    _logger.Log(NLog.LogLevel.Info, $"Sicom respuesta {responseBody}");
                    var chipResponse = JsonConvert.DeserializeObject<ChipResponse>(responseBody);
                    return chipResponse.estado == "0";
                } catch(Exception ex)
                {
                    
                    _logger.Log(NLog.LogLevel.Info, $"Sicom error {ex.Message}");
                    return true;
                }
            }
        }


        public async Task<bool> enviarVenta(string iButton, float consumo)
        {
            using (var client = new HttpClient())
            {
                try
                {

                    var path = $"/consumo/{iButton}";

                    Uri uri = new Uri(sicom.Url + path);
                    var authenticationString = $"{sicom.Usuario}:{sicom.Contrasena}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                    client.DefaultRequestHeaders.Add("APIKey", sicom.APIKey);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                    var person = new Consumo() { Volumen = consumo, fecha = DateTime.Now };

                    var json = JsonConvert.SerializeObject(person);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(uri, data);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    _logger.Log(NLog.LogLevel.Info, $"Sicom respuesta {responseBody}");
                    //var chipResponse = JsonConvert.DeserializeObject<ChipResponse>(responseBody);
                    return true;
                }
                catch (Exception ex)
                {

                    _logger.Log(NLog.LogLevel.Info, $"Sicom error {ex.Message}");
                    return true;
                }
            }
        }

        public async Task<string> GetInfoCarros()
        {
            using (var client = new HttpClient())
            {
                try
                {

                    var path = $"/certificados/";

                    Uri uri = new Uri(sicom.Url + path);
                    var authenticationString = $"{sicom.Usuario}:{sicom.Contrasena}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                    client.DefaultRequestHeaders.Add("APIKey", sicom.APIKey);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                    var response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    _logger.Log(NLog.LogLevel.Info, $"Sicom respuesta {responseBody}");

                    File.WriteAllText("SUIC.txt", responseBody);
                    return responseBody;
                }
                catch (Exception ex)
                {

                    _logger.Log(NLog.LogLevel.Info, $"Sicom error {ex.Message}");
                    return "Fail";
                }
            }
        }

    }
}
