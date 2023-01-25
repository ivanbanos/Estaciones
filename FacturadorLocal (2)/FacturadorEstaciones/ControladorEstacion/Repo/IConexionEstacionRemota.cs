using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorEstacionesPOSWinForm.Repo
{
    public interface IConexionEstacionRemota
    {
        bool GetIsTerceroValidoPorIdentificacion(string identificacion);

        string ObtenerFacturaPorIdVentaLocal(int idVentaLocal);

        string ObtenerOrdenDespachoPorIdVentaLocal(int identificacion);
        string CrearFacturaOrdenesDeDespacho(string guid);

        string CrearFacturaFacturas(string guid);
        string EnviarFactura(Factura factura);
        List<Canastilla> GetCanastilla();
        int GenerarFacturaCanastilla(FacturaCanastilla factura);
        object EnviarFacturaSiges(FacturaSiges factura);
    }
}
