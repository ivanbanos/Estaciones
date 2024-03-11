import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, Subscription } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ErrorService } from 'src/app/errors/services/error.service';
import { Estacion } from 'src/app/models/Estacion';
import { EstacionUI } from 'src/app/models/UIModels/EstacionUI';
import { EstacionService } from 'src/app/providers/estacion/estacion.service';
import { BaseComponent } from '../../base/base.component';

@Component({
  selector: 'app-buscar-estaciones',
  templateUrl: './buscar-estaciones.component.html',
  styleUrls: ['./buscar-estaciones.component.sass']
})
export class BuscarEstacionesComponent extends BaseComponent implements OnInit {

  estaciones$: Observable<Array<EstacionUI>>;
  estacionesSubscription: Subscription;
  columnas: string[] = ['Identificador', 'Nit', 'Nombre', 'Direccion', 'Razon', 'Linea 1', 'Linea 2', 'Linea 3', 'Linea 4',
    'Telefono'];
  FormularioEstacion: FormGroup;
  seleccion: Array<any> = [];
  loaded = false;
  isLoading = false;
  firstSearch = true;

  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    public readonly estacionesService: EstacionService,
    public readonly fb: FormBuilder) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    this.isLoading = true;
    this.estaciones$ =
      this.estacionesService.getEstaciones()
        .pipe(
          map((listaEstaciones: Array<Estacion>) => {
            this.isLoading = false;
            if (listaEstaciones.length > 0) {
              this.loaded = true;
            }
            return listaEstaciones.map((estacion) => {
              if (this.estacionesService.obtenerEstacion() && estacion.guid === this.estacionesService.obtenerEstacion()) {
                const est = new EstacionUI(estacion.guid, estacion.nit, estacion.nombre, estacion.direccion, estacion.razon,
                  estacion.linea1, estacion.linea2, estacion.linea3, estacion.linea4, estacion.telefono, true);
                this.seleccion = [est];
                return est;
              }
              return new EstacionUI(estacion.guid, estacion.nit, estacion.nombre, estacion.direccion, estacion.razon,
                estacion.linea1, estacion.linea2, estacion.linea3, estacion.linea4, estacion.telefono, false);
            });
          }), map((estaciones: Array<EstacionUI>) => {
            if (estaciones && estaciones.length && !estaciones.some(estacion => estacion.seleccionado)) {
              estaciones[0].seleccionado = true;
              this.seleccion = [estaciones[0]];
              this.estacionesService.guardarSeleccion(estaciones[0]);
            }
            return estaciones;
          }), catchError(error => {
            this.handleException(error);
            this.isLoading = false;
            return [];
          })
        );
  }


  obtenerSeleccion($event: EstacionUI, listaDeEstaciones: Array<EstacionUI>) {
    if ($event.seleccionado && listaDeEstaciones.some(estacion => estacion.seleccionado)) {
      this.seleccion = [$event];
      this.estacionesService.guardarSeleccion($event);
      listaDeEstaciones
        .filter(estacion => estacion.guid !== $event.guid)
        .map(estacion => estacion.seleccionado = false);
    } else {
      this.toast.warning('Debe estar seleccionada por lo menos una estación y no podra acceder a las demas opciones del sistema');
      this.estacionesService.borrarEstacionSeleccionada();
    }
  }

  async borrarEstacion() {
    const response = await this.estacionesService.borrarEstaciones(this.seleccion)
      .toPromise()
      .catch(error => {
        this.handleException(error);
      });

    if (response) {
      this.toast.success(`Se anularon ${response} estación(es).`);
    }
  }
  agregarEstacion() {
    this.openPage(`/dashboard/estacion`);
  }

  showUpdate = (estacion: EstacionUI) => {
    this.openPage(`/dashboard/estacion/${estacion.guid}`);
  }

}
