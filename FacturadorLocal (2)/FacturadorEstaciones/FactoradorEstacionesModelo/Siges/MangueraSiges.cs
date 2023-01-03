using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Siges
{
    public class MangueraSiges
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public bool Totalizador { get; set; } = false;
    }
}
