﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Objetos
{
    public class Resolucion
    {
        public Resolucion()
        {
        }
        public Resolucion(FacturacionelectronicaCore.Negocio.Modelo.Resolucion resolucion)
        {
            DescripcionResolucion = resolucion.Descripcion;
            FechaFinalResolucion = resolucion.FechaFinal;
            FechaInicioResolucion = resolucion.FechaInicial;
            ConsecutivoInicial = resolucion.ConsecutivoInicial;
            ConsecutivoFinal = resolucion.ConsecutivoFinal;
            ConsecutivoActual = resolucion.ConsecutivoActual;
            Autorizacion = resolucion.Autorizacion;
            ConsecutivoActual = resolucion.ConsecutivoActual;
            Habilitada = resolucion.Habilitada;
            Tipo = resolucion.Tipo;
        }

        public string DescripcionResolucion { get; set; }
        public DateTime FechaInicioResolucion { get; set; }
        public DateTime FechaFinalResolucion { get; set; }
        public int ConsecutivoInicial { get; set; }
        public int ConsecutivoFinal { get; set; }
        public int ConsecutivoActual { get; set; }
        public string Autorizacion { get; set; }
        public bool Habilitada { get; internal set; }
        public int Tipo { get; set; }
    }
}
