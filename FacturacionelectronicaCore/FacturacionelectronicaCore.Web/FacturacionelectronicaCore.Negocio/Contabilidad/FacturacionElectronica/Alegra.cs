using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class Alegra
    {
        public bool UsaAlegra { get; set; }
        public string Token { get; set; }
        public string Correo { get; set; }
        public string Url { get; set; }
        public string DataicoAccountId { get; set; }
        public string Auth { get { var authToken = $"{Correo}:{Token}";
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(authToken);
                return System.Convert.ToBase64String(plainTextBytes);
            } }

        public bool ValidaTercero { get; set; }
        public string ResolutionNumber { get; internal set; }
        public string Prefix { get; internal set; }
    }
}
