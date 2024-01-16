﻿

using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public interface IFacturacionElectronicaFacade
    {
        public Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid);
        public Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho factura, Modelo.Tercero tercero, Guid estacionGuid);
        public Task<int> GenerarTercero(Modelo.Tercero tercero);
        Task ActualizarTercero(Modelo.Tercero t, string idFacturacion);
        Task<Item> GetItem(string name);
        Task<IEnumerable<TerceroResponse>> GetTerceros(int start);
        Task<string> GenerarFacturaElectronica(List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items);
        Task<string> GenerarFacturaElectronica(List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items);
        Task<ResponseInvoice> GetFacturaElectronica(string id);
        Task<ResolucionElectronica> GetResolucionElectronica();
    }
}
