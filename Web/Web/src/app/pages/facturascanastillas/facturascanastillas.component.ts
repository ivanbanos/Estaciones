import { Component, OnInit } from '@angular/core';
import { Observable, of, Subscription } from 'rxjs';
import { FormGroup, FormBuilder } from '@angular/forms';
import { FiltroBusqueda } from 'src/app/models/filtroOrdenesDeDespacho';
import { Router } from '@angular/router';
import { ErrorService } from 'src/app/errors/services/error.service';
import { ToastrService } from 'ngx-toastr';
import { BaseComponent } from '../base/base.component';
import { map, catchError, tap, finalize, switchMap, filter } from 'rxjs/operators';
import * as FileSaver from 'file-saver';
import * as XLSX from 'xlsx';
import { FacturaCanastillaUI } from 'src/app/models/UIModels/FacturaCanastillaUI';
import { FacturasCanastillasService } from 'src/app/providers/facturasCanastillas/FacturasCanastillas.service';

@Component({
  selector: 'app-facturasCanastillas',
  templateUrl: './facturascanastillas.component.html',
  styleUrls: ['./facturascanastillas.component.scss']
})
export class FacturasCanastillasComponent extends BaseComponent implements OnInit {
  
  facturas$: Observable<Array<FacturaCanastillaUI>>;
  FormularioFactura: FormGroup;
  filtroBusqueda: FiltroBusqueda;
  valorBusqueda = '';
  listFacturaInicial: Observable<Array<FacturaCanastillaUI>>;
  seleccion: Array<any> = [];
  isLoading = false;
  loaded = false;
  firstSearch = true;
  empty = false;
  consecutivoInicial = 0;
  consecutivoFinal = 0;
  totalFacturas = 0;
  totalDinero = "0";
  excelData: any[];
  EXCEL_TYPE :string = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
  EXCEL_EXTENSION:string = '.xlsx';
  
  public columnas: string[] = ['Consecutivo', 'Fecha', 'Tercero', 'Subtotal', 'Descuento', 'Iva', 'Total'];
  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    public readonly facturaService: FacturasCanastillasService,
    public readonly fb: FormBuilder) {
    super(router, errorService, toast);
  }

  ngOnInit() {

    this.FormularioFactura = this.fb.group({
      fechaInicial: [],
      fechaFinal: [],
      identificacion: [],
      nombreTercero: [],
    });
  }
  obtenerInformacionFormulario() {
    return new FiltroBusqueda(
      this.FormularioFactura.value.fechaInicial != null ?
        new Date(this.FormularioFactura.value.fechaInicial) : null,
      this.FormularioFactura.value.fechaFinal != null ?
        new Date(this.FormularioFactura.value.fechaFinal) : null,
      this.FormularioFactura.value.identificacion,
      this.FormularioFactura.value.nombreTercero,
      localStorage.getItem('estacion'));
  }

  onSubmit() {
    if (this.FormularioFactura.valid) {
      this.filtroBusqueda = this.obtenerInformacionFormulario();
      this.isLoading = true;
      this.firstSearch = false;
      this.facturas$ = this.facturaService.buscarFacturas(this.filtroBusqueda)
        .pipe(
          map(facturas => {
            this.isLoading = false;
            if (facturas.length > 0) {
              this.loaded = true;
            }
            this.empty = facturas.length === 0;
if(!this.empty){
  this.consecutivoInicial = Math.min.apply(Math, facturas.map(function(o) { return o.consecutivo; }));
  this.consecutivoFinal = Math.max.apply(Math, facturas.map(function(o) { return o.consecutivo; }));
  this.totalFacturas = facturas.length;
  this.totalDinero = facturas.map(a => a.total).reduce(function(a, b)
  {
    return a + b;
  }).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

  
const values = facturas.map(canastilla => new FacturaCanastillaUI(canastilla.consecutivo,canastilla.fecha,  canastilla.nombre, canastilla.subtotal, canastilla.descuento, canastilla.iva, canastilla.total, 
  canastilla.guid, canastilla.facturasCanastillaId,  
  canastilla.segundo, canastilla.apellidos, canastilla.identificacion, canastilla.tipoIdentificacion, canastilla.formaDePago ) 
);
              this.excelData = [];
              debugger;
              values.forEach(obj=>
                this.excelData.push({
                  Consecutivo: obj.consecutivo,
                  Fecha:obj.fecha,
                  SubTotal:obj.subtotal,
                  Iva:obj.iva,
                  Descuento:obj.descuento,
                  Total:obj.total,
                  Identificacion:obj.identificacion,
                  Tercero:obj.nombre,
                  FormaDePago:obj.formaDePago})
                );
             
            this.listFacturaInicial = of(values);

            return values;
          }),
          catchError(error => {
            this.isLoading = false;
            this.handleException(error);
            return [];
          }));
    }
  }

  
  showUpdate = (factura: FacturaCanastillaUI) => {
    this.openPage(`/dashboard/facturacanastilla/${factura.guid}`);
  }

  exportAsXLSX() {
    
    
    console.log(this.excelData);
    this.exportAsExcelFile(this.excelData, 'Reporte de facturas');
 }
 public exportAsExcelFile(json: any[], excelFileName: string): void {
  console.log(json);
  const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(json);
  console.log(worksheet);
  const workbook: XLSX.WorkBook = { Sheets: { 'facturas': worksheet }, SheetNames: ['facturas'] };
  const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
  this.saveAsExcelFile(excelBuffer, excelFileName);
}
private saveAsExcelFile(buffer: any, fileName: string): void {
   const data: Blob = new Blob([buffer], {type: this.EXCEL_TYPE});
   FileSaver.saveAs(data, fileName + '_export_' + new  Date().getTime() +  this.EXCEL_EXTENSION);
}

}
