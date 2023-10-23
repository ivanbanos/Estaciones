using FacturadorAPI.Application.Queries.Reportes.Objetos;
using FacturadorEstacionesRepositorio;
using Modelo;
using System.Text;

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

            var reporteText = new StringBuilder();
            if (tipoReporte == "lecturas")
            {
                reporteText.Append($"<h2>{_infoEstacion.Razon}</h2>").AppendLine();
                reporteText.Append($"<h2>NIT {_infoEstacion.NIT}</h2>").AppendLine();
                reporteText.Append($"<b>Reporte de Control de Lecturas de Turnos<b>").AppendLine();
                reporteText.Append($"<b>Entre las fechas desde {dateTimePicker1.Value} hasta {dateTimePicker2.Value}<b><br />").AppendLine();
                var reporteLecturasGeneralResponse = new ReporteLecturasGeneralResponse(_infoEstacion.Nombre, _infoEstacion.NIT);
                var turnos = _estacionesRepositorio.GetTurnosByFechas(this.dateTimePicker1.Value, this.dateTimePicker2.Value);


                reporteText.Append($"<table><tr><th>Fecha</th><th>Turno</th><th>Isla</th><th>Manguera</th><th>Articulo</th><th>Lectura inicial</th><th>Lectura final</th><th>Diferencia</th><th>Ventas</th></tr>").AppendLine();
                foreach (var turno in turnos)
                {
                    var turnoinfo = _estacionesRepositorio.ObtenerTurnoSurtidor(turno.Id);
                    foreach (var turnosurtidor in turnoinfo.turnoSurtidores)
                    {
                        reporteText.Append($"<tr>").AppendLine();
                        reporteText.Append($"<td>{turno.FechaApertura}</td>").AppendLine();
                        reporteText.Append($"<td>{turno.Id}</td>").AppendLine();
                        reporteText.Append($"<td>{turno.Isla}</td>").AppendLine();
                        reporteText.Append($"<td>{turnosurtidor.Manguera.Descripcion}</td>").AppendLine();
                        reporteText.Append($"<td>{turnosurtidor.Combustible.Descripcion}</td>").AppendLine();
                        reporteText.Append($"<td>{turnosurtidor.Apertura}</td>").AppendLine();
                        reporteText.Append($"<td>{turnosurtidor.Cierre??0}</td>").AppendLine();
                        reporteText.Append($"<td>{turnosurtidor.Apertura - turnosurtidor.Cierre.Value}</td>").AppendLine();
                        reporteText.Append($"<td>{turnosurtidor.Apertura - turnosurtidor.Cierre.Value}</td>").AppendLine();


                        reporteText.Append($"</tr>").AppendLine();
                    }
                    reporteText.Append($"</table>").AppendLine();

                }
            } else
            {
                var reporteArticuloResponse = new ReporteArticuloResponse(_infoEstacion.Nombre, _infoEstacion.NIT);
                var facturas = _estacionesRepositorio.GetFacturasPorFechas(this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                var formasPago = _estacionesRepositorio.BuscarFormasPagosSiges();
                var groupForma = facturas.GroupBy(x => x.codigoFormaPago);


                reporteText.Append($"<h2>{_infoEstacion.Razon}</h2>").AppendLine();
                reporteText.Append($"<h2>NIT {_infoEstacion.NIT}</h2>").AppendLine();
                reporteText.Append($"<b>Reporte de Ventas por Articulo Resumido<b>").AppendLine();
                reporteText.Append($"<b>Entre las fechas desde {dateTimePicker1.Value} hasta {dateTimePicker2.Value}<b><br />").AppendLine();


                var groupArticulo = facturas.GroupBy(x => x.Combustible);

                reporteText.Append($"<table>").AppendLine();
                reporteText.Append($"<tr><th>Articulo</th><th>No Ventas</th><th>Cantidad</th><th>Valor Neto</th><th>Subtotal</th><th>Descuento</th><th>Recaudo</th><th>Total</th></tr>").AppendLine();
                foreach (var articulo in groupArticulo)
                {
                    reporteText.Append($"<tr>").AppendLine();
                    reporteText.Append($"<td>{articulo.Key}</td>").AppendLine();
                    reporteText.Append($"<td>{articulo.Count()}</td>").AppendLine();
                    reporteText.Append($"<td>${articulo.Sum(x => x.Subtotal)}</td>").AppendLine();
                    reporteText.Append($"<td>${articulo.Sum(x => x.Subtotal)}</td>").AppendLine();
                    reporteText.Append($"<td>${articulo.Sum(x => x.Descuento)}</td>").AppendLine();
                    reporteText.Append($"<td>$0.00</td>").AppendLine();
                    reporteText.Append($"<td>${articulo.Sum(x => x.Total)}</td>").AppendLine();

                    reporteText.Append($"</tr>").AppendLine();
                    
                }
                reporteText.Append($"</table>").AppendLine();


                reporteText.Append($"<b>Reporte de Formas de Pagos<b>").AppendLine();
                reporteText.Append($"<table>").AppendLine();
                reporteText.Append($"<tr><th>Código forma de pago</th><th>No Ventas</th><th>Cantidad</th><th>Total</th></tr>").AppendLine();
                foreach (var forma in groupForma)
                {
                    if (formasPago.Any(x => x.Id == forma.Key))
                    {

                        reporteText.Append($"<tr>").AppendLine();
                        reporteText.Append($"<td>{formasPago.First(x => x.Id == forma.Key).Id} {formasPago.First(x => x.Id == forma.Key).Descripcion}</td>").AppendLine();
                        reporteText.Append($"<td>{forma.Count()}</td>").AppendLine();
                        reporteText.Append($"<td>${forma.Sum(x => x.Cantidad)}</td>").AppendLine();
                        reporteText.Append($"<td>${forma.Sum(x => x.Total)}</td>").AppendLine();

                        reporteText.Append($"</tr>").AppendLine();
                    }
                }
                reporteText.Append($"</table>").AppendLine();
                ChromePdfRenderer renderer = new ChromePdfRenderer();
                PdfDocument pdf = renderer.RenderHtmlAsPdf(reporteText.ToString());
                pdf.SaveAs($"{_infoEstacion.Reportes}/repore-{tipoReporte}.pdf");
                MessageBox.Show("Reporte generado con exito");
                this.Close();
            }
        }
    }
}
