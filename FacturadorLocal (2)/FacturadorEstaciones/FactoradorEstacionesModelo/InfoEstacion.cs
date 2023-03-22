using EnviadorInformacionService.Models.Externos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorEstacionesPOSWinForm
{
    public class InfoEstacion
    {

        public InfoEstacion()
        {
        }
        public InfoEstacion(Estacion estacion)
        {
            Direccion = estacion.Direccion;
            Linea1 = estacion.linea1;
            Linea2 = estacion.linea2;
            Linea3 = estacion.linea3;
            Linea4 = estacion.linea4;
            NIT = estacion.Nit;
            Nombre = estacion.Nombre;
            Razon = estacion.Razon;
            Telefono = estacion.Telefono;
        }
        public string UrlLocalService { get; set; }
        public int vecesPermitidasImpresion { get; set; }

        public string Razon { get; set; }
        public string NIT { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public int CaracteresPorPagina { get; set; }
        public string Linea1 { get; set; }
        public string Linea2 { get; set; }
        public string Linea3 { get; set; }
        public string Linea4 { get; set; }
        public string ip { get; set; }
        public string puerto { get; set; }
        public int vecesImprimir { get; set; }
        public string ArchivoMovimientoContable { get; set; }
        public bool ConvertirAFactura { get; set; }
        public bool CreaMovimientoContable { get; set; }
        public bool ConvertirAOrden { get; set; }
        public string EstacionFuente { get; set; }
        public bool ImpresionAutomatica { get; set; }
        public bool ImpresionFormaDePagoOrdenDespacho { get; set; }
        public bool GeneraFacturaElectronica { get; set; }
        public bool ImpresionPDA { get; set; }
        public object Url { get; set; }
        public object Password { get; set; }
        public object User { get; set; }
    }
}
