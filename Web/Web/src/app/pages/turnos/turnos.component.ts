import { Observable } from 'rxjs';
import { FacturaService } from '../../providers/factura/factura.service';
import { BaseComponent } from '../base/base.component';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ErrorService } from 'src/app/errors/services/error.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReporteConsolidado } from 'src/app/models/ReporteConsolidado';
import { FiltroBusqueda } from 'src/app/models/filtroOrdenesDeDespacho';
import { catchError, map, tap } from 'rxjs/operators';
import pdfMake from 'pdfmake/build/pdfmake';
import pdfFonts from 'pdfmake/build/vfs_fonts';
pdfMake.vfs = pdfFonts.pdfMake.vfs;

@Component({
  selector: 'app-turnos',
  templateUrl: './turnos.component.html',
  styleUrls: ['./turnos.component.sass']
})
export class TurnosComponent extends BaseComponent implements OnInit {

  reporteConsolidado$: Observable<ReporteConsolidado>;
  FormularioReporteConsolidado: FormGroup;
  filtroBusqueda: FiltroBusqueda;
  isLoading = false;
  loaded = false;
  firstSearch = true;
  empty = false;
  columnas: string[] = ['combustible', 'cantidad', 'total'];
  columnasCliente: string[] = ['cliente', 'cantidad', 'total'];
  reporteInfo: ReporteConsolidado;
  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    private readonly facturaService: FacturaService,
    private readonly fb: FormBuilder) {
    super(router, errorService, toast);
  }
  ngOnInit(): void {
    this.FormularioReporteConsolidado = this.fb.group({
      FechaReporte: [],
      FechaFinal: []
    });
    
  }

  obtenerInformacionFormulario() {
    return new FiltroBusqueda(
      this.FormularioReporteConsolidado.value.FechaReporte != null ?
        new Date(this.FormularioReporteConsolidado.value.FechaReporte) : null,
      this.FormularioReporteConsolidado.value.FechaFinal != null ?
        new Date(this.FormularioReporteConsolidado.value.FechaFinal) : null,
      '',
      '',
      localStorage.getItem('estacion'));
  }

  onSubmit() {
    if (this.FormularioReporteConsolidado.valid) {
      this.isLoading = true;
      this.firstSearch = false;
      this.filtroBusqueda = this.obtenerInformacionFormulario();
      this.reporteConsolidado$ = this.facturaService.buscarConsolidado(this.filtroBusqueda)
      .pipe(
          tap(_ => this.isLoading = false),
          catchError(error => {
            this.isLoading = false;
            this.empty = true;
            this.handleException(error);
            return [];
          }));
    }
    this.reporteConsolidado$.subscribe(reporteConsolidado => {
      this.reporteInfo = reporteConsolidado;
   });
  }
  
  generatePdf(){
    console.log(this.reporteInfo);
    let nombreEstacion = localStorage.getItem('estacion_nombre');
    let date_ob = this.FormularioReporteConsolidado.value.FechaReporte;
    let date = ("0" + date_ob.getDate()).slice(-2);
    let month = ("0" + (date_ob.getMonth() + 1)).slice(-2);
    let year = date_ob.getFullYear();

    
    let date_obf = this.FormularioReporteConsolidado.value.FechaFinal != null ?
        this.FormularioReporteConsolidado.value.FechaFinal : this.FormularioReporteConsolidado.value.FechaReporte;
    let datef = ("0" + date_obf.getDate()).slice(-2);
    let monthf = ("0" + (date_obf.getMonth() + 1)).slice(-2);
    let yearf = date_obf.getFullYear();
    
    // prints date in YYYY-MM-DD format
    let dateTitle = "Fecha desde "+date + "-" + month + "-" + year +" hasta "+datef + "-" + monthf + "-" + yearf  ;
    let cantidadTotal=0;
    let totalTotal=0;
    let infoTabla = [
    ];
    infoTabla.push([ {text: 'Combustible', style: 'tableHeader'},{text: 'Cantidad', style: 'tableHeader'}	,	{text: 'Total', style: 'tableHeader'} ]);
    this.reporteInfo.consolidados.forEach(obj => {
      cantidadTotal+=obj.cantidad;
      totalTotal+=obj.total;
      infoTabla.push([obj.combustible, obj.cantidad.toFixed(3), "$"+obj.total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);
    });
    infoTabla.push(['Total', cantidadTotal.toFixed(3), "$"+totalTotal.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);

    cantidadTotal=0;
    totalTotal=0;
    let infoTablaConsolidadoOrdenes = [
    ];
    infoTablaConsolidadoOrdenes.push([ {text: 'Combustible', style: 'tableHeader'},{text: 'Cantidad', style: 'tableHeader'}	,	{text: 'Total', style: 'tableHeader'} ]);
    this.reporteInfo.consolidadosOrdenes.forEach(obj => {
      cantidadTotal+=obj.cantidad;
      totalTotal+=obj.total;
      infoTablaConsolidadoOrdenes.push([obj.combustible, obj.cantidad.toFixed(3), "$"+obj.total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);
    });
    infoTablaConsolidadoOrdenes.push(['Total', cantidadTotal.toFixed(3), "$"+totalTotal.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);

    cantidadTotal=0;
    totalTotal=0;
    let infoTablaConsolidadoClientes = [
    ];
    infoTablaConsolidadoClientes.push([ {text: 'Clientes', style: 'tableHeader'},{text: 'Cantidad', style: 'tableHeader'}	,	{text: 'Total', style: 'tableHeader'} ]);
    this.reporteInfo.consolidadoClienteFacturas.forEach(obj => {
      cantidadTotal+=obj.cantidad;
      totalTotal+=obj.total;
      infoTablaConsolidadoClientes.push([obj.cliente, obj.cantidad.toFixed(3), "$"+obj.total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);
    });
    infoTablaConsolidadoClientes.push(['Total', cantidadTotal.toFixed(3), "$"+totalTotal.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);

    cantidadTotal=0;
    totalTotal=0;
    let infoTablaConsolidadoCleintesOrdenes = [
    ];
    infoTablaConsolidadoCleintesOrdenes.push([ {text: 'Clientes', style: 'tableHeader'},{text: 'Cantidad', style: 'tableHeader'}	,	{text: 'Total', style: 'tableHeader'} ]);
    this.reporteInfo.consolidadoClienteOrdenes.forEach(obj => {
      cantidadTotal+=obj.cantidad;
      totalTotal+=obj.total;
      infoTablaConsolidadoCleintesOrdenes.push([obj.cliente, obj.cantidad.toFixed(3), "$"+obj.total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);
    }); 
    infoTablaConsolidadoCleintesOrdenes.push(['Total', cantidadTotal.toFixed(3), "$"+totalTotal.toFixed(3).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);

    




    cantidadTotal=0;
    totalTotal=0;
    let infoTablaFacturasAnuladas = [
    ];
    infoTablaFacturasAnuladas.push([ {text: 'Combustible', style: 'tableHeader'},{text: 'Cantidad', style: 'tableHeader'}	,	{text: 'Total', style: 'tableHeader'} ]);
    this.reporteInfo.consolidadoFacturasAnuladas.forEach(obj => {
      cantidadTotal+=obj.cantidad;
      totalTotal+=obj.total;
      infoTablaFacturasAnuladas.push([obj.combustible, obj.cantidad.toFixed(3), "$"+obj.total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);
    });
    infoTablaFacturasAnuladas.push(['Total', cantidadTotal.toFixed(3), "$"+totalTotal.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);

    cantidadTotal=0;
    totalTotal=0;
    let infoTablaOrdenesAnuladas = [
    ];
    infoTablaOrdenesAnuladas.push([ {text: 'Combustible', style: 'tableHeader'},{text: 'Cantidad', style: 'tableHeader'}	,	{text: 'Total', style: 'tableHeader'} ]);
    this.reporteInfo.consolidadoOrdenesAnuladas.forEach(obj => {
      cantidadTotal+=obj.cantidad;
      totalTotal+=obj.total;
      infoTablaOrdenesAnuladas.push([obj.combustible, obj.cantidad.toFixed(3), "$"+obj.total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);
    });
    infoTablaOrdenesAnuladas.push(['Total', cantidadTotal.toFixed(3), "$"+totalTotal.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",")]);






    
    const documentDefinition = { 
      info: {
        title: 'Turnos-'+nombreEstacion+'-'+dateTitle,
        author: 'SIGES Soluciones',
        subject: 'subject of document',
        keywords: 'keywords for document',
        },
      watermark: { text: 'SIGES Soluciones', color: 'blue', opacity: 0.1, bold: true, italics: false },
      footer: function(currentPage, pageCount) { return 'SIGES Soluciones Turnos - '+nombreEstacion+' - '+currentPage.toString() + ' of ' + pageCount; },
      content: [
        { text: 'Reporte Fiscal - '+dateTitle, style: 'header' },
        { text: localStorage.getItem('estacion_nombre'), style: 'subtitulo' },
        { text: 'Informacion general:', bold: true, margin: [0, 20, 0, 8] },
        {
          // to treat a paragraph as a bulleted list, set an array of items under the ul key
          ul: [
            'Consecutivo Inicial:'+this.reporteInfo.consecutivoFacturaInicial,
            'Consecutivo Final:'+this.reporteInfo.consecutivoDeFacturaFinal,
            'Total de ventas facturadas: '+this.reporteInfo.totalDeVentas,
            'Total de ordenes de despacho: '+this.reporteInfo.totalDeOrdenes,
            'Cantidad de facturas anuladas:'+this.reporteInfo.totalFacturasAnuladas,
            'Cantidad de ordenes anuladas:'+this.reporteInfo.totalOrdenesAnuladas,
          ]
        },
        { text: 'Consolidado de facturas por combustible', bold: true , margin: [0, 20, 0, 8]},
        {
          style: 'tableReporte',
          table: {
            widths: ['*', 'auto', 'auto'],
            // headers are automatically repeated if the table spans over multiple pages
            // you can declare how many rows should be treated as headers
            headerRows: 1,
            body: infoTabla,
            layout: {
              hLineWidth: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 2 : 1;
              },
              vLineWidth: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 2 : 1;
              },
              hLineColor: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 'black' : 'gray';
              },
              vLineColor: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 'black' : 'gray';
              },
              fillColor: function (rowIndex, node, columnIndex) {
                return (rowIndex % 2 === 0) ? '#CCCCCC' : null;
              }
            }
          }
        },
        { text: 'Consolidado de ordenes por combustible', bold: true , margin: [0, 20, 0, 8]},
        {
          style: 'tableReporte',
          table: {
            widths: ['*', 'auto', 'auto'],
            // headers are automatically repeated if the table spans over multiple pages
            // you can declare how many rows should be treated as headers
            headerRows: 1,
            body: infoTablaConsolidadoOrdenes,
            layout: {
              hLineWidth: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 2 : 1;
              },
              vLineWidth: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 2 : 1;
              },
              hLineColor: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 'black' : 'gray';
              },
              vLineColor: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 'black' : 'gray';
              },
            }
          }
        },
        { text: 'Consolidado de facturas por cliente', bold: true , margin: [0, 20, 0, 8]},
        {
          style: 'tableReporte',
          table: {
            widths: ['*', 'auto', 'auto'],
            // headers are automatically repeated if the table spans over multiple pages
            // you can declare how many rows should be treated as headers
            headerRows: 1,
            body: infoTablaConsolidadoClientes,
            layout: {
              hLineWidth: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 2 : 1;
              },
              vLineWidth: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 2 : 1;
              },
              hLineColor: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 'black' : 'gray';
              },
              vLineColor: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 'black' : 'gray';
              },
              fillColor: function (rowIndex, node, columnIndex) {
                return (rowIndex % 2 === 0) ? '#CCCCCC' : null;
              }
            }
          }
        },
        { text: 'Consolidado de ordenes por clientes', bold: true , margin: [0, 20, 0, 8]},
        {
          style: 'tableReporte',
          table: {
            widths: ['*', 'auto', 'auto'],
            // headers are automatically repeated if the table spans over multiple pages
            // you can declare how many rows should be treated as headers
            headerRows: 1,
            body: infoTablaConsolidadoCleintesOrdenes,
            layout: {
              hLineWidth: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 2 : 1;
              },
              vLineWidth: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 2 : 1;
              },
              hLineColor: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 'black' : 'gray';
              },
              vLineColor: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 'black' : 'gray';
              },
              fillColor: function (rowIndex, node, columnIndex) {
                return (rowIndex % 2 === 0) ? '#CCCCCC' : null;
              }
            }
          }
        },


        { text: 'Consolidado de facturas anuladas', bold: true , margin: [0, 20, 0, 8]},
        {
          style: 'tableReporte',
          table: {
            widths: ['*', 'auto', 'auto'],
            // headers are automatically repeated if the table spans over multiple pages
            // you can declare how many rows should be treated as headers
            headerRows: 1,
            body: infoTablaFacturasAnuladas,
            layout: {
              hLineWidth: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 2 : 1;
              },
              vLineWidth: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 2 : 1;
              },
              hLineColor: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 'black' : 'gray';
              },
              vLineColor: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 'black' : 'gray';
              },
              fillColor: function (rowIndex, node, columnIndex) {
                return (rowIndex % 2 === 0) ? '#CCCCCC' : null;
              }
            }
          }
        },
        { text: 'Consolidado de ordenes anuladas', bold: true , margin: [0, 20, 0, 8]},
        {
          style: 'tableReporte',
          table: {
            widths: ['*', 'auto', 'auto'],
            // headers are automatically repeated if the table spans over multiple pages
            // you can declare how many rows should be treated as headers
            headerRows: 1,
            body: infoTablaOrdenesAnuladas,
            layout: {
              hLineWidth: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 2 : 1;
              },
              vLineWidth: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 2 : 1;
              },
              hLineColor: function (i, node) {
                return (i === 0 || i === node.table.body.length) ? 'black' : 'gray';
              },
              vLineColor: function (i, node) {
                return (i === 0 || i === node.table.widths.length) ? 'black' : 'gray';
              },
              fillColor: function (rowIndex, node, columnIndex) {
                return (rowIndex % 2 === 0) ? '#CCCCCC' : null;
              }
            }
          }
        }
      ], 
      styles: {
        header: {
          fontSize: 22,
          bold: true,
          alignment: 'center' ,
          color:'#3f51b5'
        },
        subtitulo: {
          fontSize: 20,
          bold: true,
          alignment: 'left' ,
          color:'#3f51b5'
        },
        anotherStyle: {
          italics: true,
          alignment: 'right'
        },
        headertable: {
          fontSize: 18,
          bold: true,
          margin: [0, 0, 0, 10]
        },
        subheader: {
          fontSize: 16,
          bold: true,
          margin: [0, 10, 0, 5]
        },
        tableExample: {
          margin: [0, 5, 0, 15]
        },
        tableHeader: {
          bold: true,
          fontSize: 13,
          color: 'black'
        }
      }
  };
    pdfMake.createPdf(documentDefinition).download('Turnos - '+nombreEstacion+' - '+dateTitle+'.pdf');
   }


  
}
