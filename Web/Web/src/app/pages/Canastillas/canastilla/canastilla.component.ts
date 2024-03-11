import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, map, switchMap } from 'rxjs/operators';
import { ErrorService } from 'src/app/errors/services/error.service';
import { Canastilla } from 'src/app/models/canastilla';
import { BaseComponent } from 'src/app/pages/base/base.component';
import { CanastillasService } from 'src/app/providers/canastillas/canastillas.service';
import { TipoIdentificacionService } from 'src/app/providers/tipoIdentificacion/TipoIdentificacion.service';

import { Observable } from 'rxjs';
@Component({
  selector: 'app-canastilla',
  templateUrl: './canastilla.component.html',
  styleUrls: ['./canastilla.component.sass']
})
export class CanastillaComponent extends BaseComponent implements OnInit {

  idcanastilla:string;
  public canastilla: Canastilla;
  public identificaciones$:Observable<Array<string>>;
  public isLoading = false;
  public total = 0;
  empty: boolean;

  constructor(public readonly router: Router,
              public readonly errorService: ErrorService,
              public readonly toast: ToastrService,
              private readonly canastillasService: CanastillasService,
              private readonly tipoIdentifciacionService: TipoIdentificacionService,
              private activatedRoute: ActivatedRoute) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    
    this.isLoading = true;
    debugger;
    if (this.activatedRoute.snapshot.params['idCanastilla']) {
      this.activatedRoute.params
      .pipe(
        map(params => {
          return params.idCanastilla;
        }),
        switchMap(idCanastilla => this.canastillasService.getCanastilla(idCanastilla)))
      .subscribe(canastilla => {
        this.canastilla = canastilla;
        this.canastilla.precio = Math.round(+((this.canastilla.precio*1)/(1+this.canastilla.iva/100)));
        this.setTotal();
      }, error => this.handleException(error));
    }else {
      this.canastilla = {
        guid:"",
    descripcion:"",
    deleted:false,
unidad : "",
precio:0,
iva:0
      }
    }

      
  }

  public async onSubmit() {
    if (this.canastilla) {
      debugger;
      if(this.canastilla.guid == null ||this.canastilla.guid == ""){

        this.canastilla.guid = "00000000-0000-0000-0000-000000000000";
      }
      this.canastilla.precio = this.total
      this.canastilla.iva = +this.canastilla.iva;
      const response = await this.canastillasService.addOrUpdate([this.canastilla]).toPromise()
      .then(x => {
        
        this.toast.warning(`iten de canastilla actualizado.`);
         this.openPage(`/dashboard/canastillas`);
      });
    }
  }

  public async borrar() {
    if (this.canastilla) {
      debugger;
      if(this.canastilla.guid == null ||this.canastilla.guid == ""){

        this.canastilla.guid = "00000000-0000-0000-0000-000000000000";
      }
      this.canastilla.deleted = true;
      this.canastilla.precio = +this.canastilla.precio;
      this.canastilla.iva = +this.canastilla.iva;
      const response = await this.canastillasService.addOrUpdate([this.canastilla]).toPromise()
      .then(x => {
        
        this.toast.warning(`iten de canastilla borrado.`);
         this.openPage(`/dashboard/canastillas`);
      });
    }
  }

  public back() {
    this.openPage(`/dashboard/canastillas`);
  }

  public setTotal(){
    this.total = Math.floor(+((this.canastilla.precio*1)*(1+this.canastilla.iva/100)));
  }
}
