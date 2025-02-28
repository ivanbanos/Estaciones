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
        public bool Desactivado { get; set; }
        public string City { get;  set; }
        public string Department { get; set; }
        public bool ExcluirDireccion { get; set; }


        public string Contrasena { get; set; }
        public string Usuario { get; set; }
        public string Nit { get; set; }
        public int Debito { get; set; }
        public int Credito { get; set; }
        public int Efectivo { get; set; }
    }
}
