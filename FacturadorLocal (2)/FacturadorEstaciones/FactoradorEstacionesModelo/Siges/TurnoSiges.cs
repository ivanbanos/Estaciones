﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Siges
{
    public class TurnoSiges
    {
        public int impresa { get; set; } = 0;

        public int Id { get; set; }
        public string Empleado { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int IdEstado { get; set; }

        public List<TurnoSurtidor> turnoSurtidores {get;set;}
    }
}
