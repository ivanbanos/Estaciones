using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Convertidor;
using FactoradorEstacionesModelo.Objetos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public class EstacionesRepositorioSqlServer : IEstacionesRepositorio
    { 
        private readonly ConnectionStrings _connectionString;

        private Convertidor _convertidor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="estacionesRepositorio"></param>
        public EstacionesRepositorioSqlServer()
        {
            _convertidor = new Convertidor();
            _connectionString = new ConnectionStrings()
            {
                estacion = ConfigurationManager.ConnectionStrings["estacion"].ConnectionString,
                Facturacion = ConfigurationManager.ConnectionStrings["Facturacion"].ConnectionString
            };
        }

        public List<Isla> getIslas()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerIslas",
                new Dictionary<string, object>
                {

                });

            return _convertidor.ConvertirIsla(dt).ToList();
        }

        public List<Cara> getCaras()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerCaras",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirCara(dt).ToList();
        }

        public Tercero getTercero(int tipoIdeintificacion, string nombre)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerTercero",
                   new Dictionary<string, object>
                   {
                    {"@tipoIdentificacion", tipoIdeintificacion },
                    {"@identificacion", nombre }
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
            venta.COD_CAR = cOD_CAR;
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
                                {"@ventaId", ventaId}
                            });
        }

        public List<Factura> GetFacturas(DateTime fechaInicio, DateTime fechaFinal, string nombre, string identificacion)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerVentasPorfechasYCliente",
                        new Dictionary<string, object>{

                    {"@fechaInicio", fechaInicio },
                    {"@fechaFinal", fechaFinal }
                        });

            var ventas = _convertidor.ConvertirVenta(dt);

            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var venta in ventas)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = venta.CONSECUTIVO;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@ventas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerFacturasPorVentas",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);
            foreach(Factura factura in facturas)
            {
                factura.Venta = ventas.FirstOrDefault(x => x.CONSECUTIVO == factura.ventaId);
            }
            return facturas.Where(x => (string.IsNullOrEmpty(nombre) || (x.Venta.NOMBRE!=null && x.Venta.NOMBRE.Contains(nombre))) && (string.IsNullOrEmpty(identificacion) || (x.Venta.NIT!=null&&x.Venta.NIT.Contains(identificacion)))).ToList();
        }

        public List<FormasPagos> BuscarFormasPagos()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerFormasPago",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirFormasPagos(dt).ToList();
        }

        public void MandarImprimir(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "MandarImprimir",
                            new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                            });
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
        public void ActuralizarFacturasEnviados(List<int> facturas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in facturas)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@facturas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CambiarEstadoFactursEnviada",
                         parameters);
        }

        public IEnumerable<Canastilla> GetCanastillas()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetCanastilla",
                         parameters);


            return _convertidor.ConvertirCanastilla(dt);
        }

        public int GenerarFacturaCanastilla(FacturaCanastilla factura, bool imprimir)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("CanastillaId", typeof(int)));
            ventasIds.Columns.Add(new DataColumn("Guid", typeof(string)));
            ventasIds.Columns.Add(new DataColumn("descripcion", typeof(string))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("unidad", typeof(string))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("precio", typeof(float))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("deleted", typeof(bool))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("iva", typeof(int))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("cantidad", typeof(float)));
            foreach (var t in factura.canastillas)
            {
                var row = ventasIds.NewRow();
                row["CanastillaId"] = t.Canastilla.CanastillaId;
                row["Guid"] = t.Canastilla.guid; 
                 row["descripcion"] = t.Canastilla.descripcion;
                row["unidad"] = t.Canastilla.unidad;
                row["precio"] = t.Canastilla.precio;
                row["deleted"] = t.Canastilla.deleted;
                row["iva"] = t.iva;
                row["cantidad"] = t.cantidad;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@canastillaIds",ventasIds},

                {"@terceroId",factura.terceroId.terceroId},

                {"@COD_FOR_PAG",factura.codigoFormaPago.Id},

                {"@imprimir", imprimir},

                {"@descuento",factura.descuento}
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CrearFacturaCanastilla",
                         parameters);

            return dt.AsEnumerable().Select(dr => dr.Field<int>("facturaCanastillaId")).FirstOrDefault();
        }

        public void ActualizarCanastilla(IEnumerable<Canastilla> canastillas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("CanastillaId", typeof(int)));
            ventasIds.Columns.Add(new DataColumn("Guid", typeof(string)));
            ventasIds.Columns.Add(new DataColumn("descripcion", typeof(string))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("unidad", typeof(string))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("precio", typeof(float))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("deleted", typeof(bool))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("iva", typeof(int))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("cantidad", typeof(float)));
            foreach (var t in canastillas)
            {
                var row = ventasIds.NewRow();
                row["Guid"] = t.guid;
                row["descripcion"] = t.descripcion;
                row["unidad"] = t.unidad;
                row["precio"] = t.precio;
                row["deleted"] = t.deleted;
                row["iva"] = t.iva;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@canastillas",ventasIds }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "UpdateOrCreateCanastilla",
                         parameters);

        }



        public FacturaCanastilla BuscarFacturaCanastillaPorConsecutivo(int consecutivo)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@consecutivo",consecutivo }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "BuscarFacturaCanastillaPorConsecutivo",
                         parameters);
            var facturas = _convertidor.ConvertirFacturaCanastilla(dt);
            List<FacturaCanastilla> facturasEnviar = new List<FacturaCanastilla>();
            foreach (var factura in facturas)
            {


                var parameters2 = new Dictionary<string, object>
            {
                {"@FacturaCanastillaId",factura.FacturasCanastillaId }
            };
                DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaCanatillaDetalle",
                             parameters2);

                factura.canastillas = _convertidor.ConvertirFacturaCanastillaDEtalle(dt2);
                facturasEnviar.Add(factura);
            }
            return facturasEnviar.FirstOrDefault();
        }

        public string ObtenerCodigoInterno(string placa, string identificacion)
        {
            var parameters = new Dictionary<string, object>
            {{"@PLACA",placa },
            {"@identificacion",identificacion }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerCodigoInterno",
                         parameters);
            return dt2.AsEnumerable().Select(dr => dr.Field<string>("COD_INT")).FirstOrDefault();
        }
    }
}
