using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using System;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class DatosFactura
    {

        public DatosFactura(FacturaSilog factura, string usuario)
		{
			Consecutivo = factura.Consecutivo+"";
			Detalles = "Detalles";
			Placa = factura.Placa;
			Kilometraje = factura.Kilometraje;
			Surtidor = factura.Surtidor;
			Cara = factura.Cara;
			Manguera = factura.Manguera;
			FechaFacturacion = factura.Fecha.ToString("yyyy-MM-dd");
            Tercero = factura.Tercero.Identificacion;
            Usuario = usuario;

            Cantidad = factura.Cantidad + "";
			Precio = factura.Precio + "";
			Descuento = "0";
			Iva = "0";
			Total = String.Format("{0:0.##}", factura.Precio * factura.Cantidad);
			SubTotal = String.Format("{0:0.##}", factura.SubTotal);
			FechaProximoMantenimiento = factura.FechaProximoMantenimiento;
			Guid = factura.Guid;
			Prefijo = factura.Prefijo;
            if (factura.FormaDePago.ToLower().Contains("efectivo"))
			{
				FormaPago = "1";
			}
			else
			{
				FormaPago = "3";
			}
			if (factura.Combustible.ToLower().Contains("corriente"))
			{
				Combustible = "100";
			}
			else if (factura.Combustible.ToLower().Contains("acpm"))
			{
				Combustible = "200";
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
		public string Combustible { get; set; }
		public string Cantidad { get; set; }
		public string Precio { get; set; }
		public string Descuento { get; set; }
		public string Iva { get; set; }
		public string Total { get; set; }
		public DateTime? FechaProximoMantenimiento { get; set; }
		public string SubTotal { get; set; }
        public string Usuario { get; set; }
        public string Prefijo { get; set; }
        public Guid Guid { get; set; }

    }
}
