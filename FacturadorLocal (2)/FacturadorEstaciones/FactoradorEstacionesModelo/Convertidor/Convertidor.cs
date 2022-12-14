using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SurtidorSiges = FactoradorEstacionesModelo.Siges.SurtidorSiges;

namespace FactoradorEstacionesModelo.Convertidor
{
    public class Convertidor
    {
        public IEnumerable<T> Convertir<T>(DataTable dt)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Isla> ConvertirIsla(DataTable dt)
        {
            List<Isla> response = new List<Isla>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Isla()
                {
                    idIsla = dr.Field<int>("Id"),
                    descripcion = dr.Field<string>("descripcion")
                })
            );
            return response;
        }

        public IEnumerable<Surtidor> ConvertirSurtidor(DataTable dt)
        {
            List<Surtidor> response = new List<Surtidor>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Surtidor()
                {
                    COD_SUR = dr.Field<short>("COD_SUR"),
                    MARCA = dr.Field<string>("MARCA")
                })
            );
            return response;
        }

        public IEnumerable<Cara> ConvertirCara(DataTable dt)
        {
            List<Cara> response = new List<Cara>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Cara()
                {
                    COD_CAR = dr.Field<short>("COD_CAR"),
                    POS = dr.Field<byte>("POS"),
                    DESCRIPCION = dr.Field<string>("DESCRIPCION"),
                    NUM_POS = dr.Field<short?>("NUM_POS"),
                    COD_SUR = dr.Field<short>("COD_SUR")
                })
            );
            return response;
        }

        public IEnumerable<Manguera> ConvertirManguera(DataTable dt)
        {
            List<Manguera> response = new List<Manguera>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Manguera()
                {
                    COD_MAN = dr.Field<short>("COD_MAN"),
                    COD_TANQ = dr.Field<short>("COD_TANQ"),
                    DESCRIPCION = dr.Field<string>("DESCRIPCION"),
                    DS_ROM = dr.Field<string>("DS_ROM")
                })
            );
            return response;
        }

        public IEnumerable<Venta> ConvertirVenta(DataTable dt)
        {
            List<Venta> response = new List<Venta>();

            response.AddRange(
                dt.AsEnumerable().Select(dr =>

                    CrearVenta(dr)
                )
            );
            return response;
        }

        private static Venta CrearVenta(DataRow dr)
        {
            var venta = new Venta();

            venta.CONSECUTIVO = dr.IsNull("CONSECUTIVO") ? 0 : dr.Field<int>("CONSECUTIVO");
            venta.COD_CLI = dr.IsNull("COD_CLI") ? "" : dr.Field<string>("COD_CLI");
            venta.PLACA = dr.IsNull("PLACA") ? "" : dr.Field<string>("PLACA");
            venta.KILOMETRAJE = dr.IsNull("KIL_ACT") ? 0 : dr.Field<decimal?>("KIL_ACT");
            venta.CANTIDAD = dr.IsNull("CANTIDAD") ? 0 : dr.Field<decimal>("CANTIDAD");
            venta.PRECIO_UNI = dr.IsNull("PRECIO_UNI") ? 0 : dr.Field<decimal>("PRECIO_UNI");
            venta.IVA = dr.IsNull("IVA") ? 0 : dr.Field<int>("IVA");
            venta.SUBTOTAL = dr.IsNull("SUBTOTAL") ? 0 : dr.Field<decimal>("SUBTOTAL");
            venta.TOTAL = dr.IsNull("TOTAL") ? 0 : dr.Field<decimal>("TOTAL");
            venta.VALORNETO = dr.IsNull("VALORNETO") ? 0 : dr.Field<decimal>("VALORNETO");
            venta.NOMBRE = dr.IsNull("NOMBRE") ? "" : dr.Field<string>("NOMBRE");
            venta.TIPO_NIT = dr.IsNull("TIPO_NIT") ? "" : dr.Field<string>("TIPO_NIT");
            venta.NIT = dr.IsNull("NIT") ? "" : dr.Field<string>("NIT");
            venta.DIR_OFICINA = dr.IsNull("DIR_OFICINA") ? "" : dr.Field<string>("DIR_OFICINA");
            venta.TEL_OFICINA = dr.IsNull("TEL_OFICINA") ? "" : dr.Field<string>("TEL_OFICINA");
            venta.IMP_NOM = dr.IsNull("IMP_NOM") ? "" : dr.Field<string>("IMP_NOM");

            var type = dr["COD_CAR"].GetType().Name;
            var typesur = dr["COD_SUR"].GetType().Name;
            venta.COD_CAR = dr.Table.Columns.Contains("COD_CAR") ? (dr.IsNull("COD_CAR") ? (short)0 : Convert.ToInt16(dr["COD_CAR"])) : (short)0;
            venta.COD_SUR = dr.Table.Columns.Contains("COD_SUR") ? (dr.IsNull("COD_SUR") ? (short)0 : Convert.ToInt16(dr["COD_SUR"])) : (short)0;
            venta.COD_INT = dr.Table.Columns.Contains("COD_INT") ? (dr.IsNull("COD_INT") ? "" : dr.Field<string>("COD_INT")) : "";
            venta.COD_FOR_PAG = dr.IsNull("KIL_ACT") ? 0 : dr.Field<int>("COD_FOR_PAG");
            venta.FECH_ULT_ACTU = dr.IsNull("FECH_ULT_ACTU") ? null : dr.Field<DateTime?>("FECH_ULT_ACTU");

            venta.Combustible = dr.IsNull("DESCRIPCION") ? "" : dr.Field<string>("DESCRIPCION");
            venta.Descuento = dr.IsNull("DESCUENTO") ? 0 : dr.Field<decimal>("DESCUENTO");
            venta.EMPLEADO = dr.IsNull("VENDEDOR") ? "" : dr.Field<string>("VENDEDOR");

            var suma = Convert.ToInt32(venta.PRECIO_UNI * venta.CANTIDAD);
            var sumaTotal = Convert.ToInt32((venta.PRECIO_UNI * venta.CANTIDAD) - venta.Descuento);
            if (suma >= 1000000 && venta.VALORNETO < 1000000)
            {
                venta.VALORNETO = suma;
            }
            if (sumaTotal >= 1000000 && venta.TOTAL < 1000000)
            {
                venta.TOTAL = sumaTotal;
            }

            return venta;
        }

        private long getNumericValue(object dbValue)
        {
            if (dbValue.GetType() == typeof(int))
            {
                return (int)dbValue;
            }
            else if (dbValue.GetType() == typeof(long))
            {
                return (long)dbValue;
            }
            else if (dbValue.GetType() == typeof(short))
            {
                return (short)dbValue;
            }
            else
            {
                return 0;
            }
        }

        public IEnumerable<FormasPagos> ConvertirFormasPagos(DataTable dt)
        {
            List<FormasPagos> response = new List<FormasPagos>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new FormasPagos()
                {
                    Id = dr.Field<short>("COD_FOR_PAG"),
                    Descripcion = dr.Field<string>("DESCRIPCION")
                })
            );
            return response;
        }

        public IEnumerable<Factura> ConvertirFactura(DataTable dt)
        {
            List<Factura> response = new List<Factura>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Factura()
                {
                    facturaPOSId = dr.Field<int>("facturaPOSId"),
                    ventaId = dr.Field<int>("ventaId"),
                    Consecutivo = dr.Field<int>("CONSECUTIVO"),
                    DescripcionResolucion = dr.Field<string>("descripcionRes"),
                    Autorizacion = dr.Field<string>("autorizacion"),
                    Placa = dr.Field<string>("Placa"),
                    Kilometraje = dr.Field<string>("Kilometraje"),
                    fecha = dr.Field<DateTime>("fecha"),
                    Final = dr.Field<int>("consecutivoFinal"),
                    Inicio = dr.Field<int>("consecutivoInicio"),
                    FechaFinalResolucion = dr.Field<DateTime>("fechafinal"),
                    FechaInicioResolucion = dr.Field<DateTime>("fechaInicio"),
                    habilitada = dr.Field<bool>("habilitada"),
                    impresa = dr.Field<int>("impresa"),
                    Estado = dr.Field<string>("estado"),
                    codigoFormaPago = dr.Field<int>("codigoFormaPago"),

                    Tercero = new Tercero()
                    {
                        COD_CLI = dr.Field<string>("COD_CLI"),
                        Direccion = dr.Field<string>("direccion"),
                        Nombre = dr.Field<string>("Nombre"),
                        Telefono = dr.Field<string>("Telefono"),
                        identificacion = dr.Field<string>("identificacion"),

                        Correo = dr.Field<string>("correo"),
                        terceroId = dr.Field<int>("terceroId"),
                        tipoIdentificacion = dr.Field<int?>("tipoIdentificacion"),
                        tipoIdentificacionS = dr.Field<string>("descripcion"),
                    },
                })
            );
            return response;
        }

        public IEnumerable<Tercero> ConvertirTercero(DataTable dt)
        {
            List<Tercero> response = new List<Tercero>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Tercero()
                {
                    COD_CLI = dr.Field<string>("COD_CLI"),
                    Direccion = dr.Field<string>("direccion"),
                    Nombre = dr.Field<string>("Nombre"),
                    Telefono = dr.Field<string>("Telefono"),
                    identificacion = dr.Field<string>("identificacion"),

                    Correo = dr.Field<string>("correo"),
                    terceroId = dr.Field<int>("terceroId"),
                    tipoIdentificacion = dr.Field<int?>("tipoIdentificacion"),
                    tipoIdentificacionS = dr.Field<string>("descripcion"),
                })
            );
            return response;
        }

        public IEnumerable<TipoIdentificacion> ConvertirTipoIdentificacion(DataTable dt)
        {
            List<TipoIdentificacion> response = new List<TipoIdentificacion>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new TipoIdentificacion()
                {
                    CodigoDian = dr.Field<short>("CodigoDian"),
                    Descripcion = dr.Field<string>("Descripcion"),
                    TipoIdentificacionId = dr.Field<int>("TipoIdentificacionId"),
                })
            );
            return response;
        }

        public List<string> ConvertirIds(DataTable dt)
        {
            List<string> response = new List<string>();
            return dt.AsEnumerable().Select(dr => dr.Field<string>("consecutivo")).ToList();
        }

        public List<int> ConvertirIntIds(DataTable dt1)
        {
            List<int> response = new List<int>();
            return dt1.AsEnumerable().Select(dr => dr.Field<int>("ventaId")).ToList();
        }

        public List<SurtidorSiges> ConvertirSurtidoresSiges(DataTable dt)
        {
            List<SurtidorSiges> response = new List<SurtidorSiges>();
            foreach(var dr in dt.AsEnumerable())
            {
                if (!response.Any(x => x.Numero == dr.Field<int>("Numero")))
                {
                    response.Add(new SurtidorSiges()
                    {
                        caras = new List<CaraSiges>(),
                        Descripcion = dr.Field<string>("Surtidor"),
                        Id = dr.Field<int>("IdSurtidor"),
                        Puerto = dr.Field<string>("puerto"),
                        Numero = dr.Field<int>("Numero"),
                    });
                }
                response.Find(x=>x.Numero == dr.Field<int>("Numero")).caras.Add(
                new CaraSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Descripcion = dr.Field<string>("descripcion"),
                    Isla = dr.Field<string>("Isla")
                });
            }
            return response;
        }

        public List<MangueraSiges> ConvertirManguerasSiges(DataTable dt)
        {
            List<MangueraSiges> response = new List<MangueraSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new MangueraSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Descripcion = dr.Field<string>("descripcion"),
                    Ubicacion = dr.Field<string>("ubicacion")
                })
            );
            return response;
        }

        public List<CaraSiges> ConvertirCarasSiges(DataTable dt)
        {
            List<CaraSiges> response = new List<CaraSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new CaraSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Descripcion = dr.Field<string>("descripcion")
                });
            }
            return response;
        }

        public List<FormaPagoSiges> ConvertirFormaPagoSiges(DataTable dt)
        {
            List<FormaPagoSiges> response = new List<FormaPagoSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new FormaPagoSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Descripcion = dr.Field<string>("descripcion")
                });
            }
            return response;
        }

        public List<FacturaSiges> ConvertirFacturasSiges(DataTable dt)
        {
            List<FacturaSiges> response = new List<FacturaSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new FacturaSiges()
                {
                    facturaPOSId = dr.Field<int>("facturaPOSId"),
                    ventaId = dr.Field<int>("ventaId"),
                    Consecutivo = dr.Field<int>("CONSECUTIVO"),
                    DescripcionResolucion = dr.Field<string>("descripcionRes"),
                    Autorizacion = dr.Field<string>("autorizacion"),
                    Placa = dr.Field<string>("Placa"),
                    Kilometraje = dr.Field<string>("Kilometraje"),
                    fecha = dr.Field<DateTime>("fecha"),
                    Final = dr.Field<int>("consecutivoFinal"),
                    Inicio = dr.Field<int>("consecutivoInicio"),
                    FechaFinalResolucion = dr.Field<DateTime>("fechafinal"),
                    FechaInicioResolucion = dr.Field<DateTime>("fechaInicio"),
                    habilitada = dr.Field<bool>("habilitada"),
                    impresa = dr.Field<int>("impresa"),
                    Estado = dr.Field<string>("estado"),
                    codigoFormaPago = dr.Field<int>("codigoFormaPago"),
                    Combustible = dr.Field<string>("Combustible"),
                    Surtidor = dr.Field<string>("Surtidor"),
                    Cara = dr.Field<string>("Cara"),
                    Mangueras = dr.Field<string>("Manguera"),
                    Cantidad = dr.Field<double>("cantidad"),
                    Precio = dr.Field<double>("precio"),
                    Total = dr.Field<double>("total"),
                    Subtotal = dr.Field<double>("subtotal"),
                    Descuento = dr.Field<double>("descuento"),

                    Tercero = new Tercero()
                    {
                        COD_CLI = dr.Field<string>("COD_CLI"),
                        Direccion = dr.Field<string>("direccion"),
                        Nombre = dr.Field<string>("Nombre"),
                        Telefono = dr.Field<string>("Telefono"),
                        identificacion = dr.Field<string>("identificacion"),

                        Correo = dr.Field<string>("correo"),
                        terceroId = dr.Field<int>("terceroId"),
                        tipoIdentificacion = dr.Field<int?>("tipoIdentificacion"),
                        tipoIdentificacionS = dr.Field<string>("descripcion"),
                    },
                })
            );
            return response;
        }

        public IEnumerable<TurnoSiges> ConvertirTurnoSiges(DataTable dt)
        {
            List<TurnoSiges> response = new List<TurnoSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new TurnoSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Empleado = dr.Field<string>("Nombre"),
                    IdEstado = dr.Field<int>("IdEstado")
                });
            }
            return response;
        }

        public List<FacturaSiges> ConvertirVentasSiges(DataTable dt)
        {
            List<FacturaSiges> response = new List<FacturaSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new FacturaSiges()
                {
                    ventaId = dr.Field<int>("Id"),

                    Cantidad = dr.Field<double>("cantidad"),


                    IButton = dr.Field<string>("Ibutton"),
                })
            );
            return response;
        }
    }
}
