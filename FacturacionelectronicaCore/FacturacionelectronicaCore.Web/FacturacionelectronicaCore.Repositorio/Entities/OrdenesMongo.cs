using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class OrdenesMongo : OrdenDeDespacho
    {
        public OrdenesMongo() { }

        public OrdenesMongo(OrdenDeDespacho orden)
        {
            guid = orden.guid;
            IdFactura = orden.IdFactura;
            Identificacion = orden.Identificacion;
            NombreTercero = orden.NombreTercero;
            Combustible = orden.Combustible;
            Cantidad = orden.Cantidad;
            Precio = orden.Precio;
            Total = orden.Total;
            Descuento = orden.Descuento;
            IdInterno = orden.IdInterno;
            Placa = orden.Placa;
            Kilometraje = orden.Kilometraje;
            IdEstadoActual = orden.IdEstadoActual;
            Surtidor = orden.Surtidor;
            Cara = orden.Cara;
            Manguera = orden.Manguera;
            Fecha = orden.Fecha;
            Estado = orden.Estado;
            IdentificacionTercero = orden.IdentificacionTercero;
            FormaDePago = orden.FormaDePago;
            IdLocal = orden.IdLocal;
            IdVentaLocal = orden.IdVentaLocal;
            IdTerceroLocal = orden.IdTerceroLocal;
            IdEstacion = orden.IdEstacion;
            SubTotal = orden.SubTotal;
            FechaProximoMantenimiento = orden.FechaProximoMantenimiento;
            Vendedor = orden.Vendedor;
            idFacturaElectronica = orden.idFacturaElectronica;
        }

        public DateTime FechaReporte { get; set; }
        public string EstacionGuid { get; set; }
        public string TurnoGuid { get; set; }
    }
}
