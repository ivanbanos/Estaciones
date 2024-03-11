import { Component, OnInit } from '@angular/core';
import { BaseComponent } from '../base/base.component';
import { Router } from '@angular/router';
import { ErrorService } from 'src/app/errors/services/error.service';
import { ToastrService } from 'ngx-toastr';
import { OrdenDeDespachoService } from 'src/app/providers/ordenDeDespacho/orden-de-despacho.service';
import { OrdenDeDespacho } from 'src/app/models/OrdenDeDespacho';
import { Observable, of, Subscription } from 'rxjs';
import { OrdenDeDespachoUI } from 'src/app/models/UIModels/OrdenDeDespachoUI';
import { FormBuilder, FormGroup } from '@angular/forms';
import { FiltroBusqueda } from 'src/app/models/filtroOrdenesDeDespacho';
import { map, catchError, switchMap } from 'rxjs/operators';
import { FacturaService } from 'src/app/providers/factura/factura.service';
import * as FileSaver from 'file-saver';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-orden-de-despacho',
  templateUrl: './orden-de-despacho.component.html',
  styleUrls: ['./orden-de-despacho.component.sass']
})
export class OrdenDeDespachoComponent extends BaseComponent implements OnInit {

  ordenes: Array<OrdenDeDespacho>;
  ordenes$: Observable<Array<OrdenDeDespachoUI>>;
  FormularioOrdenDespacho: FormGroup;
  filtroOrdenDeDespacho: FiltroBusqueda;
  listOrdenesInicial$: Observable<Array<OrdenDeDespachoUI>>;
  seleccion: Array<any> = [];
  loaded = false;
  valorBusqueda = '';
  isLoading = false;
  firstSearch = true;
  empty = false;
  totalFacturas = 0;
  totalDinero = "0";
  excelData: any[];
  EXCEL_TYPE :string = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
  EXCEL_EXTENSION:string = '.xlsx';

  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    public readonly ordenDeDespachoService: OrdenDeDespachoService,
    public readonly facturaService: FacturaService,
    public readonly fb: FormBuilder) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    this.Formulario();
  }

  Formulario() {
    this.FormularioOrdenDespacho = this.fb.group({
      fechaInicial: [],
      fechaFinal: [],
      identificacion: [],
      nombreTercero: [],
    });
  }

  onSubmit() {
    if (this.FormularioOrdenDespacho.valid) {
      this.isLoading = true;
      this.firstSearch = false;
      this.filtroOrdenDeDespacho = this.obtenerInformacionFormulario();
      this.ordenes$ =
        this.ordenDeDespachoService.postOrdenesDeDespacho(this.filtroOrdenDeDespacho)
          .pipe(
            map((listaordenes: Array<OrdenDeDespacho>) => {
              this.isLoading = false;
              if (listaordenes.length > 0) {
                this.loaded = true;
              }
              this.empty = listaordenes.length === 0;

              if(!this.empty){
                this.totalFacturas = listaordenes.length;
                this.totalDinero = listaordenes.map(a => a.total).reduce(function(a, b)
                {
                  return a + b;
                }).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",");
              }
              const values = listaordenes.map(orden =>
                new OrdenDeDespachoUI(orden.idVentaLocal, orden.identificacion, orden.nombreTercero, orden.combustible,
                  orden.cantidad, orden.precio, orden.total, orden.idInterno, orden.placa, orden.kilometraje,
                  orden.surtidor, orden.isla, orden.manguera, orden.fecha, orden.estado, orden.guid, orden.subTotal,
                  orden.descuento, orden.nit, orden.formaDePago, false, orden.idFacturaElectronica));
debugger;
                  this.excelData = [];
                  values.forEach(obj=>
                    this.excelData.push({
                      Consecutivo: obj.idVentaLocal,
                      Combustible:obj.combustible,
                      Cantidad:obj.cantidad,
                      Precio:obj.precio,
                      Fecha:obj.fecha,
                      SubTotal:obj.subTotal,
                      Descuento:obj.descuento,
                      Total:obj.total,
                      Placa:obj.placa,
                      Identificacion:obj.identificacion,
                      Tercero:obj.nombreTercero,
                      Estado:obj.estado,
                      FormaDePago:obj.formaDePago})
                    );
              this.listOrdenesInicial$ = of(values);
              return values;
            }), catchError(error => {
              this.handleException(error);
              this.isLoading = false;
              this.empty = true;
              return [];
            })
          );
    }
  }

  obtenerInformacionFormulario() {
    return new FiltroBusqueda(
      this.FormularioOrdenDespacho.value.fechaInicial != null ?
        new Date(this.FormularioOrdenDespacho.value.fechaInicial) : null,
      this.FormularioOrdenDespacho.value.fechaFinal != null ?
        new Date(this.FormularioOrdenDespacho.value.fechaFinal) : null,
      this.FormularioOrdenDespacho.value.identificacion,
      this.FormularioOrdenDespacho.value.nombreTercero,
      localStorage.getItem('estacion'));
  }

  obtenerSeleccion($event: OrdenDeDespachoUI) {
    if ($event.seleccionado && !this.seleccion.includes($event.guid)) {
debugger;
      this.seleccion = [...this.seleccion, { guid: $event.guid }];
    } else {
      this.seleccion = this.seleccion.filter(item => item.guid === $event.guid);
    }

  }

  async generarFactura() {
    const response = await this.ordenDeDespachoService.CrearFacturaOrdenesDeDespacho(this.seleccion)
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

  async anularOrnedesDespacho() {
    const response = await this.ordenDeDespachoService.anularOrdenesDeDespacho(this.seleccion)
      .toPromise()
      .catch(error => {
        this.handleException(error);
      });

    if (response) {
      this.toast.success(`Se anularon ${response} ordenes de despacho.`);
    }
    
    this.seleccion = [];
    
    this.onSubmit();
  }

  async impimirOrnedesDespacho() {
    this.seleccion = this.seleccion.filter((value, index, self) => self.indexOf(value) === index)
    const response = await this.ordenDeDespachoService.imprimirOrdenesDeDespacho(this.seleccion)
      .toPromise()
      .catch(error => {
        this.seleccion = [];
        this.handleException(error);
      });

    if (response) {
      this.toast.success(`Se imprimieron ${response} orden(es) de despacho.`);
    }
    this.seleccion = [];
    
    this.onSubmit();
  }

  filtrar = (valorABuscar: string) => {
    if (valorABuscar.length < 3) {
      return;
    }
    this.isLoading = true;
    this.ordenes$ = this.ordenes$.pipe(
      switchMap(_ => this.listOrdenesInicial$
        .pipe(
          map(ordenes => {
            this.isLoading = false;
            this.empty = ordenes.length === 0;
            return ordenes.filter((orden: OrdenDeDespachoUI) =>
              orden.idVentaLocal.toString().includes(valorABuscar)
              || orden.identificacion.toLowerCase().includes(valorABuscar)
              || orden.nombreTercero.toLowerCase().includes(valorABuscar)
            );
          }))));
  }
  exportAsXLSX() {
    
    
    console.log(this.excelData);
    this.exportAsExcelFile(this.excelData, 'Reporte de ordenes de despacho');
 }


  public exportAsExcelFile(json: any[], excelFileName: string): void {
    console.log(json);
    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(json);
    console.log(worksheet);
    const workbook: XLSX.WorkBook = { Sheets: { 'ordenes': worksheet }, SheetNames: ['ordenes'] };
    const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    this.saveAsExcelFile(excelBuffer, excelFileName);
  }
  private saveAsExcelFile(buffer: any, fileName: string): void {
     const data: Blob = new Blob([buffer], {type: this.EXCEL_TYPE});
     FileSaver.saveAs(data, fileName + '_export_' + new  Date().getTime() +  this.EXCEL_EXTENSION);
  }
}
