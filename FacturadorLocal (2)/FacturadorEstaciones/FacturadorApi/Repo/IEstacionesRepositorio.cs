using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public interface IEstacionesRepositorio
    {
        Tercero getTercero(string identificacion);
        List<Cara> getCaras();
        List<TipoIdentificacion> getTiposIdentifiaciones();
        List<Factura> getUltimasFacturas(short cOD_CAR, int v);
        Tercero crearTercero(int terceroId, TipoIdentificacion tipoIdentificacion, string identificacion, string nombre, string telefono, string correo, string direccion, string cOD_CLI);
        void ActualizarFactura(int facturaPOSId, string placa, string kilometraje, int formaPago, int terceroId, int ventaId); 
        List<FormasPagos> BuscarFormasPagos();
        void MandarImprimir(int ventaId);
        List<Isla> getIslas();
        void enviarFacturacionSiigo(int ventaId);
        void ConvertirAFactura(int ventaId);
        void ActuralizarFacturasEnviados(List<int> lists);
        IEnumerable<Canastilla> GetCanastillas();
        int GenerarFacturaCanastilla(FacturaCanastilla factura, bool imprimir);


        void ActualizarCanastilla(IEnumerable<Canastilla> canastillas);
        FacturaCanastilla BuscarFacturaCanastillaPorConsecutivo(int consecutivo);
        string ObtenerCodigoInterno(string placa, string v);
        void ConvertirAOrden(int ventaId);
    }
}
