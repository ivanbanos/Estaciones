﻿using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesServicio.Repositorio.Repositorios;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EstacionesServicio.Respositorio.Extention
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRespositoryDependencies(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            services.AddTransient<ISQLHelper>(s => new SQLHelper(configuration.GetConnectionString("ConnStr")));
            services.AddScoped<IUsuarioRespositorio, UsuarioRepositorio>();
            services.AddScoped<IFacturasRepository, FacturasRepository>();
            services.AddScoped<ITerceroRepositorio, TerceroRepositorio>();
            services.AddScoped<IOrdenDeDespachoRepositorio, OrdenDeDespachoRepositorio>();
            services.AddScoped<IFacturasRepository, FacturasRepository>();
            services.AddScoped<IResolucionRepositorio, ResolucionRepositorio>();
            services.AddScoped<ITipoIdentificacionRepositorio, TipoIdentificacionRepositorio>();
            services.AddScoped<IEstacionesRepository, EstacionesRepository>();
            services.AddScoped<ICanastillaRepositorio, CanastillaRepositorio>();
            services.AddScoped<IFacturaCanastillaRepository, FacturaCanastillaRepository>();
            return services;
        }
    }
}