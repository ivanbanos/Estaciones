import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { map, switchMap } from 'rxjs/operators';
import { ErrorService } from 'src/app/errors/services/error.service';
import { FacturaCanastilla } from 'src/app/models/FacturaCanastilla';
import { FacturaCanastillaDetalle } from 'src/app/models/FacturaCanastillaDetalle';
import { FacturaCanastillaUI } from 'src/app/models/UIModels/FacturaCanastillaUI';
import { FacturasCanastillasService } from 'src/app/providers/facturasCanastillas/FacturasCanastillas.service';
import { BaseComponent } from '../../base/base.component';

@Component({
  selector: 'app-facturaCanastillaDetalle',
  templateUrl: './factura-canastilla-detalle.component.html',
  styleUrls: ['./factura-canastilla-detalle.component.scss']
})
export class FacturaCanastillaDetalleComponent extends BaseComponent implements OnInit {
  
  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    private readonly facturasCanastillasService: FacturasCanastillasService,
    private activatedRoute: ActivatedRoute) {
    super(router, errorService, toast);
  }
  idcanastilla:string;
  public facturaCanastilla: FacturaCanastillaUI;
  consecutivio:number;
  public facturaCanastillaDetalles: FacturaCanastillaDetalle[];
  public isLoading = false;
  empty: boolean;
  public columnas: string[] = ['fecha', 'consecutivo', 'iva', 'total', 'Nombre', 'FormaDePago'];
  

  ngOnInit() {
    
    this.isLoading = true;
    debugger;
    if (this.activatedRoute.snapshot.params['idCanastilla']) {
      this.activatedRoute.params
      .pipe(
        map(params => {
          return params.idCanastilla;
        }),
        switchMap(idCanastilla => this.facturasCanastillasService.getFacturaCanastilla(idCanastilla)))
      .subscribe(canastilla => {
        this.consecutivio = canastilla.consecutivo;
        this.facturaCanastilla = new FacturaCanastillaUI(canastilla.consecutivo,canastilla.fecha,  canastilla.nombre, canastilla.subtotal, canastilla.descuento, canastilla.iva, canastilla.total, 
          canastilla.guid, canastilla.facturasCanastillaId,  
          canastilla.segundo, canastilla.apellidos, canastilla.identificacion, canastilla.tipoIdentificacion, canastilla.formaDePago );
      }, error => this.handleException(error));

      // this.activatedRoute.params
      // .pipe(
      //   map(params => {
      //     return params.idCanastilla;
      //   }),
      //   switchMap(idCanastilla => this.facturasCanastillasService.getFacturaCanastillaDetalle(idCanastilla)))
      // .subscribe(canastilla => {
      //   this.facturaCanastillaDetalles = canastilla;
      // }, error => this.handleException(error));
    }else {
      this.toast.warning(`No es posible encontrar factura.`);
      this.openPage(`/dashboard/facturascanastillas`);
      
    }

      
  }

  public back() {
    this.openPage(`/dashboard/facturascanastillas`);
  }
  async imprimir() {
    const response = await this.facturasCanastillasService.ColocarEspera(this.facturaCanastilla.guid, localStorage.getItem('estacion'))
      .toPromise()
      .catch(error => {
        this.toast.error(`Error grave al generar factura ${error}`);
      });
      if(response){
        if (response == 0) {
          this.toast.success(`Impreso`);
        } else{
          this.toast.warning(`Error al imprimir.  Razón ${response}`);
        }
        }
  }
}
