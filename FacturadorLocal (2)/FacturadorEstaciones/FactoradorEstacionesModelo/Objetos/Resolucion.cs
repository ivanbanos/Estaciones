using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Objetos
{
    public class Resolucion
    {
        public string DescripcionResolucion { get; set; }
        public DateTime FechaInicioResolucion { get; set; }
        public DateTime FechaFinalResolucion { get; set; }
        public int consecutivoInicial { get; set; }
        public int consecutivoFinal { get; set; }
        public int consecutivoActual { get; set; }
    }
}
