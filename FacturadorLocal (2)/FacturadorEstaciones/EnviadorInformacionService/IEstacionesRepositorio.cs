using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public interface IEstacionesRepositorio
    {
        Resolucion BuscarResolucionActiva(IEnumerable<Resolucion> resolucionRemota);
        void CambiarConsecutivoActual(int v);
        List<Tercero> BuscarTercerosNoEnviados();
        List<Factura> BuscarFacturasNoEnviadas();
        List<FormasPagos> BuscarFormasPagos();
        void ActuralizarTercerosEnviados(IEnumerable<int> enumerable);
        void ActuralizarFacturasEnviados(IEnumerable<int> enumerable);
        void ActuralizarTerceros(Tercero tercero);
        void AgregarFacturaDesdeIdVenta();
        void MandarImprimir(int ventaId);
        List<Factura> BuscarFacturasNoEnviadasFacturacion();
        void ActuralizarFacturasEnviadosFacturacion(IEnumerable<int> facturas);
        List<FacturaFechaReporte> BuscarFechasReportesNoEnviadas();
        void ActuralizarFechasReportesEnviadas(IEnumerable<int> facturas);
    }
}
