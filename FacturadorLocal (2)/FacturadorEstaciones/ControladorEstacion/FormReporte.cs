using FactoradorEstacionesModelo.Siges;
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
            this.Text += " " + tipo;
            tipoReporte = tipo;
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var reporteText = new StringBuilder();
                if (tipoReporte == "lecturas")
                {
                    reporteText.Append($"<h2>{_infoEstacion.Razon}</h2>").AppendLine();
                    reporteText.Append($"<h2>NIT {_infoEstacion.NIT}</h2>").AppendLine();
                    reporteText.Append($"<b>Reporte de Control de Lecturas de Turnos<b>").AppendLine();
                    reporteText.Append($"<b>Desde {dateTimePicker1.Value} Hasta {dateTimePicker2.Value}<b><br />").AppendLine();
                    var reporteLecturasGeneralResponse = new ReporteLecturasGeneralResponse(_infoEstacion.Nombre, _infoEstacion.NIT);
                    var turnos = _estacionesRepositorio.GetTurnosByFechas(this.dateTimePicker1.Value.Date, this.dateTimePicker2.Value.AddDays(1).Date);


                    reporteText.Append($"<table class=\"tableReporte\"><tr><th>Fecha</th><th>Turno</th><th>Isla</th><th>Manguera</th><th>Articulo</th><th>Precio</th><th>Lectura inicial</th><th>Lectura final</th><th>Diferencia</th><th>Ventas</th></tr>").AppendLine();
                    foreach (var turno in turnos)
                    {
                        var turnoinfo = _estacionesRepositorio.ObtenerTurnoInfo(turno.Id);

                        foreach (var turnosurtidor in turnoinfo)
                        {
                            if (!turnosurtidor.Cierre.HasValue)
                            {

                                continue;
                            }
                            reporteText.Append($"<tr>").AppendLine();
                            reporteText.Append($"<td>{turno.FechaApertura}</td>").AppendLine();
                            reporteText.Append($"<td>{turno.Id}</td>").AppendLine();
                            reporteText.Append($"<td>{turno.Isla}</td>").AppendLine();
                            reporteText.Append($"<td>{turnosurtidor.Manguera.Descripcion}</td>").AppendLine();
                            reporteText.Append($"<td>{turnosurtidor.Combustible.Descripcion}</td>").AppendLine();
                            reporteText.Append($"<td>{turnosurtidor.Combustible.Precio}</td>").AppendLine();
                            reporteText.Append($"<td>{turnosurtidor.Apertura}</td>").AppendLine();
                            reporteText.Append($"<td>{turnosurtidor.Cierre ?? 0}</td>").AppendLine();
                            reporteText.Append($"<td>{turnosurtidor.Cierre.Value - turnosurtidor.Apertura}</td>").AppendLine();
                            reporteText.Append($"<td>${String.Format("{0:#,0.00}", (turnosurtidor.Cierre.Value - turnosurtidor.Apertura) * turnosurtidor.Combustible.Precio)}</td>").AppendLine();


                            reporteText.Append($"</tr>").AppendLine();
                        }

                    }
                    reporteText.Append($"</table>").AppendLine();
                }
                else
                {
                    var reporteArticuloResponse = new ReporteArticuloResponse(_infoEstacion.Nombre, _infoEstacion.NIT);
                    var facturas = _estacionesRepositorio.GetFacturasPorFechas(this.dateTimePicker1.Value.Date, this.dateTimePicker2.Value.AddDays(1).Date);
                    var formasPago = _estacionesRepositorio.BuscarFormasPagosSiges();
                    var groupForma = facturas.GroupBy(x => x.codigoFormaPago);


                    reporteText.Append($"<h2>{_infoEstacion.Razon}</h2>").AppendLine();
                    reporteText.Append($"<h2>NIT {_infoEstacion.NIT}</h2>").AppendLine();
                    reporteText.Append($"<b>Reporte de Ventas por Articulo Resumido<b>").AppendLine();
                    reporteText.Append($"<b>Desde {dateTimePicker1.Value} Hasta {dateTimePicker2.Value} <b><br />").AppendLine();


                    var groupArticulo = facturas.GroupBy(x => x.Combustible);

                    reporteText.Append($"<table class=\"tableReporte\">").AppendLine();
                    reporteText.Append($"<tr><th>Articulo</th><th>No Ventas</th><th>Cantidad</th><th>Valor Neto</th><th>Subtotal</th><th>Descuento</th><th>Recaudo</th><th>Total</th></tr>").AppendLine();
                    foreach (var articulo in groupArticulo)
                    {
                        reporteText.Append($"<tr>").AppendLine();
                        reporteText.Append($"<td>{articulo.Key}</td>").AppendLine();
                        reporteText.Append($"<td>{articulo.Count()}</td>").AppendLine();
                        reporteText.Append($"<td>{String.Format("{0:#,0.00}", articulo.Sum(x => x.Cantidad))}</td>").AppendLine();
                        reporteText.Append($"<td>${String.Format("{0:#,0.00}", articulo.Sum(x => x.Subtotal))}</td>").AppendLine();
                        reporteText.Append($"<td>${String.Format("{0:#,0.00}", articulo.Sum(x => x.Descuento))}</td>").AppendLine();
                        reporteText.Append($"<td>$0.00</td>").AppendLine();
                        reporteText.Append($"<td>${String.Format("{0:#,0.00}", articulo.Sum(x => x.Total))}</td>").AppendLine();

                        reporteText.Append($"</tr>").AppendLine();

                    }
                    reporteText.Append($"<tr>").AppendLine();
                    reporteText.Append($"<td>Total</td>").AppendLine();
                    reporteText.Append($"<td>{facturas.Count()}</td>").AppendLine();
                    reporteText.Append($"<td>{String.Format("{0:#,0.00}", facturas.Sum(x => x.Cantidad))}</td>").AppendLine();
                    reporteText.Append($"<td>${String.Format("{0:#,0.00}", facturas.Sum(x => x.Subtotal))}</td>").AppendLine();
                    reporteText.Append($"<td>${String.Format("{0:#,0.00}", facturas.Sum(x => x.Descuento))}</td>").AppendLine();
                    reporteText.Append($"<td>$0.00</td>").AppendLine();
                    reporteText.Append($"<td>${String.Format("{0:#,0.00}", facturas.Sum(x => x.Total))}</td>").AppendLine();

                    reporteText.Append($"</tr>").AppendLine();
                    reporteText.Append($"</table >").AppendLine();


                    reporteText.Append($"<b>Reporte de Formas de Pagos<b>").AppendLine();
                    reporteText.Append($"<table class=\"tableReporte\">").AppendLine();
                    reporteText.Append($"<tr><th>Código forma de pago</th><th>No Ventas</th><th>Cantidad</th><th>Total</th></tr>").AppendLine();
                    foreach (var forma in groupForma)
                    {
                        if (formasPago.Any(x => x.Id == forma.Key))
                        {

                            reporteText.Append($"<tr>").AppendLine();
                            reporteText.Append($"<td>{formasPago.First(x => x.Id == forma.Key).Id} {formasPago.First(x => x.Id == forma.Key).Descripcion}</td>").AppendLine();
                            reporteText.Append($"<td>{forma.Count()}</td>").AppendLine();
                            reporteText.Append($"<td>{String.Format("{0:#,0.00}", forma.Sum(x => x.Cantidad))}</td>").AppendLine();
                            reporteText.Append($"<td>${String.Format("{0:#,0.00}", forma.Sum(x => x.Total))}</td>").AppendLine();

                            reporteText.Append($"</tr>").AppendLine();
                        }
                    }
                    reporteText.Append($"<tr>").AppendLine();
                    reporteText.Append($"<td>Total</td>").AppendLine();
                    reporteText.Append($"<td>{facturas.Count()}</td>").AppendLine();
                    reporteText.Append($"<td>{String.Format("{0:#,0.00}", facturas.Sum(x => x.Cantidad))}</td>").AppendLine();
                    reporteText.Append($"<td>${String.Format("{0:#,0.00}", facturas.Sum(x => x.Total))}</td>").AppendLine();

                    reporteText.Append($"</tr>").AppendLine();
                    reporteText.Append($"</table>").AppendLine();

                }

                reporteText.Append($"<br /><b>Reporte generado en {DateTime.Now}, por SIGES SOLUCIONES SAS<b>").AppendLine();

                var pageReporte = File.ReadAllText("FormatoReporte.html");
                pageReporte = pageReporte.Replace("{body}", reporteText.ToString());
                ChromePdfRenderer renderer = new ChromePdfRenderer();

                PdfDocument pdf = renderer.RenderHtmlAsPdf(pageReporte);
                

                pdf.SaveAs($"{_infoEstacion.Reportes}/reporte-{tipoReporte}-{this.dateTimePicker1.Value.ToString("dd-MM-yyyy")}-{this.dateTimePicker2.Value.ToString("dd-MM-yyyy")}.pdf");
                MessageBox.Show("Reporte generado con exito");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generando reporte. Por favor, comunicarse con asistencia.{ex.Message},{ex.StackTrace}");
                this.Close();

            }

        }
    }
}
