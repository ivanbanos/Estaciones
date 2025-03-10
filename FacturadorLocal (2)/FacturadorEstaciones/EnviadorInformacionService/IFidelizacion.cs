﻿
using EnviadorInformacionService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public interface IFidelizacion
    {
        Task<IEnumerable<Fidelizado>> GetFidelizados(string documentoFidelizado);
        Task SubirPuntops(float total, string documentoFidelizado, string factura);
    }
}
