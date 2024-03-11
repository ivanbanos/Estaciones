import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, map, switchMap } from 'rxjs/operators';
import { ErrorService } from 'src/app/errors/services/error.service';
import { Tercero } from 'src/app/models/Tercero';
import { BaseComponent } from 'src/app/pages/base/base.component';
import { TercerosService } from 'src/app/providers/terceros/terceros.service';
import { TipoIdentificacionService } from 'src/app/providers/tipoIdentificacion/TipoIdentificacion.service';

import { Observable } from 'rxjs';
@Component({
  selector: 'app-tercero',
  templateUrl: './tercero.component.html',
  styleUrls: ['./tercero.component.sass']
})
export class TerceroComponent extends BaseComponent implements OnInit {

  idTercero:string;
  public tercero: Tercero;
  public identificaciones$:Observable<Array<string>>;
  public isLoading = false;
  empty: boolean;

  constructor(public readonly router: Router,
              public readonly errorService: ErrorService,
              public readonly toast: ToastrService,
              private readonly tercerosService: TercerosService,
              private readonly tipoIdentifciacionService: TipoIdentificacionService,
              private activatedRoute: ActivatedRoute) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    
    this.isLoading = true;
    debugger;
    if (this.activatedRoute.snapshot.params['idTercero']) {
      this.activatedRoute.params
      .pipe(
        map(params => {
          return params.idTercero;
        }),
        switchMap(idTercero => this.tercerosService.getTercero(idTercero)))
      .subscribe(tercero => {
        this.tercero = tercero;
      }, error => this.handleException(error));
    }else {
      this.tercero = {
        guid:"",
    nombre:"",
	segundo:"",
	apellidos :"",
	tipoPersona:0,
	responsabilidadTributaria :0,
	municipio:"",
	departamento :"",
	direccion :"",
	pais :"",
	codigoPostal :"",
	celular :"",
	telefono :"",
	telefono2 :"",
	correo :"",
	correo2 :"",
	vendedor :"",
	comentarios :"",
    tipoIdentificacion: 1,
    descripcionTipoIdentificacion:"",
    identificacion:"",
    idLocal:0,
    idFacturacion : null
      }
    }

      this.identificaciones$ = this.tipoIdentifciacionService.getTipos().pipe(
        map(tipos => {
          if (tipos) {
            this.isLoading = false;
          }
          this.empty = tipos.length === 0;
          return tipos;
        }), catchError(error => {
          this.handleException(error);
          this.isLoading = false;
          return [];
        })
      );
  }

  public async onSubmit() {
    if (this.tercero) {
      debugger;
      if(this.tercero.guid == null ||this.tercero.guid == ""){

        this.tercero.guid = "00000000-0000-0000-0000-000000000000";
      }
      const response = await this.tercerosService.addOrUpdate([this.tercero]).toPromise()
      .then(x => {
        if(x > 0){
          this.toast.warning(`Tercero Actualizado. Aun no apto para facturación electrónica`);
        } else{
          this.toast.warning(`Tercero Actualizado.`);
        }
         this.openPage(`/dashboard/terceros`);
      });
    }
  }

  public back() {
    this.openPage(`/dashboard/terceros`);
  }
}
