using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using Google.Protobuf.WellKnownTypes;
using System;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class DatosFactura
    {

        public DatosFactura(FacturaSilog factura, string usuario)
        {
            Consecutivo = factura.Consecutivo + "";
            Detalles = $"Placa: {factura.Placa}, Kilometraje : {factura.Kilometraje}";
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

            Cantidad = factura.Cantidad + "";
            Precio = factura.Precio + "";
            Descuento = "0";
            Iva = "0";
            Total = String.Format("{0:0.##}", factura.SubTotal);
            Subtotal = String.Format("{0:0.##}", factura.SubTotal);
            FechaProximoMantenimiento = factura.FechaProximoMantenimiento;
            Guid = factura.Guid;
            Prefijo = factura.Prefijo.Substring(0, factura.Prefijo.Length - Consecutivo.Length);
            Console.WriteLine();
            if (factura.FormaDePago.ToLower().Contains("efectivo"))
            {
                FormaPago = "1";
            }
            else if (factura.FormaDePago.ToLower().Contains("dito"))
            {
                FormaPago = "3";
            }
            else
            {
                FormaPago = "2";
                TipoTarjeta = GetByFormaPago(factura.FormaDePago.Trim());
            }




            if (factura.Combustible.ToLower().Contains("corriente"))
            {
                Combustible = "11GASOLINA";
            }
            else if (factura.Combustible.ToLower().Contains("diesel") || factura.Combustible.ToLower().Contains("acpm"))
            {
                Combustible = "12ACPM";
            }
            else if (factura.Combustible.ToLower().Contains("extra"))
            {
                Combustible = "300";
            }
            else if (factura.Combustible.ToLower().Contains("gas") || factura.Combustible.ToLower().Contains("gnvc"))
            {
                Combustible = "400";
            }
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
