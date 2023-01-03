using System;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class OrdenDeDespacho
    {

        public OrdenDeDespacho()
        {
        }
        public Guid Guid { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
        public string Combustible { get; set; }
        public double Cantidad { get; set; }
        public double Precio { get; set; }
        public double Total { get; set; }
        public decimal Descuento { get; set; }
        public string IdInterno { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public Tercero Tercero { get; set; }
        public string FormaDePago { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public decimal SubTotal { get; set; }
        public string Vendedor { get; set; }
        public Guid estacion { get; set; }


        public OrdenDeDespacho(FactoradorEstacionesModelo.Objetos.Factura x, string forma)
        {
            Guid = Guid.NewGuid();
            Combustible = x.Venta.Combustible;
            Cantidad = (double)x.Venta.CANTIDAD;
            Precio = (double)x.Venta.PRECIO_UNI;
            Total = (double)x.Venta.TOTAL;
            IdInterno = x.Venta.COD_INT;
            Placa = x.Placa;
            Kilometraje = x.Kilometraje;
            Surtidor = x.Venta.COD_SUR + "";
            Cara = x.Venta.COD_CAR + "";
            Manguera = x.Manguera.COD_MAN + "";
            FormaDePago = forma;
            Fecha = x.fecha;
            Descuento  = x.Venta.Descuento;
            Tercero = new Tercero(x.Tercero);
            IdLocal = x.facturaPOSId;
            IdVentaLocal = x.Venta.CONSECUTIVO;
            IdTerceroLocal = x.Tercero.terceroId;
            FechaProximoMantenimiento = x.Venta.FECH_PRMA.HasValue ? x.Venta.FECH_PRMA.Value : DateTime.Now;
            SubTotal = x.Venta.VALORNETO;
            Vendedor = x.Venta.EMPLEADO;
            Identificacion = x.Tercero.identificacion;
        }
    }
}
