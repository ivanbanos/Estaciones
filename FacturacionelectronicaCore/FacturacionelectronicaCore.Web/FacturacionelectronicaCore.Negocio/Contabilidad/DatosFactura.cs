using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Configuration;
using System.Globalization;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class DatosFactura
    {

        public DatosFactura(FacturaSilog factura, string usuario, Alegra options, string estacion)
        {
            Consecutivo = factura.Consecutivo + "";
            Detalles = $"Placa: {factura.Placa}, Kilometraje : {factura.Kilometraje}, Nro Transaccion : {factura.numeroTransaccion}";
            Placa = factura.Placa;
            Kilometraje = factura.Kilometraje;
            Surtidor = factura.Surtidor;
            Cara = factura.Cara;
            Manguera = factura.Manguera;
            if (DateTime.Now.Date < factura.Fecha.Date)
            {
                FechaFacturacion = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {

                FechaFacturacion = factura.Fecha.ToString("yyyy-MM-dd");
            }
            Tercero = factura.Tercero.Identificacion;
            Usuario = usuario;

            Cantidad = factura.Cantidad.ToString("F3",
                  CultureInfo.InvariantCulture);
            Precio = factura.Precio.ToString("F3",
                  CultureInfo.InvariantCulture);
            Descuento = factura.Descuento.ToString("F3",
                  CultureInfo.InvariantCulture);
            Iva = "0";
            Total = (factura.SubTotal - factura.Descuento).ToString("F3",
                  CultureInfo.InvariantCulture);
            Subtotal = factura.SubTotal.ToString("F3",
                  CultureInfo.InvariantCulture);
            FechaProximoMantenimiento = factura.FechaProximoMantenimiento;
            Guid = factura.Guid;
            Prefijo = factura.Prefijo;
            Console.WriteLine();
            if(options.Cliente.ToLower() == "cotaxi")
            {
                if (factura.FormaDePago.ToLower().Contains("efectivo"))
                {
                    FormaPago = "1";
                }
                else if (factura.FormaDePago.ToLower().Contains("dito"))
                {
                    FormaPago = "3";
                }
                else if (factura.FormaDePago.ToLower().Contains("trans"))
                {
                    FormaPago = "7";
                    NroTransaccion = factura.numeroTransaccion ?? factura.Kilometraje ?? "NA";
                }
                else
                {
                    FormaPago = "2";
                    TipoTarjeta = GetByFormaPago(factura.FormaDePago.Trim());
                    NroTransaccion = factura.numeroTransaccion ?? factura.Kilometraje ?? "NA";
                }

            }
            else
            {
                if (factura.FormaDePago.ToLower().Contains("efe"))
                {
                    FormaPago = "1";
                }
                else
                if (factura.FormaDePago.ToLower().Contains("dat"))
                {
                    FormaPago = "2";
                    TipoTarjeta = GetByFormaPagoCootranshuila(factura.FormaDePago.Trim(), estacion);
                    NroTransaccion = factura.Kilometraje;
                }
                else
                {
                    FormaPago = "3";
                    TipoTarjeta = GetByFormaPagoCootranshuila(factura.FormaDePago.Trim(), estacion);
                    NroTransaccion = factura.Kilometraje;
                }
            }




            if (factura.Combustible.ToLower().Contains("corriente"))
            {
                Combustible = options.Corriente;
            }
            else if (factura.Combustible.ToLower().Contains("diesel") || factura.Combustible.ToLower().Contains("bioacem") || factura.Combustible.ToLower().Contains("acpm"))
            {
                Combustible = options.Acpm;
            }
            else if (factura.Combustible.ToLower().Contains("extra"))
            {
                Combustible = options.Extra;
            }
            else if (factura.Combustible.ToLower().Contains("gas") || factura.Combustible.ToLower().Contains("gnvc"))
            {
                Combustible = options.Gas;
            }
        }

        private string GetByFormaPagoCootranshuila(string formaPago, string estacion)
        {
            if (formaPago.ToLower().Contains("asum")|| formaPago.ToLower().Contains("cali"))
            {
                return "21";

            }
            else
            {
                if (estacion.ToLower() == "BF5D6034-82FC-484D-A362-65EFB0E4C1C6".ToLower())
                {
                    //libague
                    return "22";
                }
                if (estacion.ToLower() == "F6A3C48F-ACC9-4E9A-9B3C-206A0E52C302".ToLower())
                {
                    //terminal
                    return "23";
                }
                if (estacion.ToLower() == "4E8E1941-D8E4-4732-A3C8-27A2DFEE9048".ToLower())
                {
                    //toma
                    return "9";
                }
                if (estacion.ToLower() == "9DC61E74-AEB4-4E3A-8121-3F20F391C4A3".ToLower())
                {
                    //principal
                    return "9";
                }
            }
            return "21";
        }

        private string GetByFormaPago(string formaPago)
        {
            switch (formaPago)
            {
                case "Visa":
                    return "1";
                case "MasterCard":
                    return "9";
                case "Dinners":
                    return "2";
                case "Amex":
                    return "14";
                case "Puntos Colombia":
                    return "15";
                case "Sodexo Quantum":
                    return "16";
                case "Vales":
                    return "17";
                case "Bonos Sodexo":
                    return "18";
                default:
                    return "21";
            }

        }

        public string Consecutivo { get; set; }
        public string Detalles { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public string fechaUltimoMantenimiento { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public string FechaFacturacion { get; set; }
        public string Tercero { get; set; }
        public string FormaPago { get; set; }
        public string TipoTarjeta { get; set; }
        public string NroTransaccion { get; set; }
        public string Combustible { get; set; }
        public string Cantidad { get; set; }
        public string Precio { get; set; }
        public string Descuento { get; set; }
        public string Iva { get; set; }
        public string Total { get; set; }
        public DateTime? FechaProximoMantenimiento { get; set; }
        public string Subtotal { get; set; }
        public string Usuario { get; set; }
        public string Prefijo { get; set; }
        public Guid Guid { get; set; }

    }
}
