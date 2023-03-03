using EstacionesServicio.Negocio.Usuario;
using EstacionesServicio.Respositorio.Extention;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturacionelectronicaCore.Negocio.Estacion;
using FacturacionelectronicaCore.Negocio.Factura;
using FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal;
using FacturacionelectronicaCore.Negocio.OrdenDeDespacho;
using FacturacionelectronicaCore.Negocio.Resolucion;
using FacturacionelectronicaCore.Negocio.Tercero;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EstacionesServicio.Negocio.Extention
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServicesDependencies(
          this IServiceCollection services,
          IConfiguration configuration)
        {

            services.AddScoped<IUsuarioNegocio, UsuarioNegocio>();
            services.AddScoped<IFacturaNegocio, FacturaNegocio>();
            services.AddScoped<ITerceroNegocio, TerceroNegocio>();
            services.AddScoped<IManejadorInformacionLocalNegocio, ManejadorInformacionLocalNegocio>();
            services.AddScoped<IOrdenDeDespachoNegocio, OrdenDeDespachoNegocio>();
            services.AddScoped<IApiContabilidad, ApiContabilidad>();
            services.AddScoped<IEstacionNegocio, EstacionNegocio>();
            services.AddScoped<IResolucionNegocio, ResolucionNegocio>();
            services.AddRespositoryDependencies(configuration);
            services.AddSingleton<IValidadorGuidAFacturaElectronica, ValidadorGuidAFacturaElectronica>();
            return services;
        }
    }
}