﻿using Dominio.Entidades;
using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Fidelizacion;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Repositorio.Entities;
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
        Tercero crearTercero(int terceroId, TipoIdentificacion selectedItem, string text1, string text2, string text3, string text4, string text5, string cOD_CLI);
        List<Factura> getUltimasFacturas(short cOD_CAR, int v); 
        void ActualizarFactura(int facturaPOSId, string placa, string kilometraje, int formaPago, int terceroId, int ventaId);
        List<FormasPagos> BuscarFormasPagos();
        List<Isla> getIslas();
        void MandarImprimir(int ventaId);
        void enviarFacturacionSiigo(int ventaId);
        void ConvertirAFactura(int ventaId);
        void ConvertirAOrden(int ventaId);
        List<CaraSiges> GetCarasSiges();
        List<SurtidorSiges> GetSurtidoresSiges();
        List<MangueraSiges> GetMangueras(int id);
        void AgregarVenta(int idManguera, double cantidadventa, string iButton);
        List<FacturaSiges> getVentaSinSubirSICOM();
        List<FormaPagoSiges> BuscarFormasPagosSiges();
        List<FacturaSiges> getUltimasFacturasSiges(int idCara, int cantidad);
        void actualizarVentaSubidaSicom(int ventaId);
        void CerrarTurno(Isla isla, string codigo, float totalizador);
        void AbrirTurno(Isla isla, string codigo, float totalizador);
        FacturaSiges getFacturasImprimir();
        void SetFacturaImpresa(int ventaId);
        TurnoSiges ObtenerTurnoSurtidor(int id);
        void EnviarTotalizadorCierre(int idSurtidor, int? idTurno, int idManguera, string total);
        void EnviarTotalizadorApertura(int idSurtidor, int? idTurno, int idManguera, string total);
        void ActualizarCarros(List<VehiculoSuic> vehiculos);
        TurnoSiges getTurnosSinImprimir();
        void ActualizarTurnoImpreso(int  id);
        VehiculoSuic GetVehiculoSuic(string iButton);
        Resolucion BuscarResolucionActiva(IEnumerable<Resolucion> resolucionRemota);
        void CambiarConsecutivoActual(int v);
        List<Tercero> BuscarTercerosNoEnviados();
        List<Factura> BuscarFacturasNoEnviadas();
        void ActuralizarTercerosEnviados(IEnumerable<int> enumerable);
        void ActuralizarFacturasEnviados(IEnumerable<int> enumerable);
        void ActuralizarTerceros(Tercero tercero);
        void AgregarFacturaDesdeIdVenta();
        List<Factura> BuscarFacturasNoEnviadasFacturacion();
        void ActuralizarFacturasEnviadosFacturacion(IEnumerable<int> facturas);
        List<FacturaFechaReporte> BuscarFechasReportesNoEnviadas();
        void ActuralizarFechasReportesEnviadas(IEnumerable<int> facturas);
        bool HayFacturasCanastillaPorImprimir();
        FacturaCanastilla getFacturasCanastillaImprimir();
        IEnumerable<FacturaCanastilla> BuscarFacturasNoEnviadasCanastilla();
        void ActuralizarFacturasEnviadosCanastilla(IEnumerable<int> facturas);
        FacturaCanastilla BuscarFacturaCanastillaPorConsecutivo(int consecutivo);
        void SetFacturaCanastillaImpresa(int facturasCanastillaId);
        List<FacturaSiges> BuscarFacturasNoEnviadasSiges();
        object BuscarFechasReportesNoEnviadasSiges();
        TurnoSiges ObtenerTurnoIsla(int idIsla);
        void AddFidelizado(string documento, float? puntos);
        Puntos GetVentaFidelizarAutomatica(int id);
        Fidelizado getFidelizado(string identificacion);
    }
}
