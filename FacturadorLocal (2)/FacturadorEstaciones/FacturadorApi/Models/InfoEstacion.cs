using System;
using System.Collections.Generic;
using System.Text;

namespace ReporteFacturas
{
    public class InfoEstacion
    {
        public int vecesPermitidasImpresion { get; set; }

        public string Razon { get; set; }
    public string NIT { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public int CaracteresPorPagina { get; set; }
        public string Linea1 { get; set; }
        public string Linea2 { get; set; }
        public string Linea3 { get; internal set; }
        public string Linea4 { get; internal set; }
    }
}
