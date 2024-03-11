import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, Subscription } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ErrorService } from 'src/app/errors/services/error.service';
import { Estacion } from 'src/app/models/Estacion';
import { Resolucion } from 'src/app/models/Resolucion';
import { EstacionService } from 'src/app/providers/estacion/estacion.service';
import { ResolucionService } from 'src/app/providers/resolucion/resolucion.service';
import { BaseComponent } from '../base/base.component';

@Component({
  selector: 'app-resolucion',
  templateUrl: './resolucion.component.html',
  styleUrls: ['./resolucion.component.sass']
})
export class ResolucionComponent extends BaseComponent implements OnInit {

  estaciones: Observable<Array<Estacion>>;
  resolucionCombustible: Resolucion;
  resolucionCanastilla: Resolucion;
  resoluciones$: Observable<Resolucion[]>;
  resolucionSubscription: Subscription;
  fechaVencimiento: string;
  fechaVencimientoCanastilla: string;
  columnas: string[] = ['Guid', 'ConsecutivoInicial', 'ConsecutivoFinal', 'FechaInicial', 'FechaFinal', 'Estado', 'ConsecutivoActual',
    'Fecha', 'Estacion', 'Autorizacion', 'Habilitada', 'Descripcion'];
  FormularioResolucion: FormGroup;
  seleccion: Array<any>;
  loaded = false;
  isLoading = false;
  firstSearch = true;
  empty = false;

  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    public readonly estacionesService: EstacionService,
    public readonly resolucionService: ResolucionService,
    public readonly fb: FormBuilder) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    this.resolucionService.getResolucion(localStorage.getItem('estacion'))
      .subscribe(r =>{
        debugger;
        this.resolucionCanastilla = r.find(x => x.tipo == 1);
        this.resolucionCombustible = r.find(x => x.tipo == 0);},
        error => {
          this.handleException(error);
          this.empty = true;
        });
  }

  Formulario() {
  }

  onSubmit() {
    if (this.FormularioResolucion.valid) {
    }
  }

  agregarResolucion() {
    this.openPage(`/dashboard/resolucion/0`);
  }agregarResolucionCanastilla() {
    this.openPage(`/dashboard/resolucion/1`);
  }

  async habilitarResolucion() {
    const response = await this.resolucionService.habilitarResolucion(this.resolucionCombustible.guid, new Date(this.fechaVencimiento))
      .toPromise()
      .catch(error => {
        this.handleException(error);
      });
    if (response) {
      this.toast.success(`Se habilitó ${response} resolución.`);
    }

  }

  async habilitarResolucionCanastilla() {
    const response = await this.resolucionService.habilitarResolucion(this.resolucionCanastilla.guid, new Date(this.fechaVencimientoCanastilla))
      .toPromise()
      .catch(error => {
        this.handleException(error);
      });
    if (response) {
      this.toast.success(`Se habilitó ${response} resolución.`);
    }

  }

  obtenerSeleccion($event: Array<string>) {
    this.seleccion = $event.map(listaResolucion => {
      return {
        guid: listaResolucion
      };
    });
  }
}
