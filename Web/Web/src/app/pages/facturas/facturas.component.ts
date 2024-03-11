import { Component, OnInit } from '@angular/core';
import { Observable, of, Subscription } from 'rxjs';
import { FacturaUI } from 'src/app/models/UIModels/FacturaUI';
import { FormGroup, FormBuilder } from '@angular/forms';
import { FiltroBusqueda } from 'src/app/models/filtroOrdenesDeDespacho';
import { Router } from '@angular/router';
import { ErrorService } from 'src/app/errors/services/error.service';
import { ToastrService } from 'ngx-toastr';
import { FacturaService } from 'src/app/providers/factura/factura.service';
import { BaseComponent } from '../base/base.component';
import { map, catchError, tap, finalize, switchMap, filter } from 'rxjs/operators';
import * as FileSaver from 'file-saver';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-facturas',
  templateUrl: './facturas.component.html',
  styleUrls: ['./facturas.component.sass']
})
export class FacturasComponent extends BaseComponent implements OnInit {

  facturas$: Observable<Array<FacturaUI>>;
  FormularioFactura: FormGroup;
  filtroBusqueda: FiltroBusqueda;
  valorBusqueda = '';
  listFacturaInicial: Observable<Array<FacturaUI>>;
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
  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    public readonly facturaService: FacturaService,
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

  
            const values = facturas.map(factura => new FacturaUI(
              factura.consecutivo,
              factura.combustible,
              factura.cantidad,
              factura.precio,
              factura.total,
              factura.fecha,
              factura.guid,
              factura.descuento,
              factura.subTotal,
              factura.descripcionResolucion,
              factura.placa,
              factura.identificacion,
              factura.nombreTercero,
              factura.estado,
              factura.formaDePago,
              false,
              factura.idFacturaElectronica));
              this.excelData = [];
              debugger;
              values.forEach(obj=>
                this.excelData.push({
                  Consecutivo: obj.consecutivo,
                  Combustible:obj.combustible,
                  Cantidad:obj.cantidad,
                  Precio:obj.precio,
                  Fecha:obj.fecha,
                  SubTotal:obj.subTotal,
                  Descuento:obj.descuento,
                  Total:obj.total,
                  Resolucion:obj.descripcionResolucion,
                  Placa:obj.placa,
                  Identificacion:obj.identificacion,
                  Tercero:obj.nombre,
                  Estado:obj.estado,
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

  obtenerSeleccion($event: FacturaUI) {
    if ($event.seleccionado) {
      this.seleccion = [...this.seleccion, { guid: $event.guid }];
    } else {
      this.seleccion = this.seleccion.filter(item => item.guid === $event.guid);
    }
  }

  async anularOrnedesDespacho() {
    const response = await this.facturaService.anularFacturas(this.seleccion)
      .toPromise()
      .catch(error => {
        this.handleException(error);
      });

    if (response) {
      this.toast.success(`Se anularon ${response} ordenes de despacho.`);
    }
  }

  async generarFactura() {
    const response = await this.facturaService.CrearFacturaFacturas(this.seleccion)
      .toPromise()
      .catch(error => {
        this.toast.error(`Error grave al generar factura ${error}`);
      });

      if(response){
        if (response == "Ok") {
          this.onSubmit();
          this.toast.success(`Factura electrónica generada`);
        } else{
          this.toast.warning(`Factura electrónica no generada.  Razón ${response}`);
        }
        }
        this.seleccion = [];
        
        this.onSubmit();
  }

  filtrar = (valorABuscar: string) => {
    if (valorABuscar.length < 3) {
      return;
    }
    valorABuscar = valorABuscar.toLowerCase();
    this.isLoading = true;
    this.facturas$ = this.facturas$.pipe(
      switchMap(_ => this.listFacturaInicial
        .pipe(
          map(facturas => {
            this.isLoading = false;
            this.empty = facturas.length === 0;
            return facturas.filter((factura: FacturaUI) =>
              `${factura.descripcionResolucion}-${factura.consecutivo.toString()}`.toLowerCase().includes(valorABuscar)
              || factura.placa.toLowerCase().includes(valorABuscar)
              || factura.identificacion.toLowerCase().includes(valorABuscar)
              || factura.nombre.toLowerCase().includes(valorABuscar)
            );
          }))));
  }

  async impimirOrnedesDespacho() {
    const response = await this.facturaService.imprimirFacturas(this.seleccion)
      .toPromise()
      .catch(error => {
        this.seleccion = [];
        
        this.onSubmit();
        this.handleException(error);
      });

    if (response) {
      this.toast.success(`Se imprimieron ${response} orden(es) de despacho.`);
      this.seleccion = [];
      
      this.onSubmit();
    }
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
