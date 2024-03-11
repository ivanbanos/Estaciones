import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { map, switchMap } from 'rxjs/operators';
import { ErrorService } from 'src/app/errors/services/error.service';
import { Estacion } from 'src/app/models/Estacion';
import { EstacionService } from 'src/app/providers/estacion/estacion.service';
import { BaseComponent } from '../../base/base.component';
import { of } from 'rxjs';

@Component({
  selector: 'app-agregar-estaciones',
  templateUrl: './agregar-estaciones.component.html',
  styleUrls: ['./agregar-estaciones.component.sass']
})
export class AgregarEstacionesComponent extends BaseComponent implements OnInit {

  idEstacion: string;
  hasNit = false;
  hasName = false;
  public estacion: Estacion;

  constructor(public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    public readonly estacionesService: EstacionService,
    private activatedRoute: ActivatedRoute) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    this.activatedRoute.params
      .pipe(
        map(params => {
          return params.idEstacion;
        }),
        switchMap(idEstacion => {
          if (idEstacion) {
            return this.estacionesService.getEstacion(idEstacion);
          }
          return of(new Estacion());
        }))
      .subscribe(estacion => {
        this.estacion = estacion;
        if(estacion.nit){
          this.hasName = true;
          this.hasNit = true;
        }
      }, error => this.handleException(error));

    if (!this.estacion.nit) {
      this.estacion = new Estacion();
      this.hasName = false;
      this.hasNit = false;
    }
  }

  public async onSubmit() {
    if (this.estacion) {
      const response = await this.estacionesService.addOrUpdate([this.estacion]).toPromise()
        .then(x => {
          this.toast.success(`Estación Actualizada.`);
          this.openPage(`/dashboard/estaciones`);
        });
    }
  }

  public back() {
    this.openPage(`/dashboard/estaciones`);
  }

}
