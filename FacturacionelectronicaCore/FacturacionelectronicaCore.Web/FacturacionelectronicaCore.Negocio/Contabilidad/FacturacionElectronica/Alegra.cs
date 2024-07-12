using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class Alegra
    {
        public string Proveedor { get; set; }
        public bool UsaAlegra { get; set; }
        public string Token { get; set; }
        public string Correo { get; set; }
        public string Url { get; set; }
        public string DataicoAccountId { get; set; }
        public string Auth
        {
            get
            {
                var authToken = $"{Correo}:{Token}";
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(authToken);
                return System.Convert.ToBase64String(plainTextBytes);
            }
        }
        public string Cliente { get; set; }
        public bool ValidaTercero { get; set; }
        public string ResolutionNumber { get; set; }
        public string Prefix { get; set; }
        public int Current { get; set; }
        public bool MultiplicarPorDies { get; set; }
        public string AccessKey { get; set; }
        public int Seller { get; set; }
        public int Documento { get; set; }
        public string Corriente { get; set; }
        public string Acpm { get; set; }
        public string Extra { get; set; }
        public string Gas { get; set; }
        public bool EnvioDirecto { get; set; }
        public bool EnviaCreditos { get; set; }
        public bool EnviaMes { get; set; }
    }
}
