using FacturadorAPI.Application.Queries.Reportes.Objetos;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
using Modelo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControladorEstacion
{
    public partial class FormReporte : Form
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly InfoEstacion _infoEstacion;
        string tipoReporte;
        public FormReporte(string tipo, IEstacionesRepositorio estacionesRepositorio, InfoEstacion infoEstacion)
        {
            InitializeComponent();
            this.Text += tipo;
            tipoReporte = tipo;
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(tipoReporte == "lecturas")
            {
                var reporteLecturasGeneralResponse = new ReporteLecturasGeneralResponse(_infoEstacion.Nombre, _infoEstacion.NIT);
                var turnos = _estacionesRepositorio.GetTurnosByFechas(this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                foreach (var turno in turnos)
                {
                    var turnoinfo = _estacionesRepositorio.ObtenerTurnoSurtidor(turno.Id);
                    foreach (var turnosurtidor in turnoinfo.turnoSurtidores)
                    {
                        reporteLecturasGeneralResponse.reporteTurnoItem.Add(
                            new ReporteTurnoItem()
                            {
                                Combustible = turnosurtidor.Combustible.Descripcion,
                                Fecha = turno.FechaApertura,
                                IdTurno = turno.Id,
                                Isla = turno.Isla,
                                LecturaInicial = turnosurtidor.Apertura.ToString(),
                                LecturaFinal = turnosurtidor.Cierre.HasValue ? turnosurtidor.Cierre.Value.ToString() : "",
                                Manguera = turnosurtidor.Manguera.Descripcion
                            }); ;
                    }
                }
            } else
            {
                var reporteArticuloResponse = new ReporteArticuloResponse(_infoEstacion.Nombre, _infoEstacion.NIT);
                var facturas = _estacionesRepositorio.GetFacturasPorFechas(this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                var formasPago = _estacionesRepositorio.BuscarFormasPagosSiges();
                var groupForma = facturas.GroupBy(x => x.codigoFormaPago);
                foreach (var forma in groupForma)
                {
                    if (formasPago.Any(x => x.Id == forma.Key))
                    {
                        reporteArticuloResponse.PorFormas.Add(new PorFormas()
                        {
                            Cantidad = forma.Sum(x => x.Cantidad),
                            Descripcion = formasPago.First(x => x.Id == forma.Key).Descripcion,
                            Facturas = forma.Count(),
                            Total = forma.Sum(x => x.Total)
                        });
                    }
                }

                var groupArticulo = facturas.GroupBy(x => x.Combustible);
                foreach (var articulo in groupArticulo)
                {
                    reporteArticuloResponse.PorArticulo.Add(new PorArticulo()
                    {
                        Cantidad = articulo.Sum(x => x.Cantidad),
                        Descripcion = articulo.Key,
                        Facturas = articulo.Count(),
                        Neto = articulo.Sum(x => x.Subtotal),
                        Recaudo = 0,
                        Descuento = articulo.Sum(x => x.Descuento),
                        Subtotal = articulo.Sum(x => x.Subtotal),
                        Total = articulo.Sum(x => x.Total)
                    });

                }
            }
        }
    }
}
