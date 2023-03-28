using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.Alegra
{
    public class Alegra
    {
        public bool UsaAlegra { get; set; }
        public string Token { get; set; }
        public string Correo { get; set; }
        public string Url { get; set; }
        public string Auth { get { var authToken = $"{Correo}:{Token}";
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(authToken);
                return System.Convert.ToBase64String(plainTextBytes);
            } }
    }
}
