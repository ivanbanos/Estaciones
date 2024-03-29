﻿using AutoMapper;
using FacturacionelectronicaCore.Repositorio.Entities;

namespace EstacionesServicio.Negocio.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Modelo.Usuario, Repositorio.Entities.Usuario>().ReverseMap();
            CreateMap<Factura, FacturacionelectronicaCore.Negocio.Modelo.Factura>().ReverseMap();
            CreateMap<OrdenDeDespacho, FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho>();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho, OrdenDeDespacho>();
            CreateMap<Resolucion, FacturacionelectronicaCore.Negocio.Modelo.Resolucion>().ReverseMap();
            CreateMap<CreacionResolucion, FacturacionelectronicaCore.Negocio.Modelo.CreacionResolucion>().ReverseMap();
            CreateMap<Tercero, FacturacionelectronicaCore.Negocio.Modelo.Tercero>().ReverseMap();
            CreateMap<Canastilla, FacturacionelectronicaCore.Negocio.Modelo.Canastilla>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.Tercero, TerceroInput>().ReverseMap();
            CreateMap<Modelo.OrdenesDeDespachoGuids, Repositorio.Entities.OrdenesDeDespachoGuids>();
            CreateMap<Modelo.FacturasEntity, FacturasEntity>();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.Estacion, Estacion>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.FacturaFechaReporte, FacturaFechaReporte>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.FacturaCanastilla, FacturaCanastilla>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.CanastillaFactura, CanastillaFactura>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.FormasPagos, FormasPagos>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.Turno, Turno>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.TurnoSurtidor, TurnoSurtidor>().ReverseMap();
        }
    }
}
