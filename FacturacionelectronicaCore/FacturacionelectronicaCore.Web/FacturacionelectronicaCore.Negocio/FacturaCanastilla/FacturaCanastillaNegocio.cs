﻿using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.FacturaCanastillaNegocio
{
    public class FacturaCanastillaNegocio : IFacturaCanastillaNegocio
    {
        private readonly IFacturaCanastillaRepository _facturaCanastillaRepository;
        private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;

        public FacturaCanastillaNegocio(IFacturaCanastillaRepository facturaCanastillaRepository, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica)
        {
            _facturaCanastillaRepository = facturaCanastillaRepository;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
        }
        public async Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(string idFactura)
        {
            return await _facturaCanastillaRepository.GetDetalleFactura(idFactura);
        }

        public async Task<FacturasCanastillaResponse> GetFactura(string idFactura)
        {
            return (await _facturaCanastillaRepository.GetFactura(idFactura)).FirstOrDefault();
        }

        public async Task<IEnumerable<FacturasCanastillaResponse>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion)
        {
            return await _facturaCanastillaRepository.GetFacturas(fechaInicial, fechaFinal, identificacionTercero, nombreTercero, estacion);
        }


        public void ColocarEspera(string guid, Guid idEstacion)
        {
            _validadorGuidAFacturaElectronica.AgregarAColaImpresionCanastilla(guid, idEstacion);
        }
        public async Task<int> ObtenerParaImprimir(Guid idEstacion)
        {
            var guid = _validadorGuidAFacturaElectronica.ObtenerColaImpresionCanastilla(idEstacion);
            if (!string.IsNullOrEmpty(guid))
            {
                var factura = await GetFactura(guid);
                factura.facturaCanastillaDetalles = await GetDetalleFactura(guid);

                return factura.consecutivo;
            }
            return 0;
        }

        public async Task<FacturaCanastillaReporte> GetFacturasReporte(DateTime? fechaInicial, DateTime? fechaFinal, string identificacion, string nombreTercero, Guid estacion)
        {
            var facturas = await _facturaCanastillaRepository.GetFacturas(fechaInicial, fechaFinal, identificacion, nombreTercero, estacion);
            var detalleArticulos = new List<DetalleArticulo>();
            var cantidad = 0;
            var subtotal = 0d;
            var iva = 0d;
            var total = 0d;
            foreach (var factura in facturas)
            {
                factura.facturaCanastillaDetalles = await GetDetalleFactura(factura.Guid.ToString());
                foreach (var detalle in factura.facturaCanastillaDetalles)
                {

                    if (!detalleArticulos.Any(x => x.Descripcion == detalle.descripcion))
                    {
                        detalleArticulos.Add(new DetalleArticulo() { Descripcion = detalle.descripcion, Cantidad=0, Subtotal = 0, Iva = 0, Total = 0 });
                    }
                    detalleArticulos.First(x => x.Descripcion == detalle.descripcion).Cantidad += 1;
                    detalleArticulos.First(x => x.Descripcion == detalle.descripcion).Subtotal += detalle.subtotal;
                    detalleArticulos.First(x => x.Descripcion == detalle.descripcion).Iva += detalle.iva;
                    detalleArticulos.First(x => x.Descripcion == detalle.descripcion).Total += detalle.total;
                    cantidad += 1;
                    subtotal += detalle.subtotal;
                    iva += detalle.iva;
                    total += detalle.total;
                }
            }
            detalleArticulos.Add(new DetalleArticulo() { Descripcion = "Total", Cantidad = cantidad, Subtotal = subtotal, Iva = iva, Total = total });

            var facturaCanastillaReporte = new FacturaCanastillaReporte();
            facturaCanastillaReporte.Facturas = facturas;
            facturaCanastillaReporte.DetalleArticulo = detalleArticulos;
            List<DetalleFormaPago> formas = facturas.GroupBy(x => x.FormaDePago)
                .Select(x => new DetalleFormaPago()
                {
                    FormaDePago = x.Key,
                    Cantidad = x.Count(),
                    Precio = x.Sum(x => x.total),
                }).ToList();
            formas.Add(new DetalleFormaPago() { FormaDePago="Total", Cantidad = cantidad , Precio = total });
            facturaCanastillaReporte.DetalleFormaPago = formas;
            return facturaCanastillaReporte;
        }
    }
}
