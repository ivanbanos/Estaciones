using FactoradorEstacionesModelo.Convertidor;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public class EstacionesRepositorioSqlServer : IEstacionesRepositorio
    {
        private readonly ILogger<EstacionesRepositorioSqlServer> _logger;

        private readonly ConnectionStrings _connectionString;

        private readonly Convertidor _convertidor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="estacionesRepositorio"></param>
        public EstacionesRepositorioSqlServer(ILogger<EstacionesRepositorioSqlServer> logger,
            IOptions<ConnectionStrings> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _convertidor = new Convertidor();
            _connectionString = options.Value;
        }


        public List<Isla> getIslas()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerIslas",
                new Dictionary<string, object>
                {

                });

            return _convertidor.ConvertirIsla(dt).ToList();
        }

        public List<Surtidor> getSurtidores(int idIsla)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerSurtidores",
                new Dictionary<string, object>
                {
                    {"@COD_ISL", idIsla }
                });

            return _convertidor.ConvertirSurtidor(dt).ToList();
        }


        public List<Cara> getCaras()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerCaras",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirCara(dt).ToList();
        }


        public List<FormasPagos> BuscarFormasPagos()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerFormasPago",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirFormasPagos(dt).ToList();
        }

        public Tercero getTercero(string identificacion)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerTercero",
                   new Dictionary<string, object>
                   {
                    {"@identificacion", identificacion }
                   });

            return _convertidor.ConvertirTercero(dt).FirstOrDefault();
        }

        public Tercero crearTercero(int terceroId, TipoIdentificacion tipoIdentificacion, string identificacion, string nombre, string telefono, string correo,
            string direccion,
            string COD_CLI)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CrearTercero",
                      new Dictionary<string, object>
                      {
                    {"@terceroId", terceroId },
                    {"@tipoIdentificacion", tipoIdentificacion.TipoIdentificacionId },
                    {"@identificacion", identificacion },
                    {"@nombre", nombre },
                    {"@telefono", telefono },
                    {"@correo", correo },
                    {"@direccion", direccion },
                    {"@estado", "AC" },
                    {"@COD_CLI", COD_CLI },
                      });
            return _convertidor.ConvertirTercero(dt).FirstOrDefault();
        }

        public List<TipoIdentificacion> getTiposIdentifiaciones()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerTipoIdentificaciones",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirTipoIdentificacion(dt).ToList();
        }



        public virtual async Task<DataTable> LoadDataTableFromStoredProcAsync(string procName, IDictionary<string, object> parameters, int? timeout = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString.estacion))
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                try
                {
                    if (timeout != null)
                    {
                        cmd.CommandTimeout = timeout.Value;
                    }

                    SetParameters(cmd, parameters);

                    await conn.OpenAsync();

                    using (IDataReader reader = await ExecuteActionWithLogAsync<IDataReader>(cmd, async c => await c.ExecuteReaderAsync()))
                    {
                        dt.Load(reader, LoadOption.OverwriteChanges);
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"LoadDataTableFromStoredProcAsync: Proc '{procName}' with exception. {e.Message} ");
                    throw;
                }
            }

            return dt;
        }

        public virtual DataTable LoadDataTableFromStoredProc(string connectionString, string procName, IDictionary<string, object> parameters, int? timeout = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                try
                {
                    if (timeout != null)
                    {
                        cmd.CommandTimeout = timeout.Value;
                    }

                    SetParameters(cmd, parameters);

                    conn.Open();

                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader, LoadOption.OverwriteChanges);
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"LoadDataTableFromStoredProcAsync: Proc '{procName}' with exception. {e.Message} ");
                    throw;
                }
            }

            return dt;
        }

        private SqlCommand GetSqlStoredProcCommand(string commandText, SqlConnection conn)
        {
            SqlCommand command = new SqlCommand(commandText, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            return command;
        }

        private void SetParameters(SqlCommand cmd, IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    if (param.Value is SqlParameter)
                    {
                        cmd.Parameters.Add(param.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
            }
        }

        protected virtual async Task<T> ExecuteActionWithLogAsync<T>(SqlCommand cmd, Func<SqlCommand, Task<T>> action)
        {
                T retVal = await action(cmd);
                return retVal;
        }

        public List<Factura> getUltimasFacturas(short cOD_CAR, int v)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerVentaPorCara",
                         new Dictionary<string, object>{

                    {"@COD_CAR", cOD_CAR }
                         });

            var venta = _convertidor.ConvertirVenta(dt).FirstOrDefault();
            if(venta == null)
            {
                return new List<Factura>() { new Factura() { Venta = new Venta() { CONSECUTIVO = -1 } } };
            }
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerFacturaPorVenta",
                         new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                         });
            var factura = _convertidor.ConvertirFactura(dt2).FirstOrDefault();
            if(factura == null)
            {
                DataTable dt3 = LoadDataTableFromStoredProc(_connectionString.estacion, "AgregarFacturaPorIdVenta",
                         new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                         });
                dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerFacturaPorVenta",
                         new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                         });
                factura = _convertidor.ConvertirFactura(dt2).FirstOrDefault();
            }
            if (factura == null)
            {
                return new List<Factura>();
            }
            factura.Manguera = _convertidor.ConvertirManguera(dt).FirstOrDefault();
            factura.Venta = venta;
            return new List<Factura>() { factura };
        }

        public void ActualizarFactura(int facturaPOSId, string placa, string kilometraje, int formaPago, int terceroId, int ventaId)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActualizarFactura",
                            new Dictionary<string, object>{

                    {"@facturaPOSId", facturaPOSId },

                    {"@Placa", placa },

                    {"@Kilometraje", kilometraje },

                    {"@codigoFormaPago", formaPago },

                    {"@terceroId", terceroId },
                                {"@ventaId", ventaId }
                            });
        }

        public void MandarImprimir(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "MandarImprimir",
                            new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                            });
        }
        public void enviarFacturacionSiigo(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "enviarFacturacionSiigo",
                            new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                            });
        }

        public void ConvertirAFactura(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "ConvertirAFactura",
                               new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                               });
        }
        public void ConvertirAOrden(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "ConvertirAOrden",
                               new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                               });
        }


        public List<MangueraSiges> GetMangueras(int id)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerMangueras",
                   new Dictionary<string, object>
                   {{"@IdCara", id }
                   });

            return _convertidor.ConvertirManguerasSiges(dt);
        }

        public void AgregarVenta(int idManguera, string cantidadventa, string iButton)
        {
            LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "AgregarVenta",
                               new Dictionary<string, object>{

                    {"@IdManguera", idManguera },
                    {"@cantidad", cantidadventa },
                    {"@IButton", iButton }
                               });
        }

        List<CaraSiges> IEstacionesRepositorio.GetCarasSiges()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerCaras",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirCarasSiges(dt);
        }

        public List<SurtidorSiges> GetSurtidoresSiges()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerSurtidores",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirSurtidoresSiges(dt);
        }

        public List<FormaPagoSiges> BuscarFormasPagosSiges()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "BuscarFormasPagosSiges",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirFormaPagoSiges(dt);
        }

        public List<FacturaSiges> getUltimasFacturasSiges(int idCara, int cantidad)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerFacturaPorVenta",
                            new Dictionary<string, object>{

                    {"@idCara", idCara }
                            });
            return _convertidor.ConvertirFacturasSiges(dt);
        }

        public FacturaSiges getFacturasImprimir()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaImprimir",
                         parameters);
            var facturas = _convertidor.ConvertirFacturasSiges(dt2);
            return facturas.FirstOrDefault();
        }

        public void SetFacturaImpresa(int ventaId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@ventaid",ventaId }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetFacturaImpresa",
                         parameters);
        }

        public void CerrarTurno(Isla isla, string codigo, float totalizador)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@codigo",codigo },
                {"@IdIsla",isla.idIsla }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CerrarTurno",
                         parameters);
        }

        public void AbrirTurno(Isla isla, string codigo, float totalizador)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@codigo",codigo },
                {"@IdIsla",isla.idIsla }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "AbrirTurno",
                         parameters);
        }

        public TurnoSiges ObtenerTurnoSurtidor(int id)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerTurnoSurtidor",
                            new Dictionary<string, object>{

                    {"@idSurtidor", id }
                            });
            return _convertidor.ConvertirTurnoSiges(dt).FirstOrDefault();
        }

        public void EnviarTotalizadorCierre(int idSurtidor, int? idTurno, int idManguera, string total)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@idSurtidor",idSurtidor },
                {"@IdTurno",idTurno },
                {"@idManguera",idManguera },
                {"@totalizador",total }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetTotalizadorTurnoSurtidorCierrre",
                         parameters);
        }

        public void EnviarTotalizadorApertura(int idSurtidor, int? idTurno, int idManguera, string total)
        {
            var parameters = new Dictionary<string, object>
            {
                 {"@idSurtidor",idSurtidor },
                {"@IdTurno",idTurno },
                {"@idManguera",idManguera },
                {"@totalizador",total }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetTotalizadorTurnoSurtidorApertura",
                         parameters);
        }

        public List<FacturaSiges> getVentaSinSubirSICOM()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetVentasSinSubir",
                         parameters);
            var facturas = _convertidor.ConvertirVentasSiges(dt2);
            return facturas;
        }

        public void actualizarVentaSubidaSicom(int ventaId)
        {
            var parameters = new Dictionary<string, object>
            {
                 {"@Id",ventaId },
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActualizarVentaSubidaSicom",
                         parameters);
        }
    }
}
