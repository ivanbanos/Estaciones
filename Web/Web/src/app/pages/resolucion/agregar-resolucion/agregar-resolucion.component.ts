import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { map, switchMap } from 'rxjs/operators';
import { ErrorService } from 'src/app/errors/services/error.service';
import { CreacionResolucion } from 'src/app/models/CreacionResolucion';
import { Resolucion } from 'src/app/models/Resolucion';
import { ResolucionService } from 'src/app/providers/resolucion/resolucion.service';
import { BaseComponent } from '../../base/base.component';

@Component({
  selector: 'app-agregar-resolucion',
  templateUrl: './agregar-resolucion.component.html',
  styleUrls: ['./agregar-resolucion.component.sass']
})
export class AgregarResolucionComponent extends BaseComponent implements OnInit {

  idEstacion: string;
  fechaIni: string;
  fechaFi: string;
  consecutivoIni: number;
  consecutivoFi: number;
  hasNit = false;
  hasName = false;
  public creacionResolucion: CreacionResolucion;

  constructor(public readonly router: Router,
              public readonly errorService: ErrorService,
              public readonly toast: ToastrService,
              public readonly resolucionService: ResolucionService,
              private activatedRoute: ActivatedRoute) {
  super(router, errorService, toast);
  }

  ngOnInit() {
    const tipo: number = this.activatedRoute.snapshot.params["tipo"];
    if (tipo) {
      
      this.creacionResolucion = new CreacionResolucion();
      this.creacionResolucion.tipo = tipo;
    }else{

        this.creacionResolucion = new CreacionResolucion();
        this.creacionResolucion.tipo = 0;
    }
  }

  public async onSubmit() {
    if (this.creacionResolucion) {
      this.creacionResolucion.habilitada = false;
      this.creacionResolucion.fechaInicial = new Date(this.fechaIni);
      this.creacionResolucion.fechaFinal = new Date(this.fechaFi);
      this.creacionResolucion.consecutivoInicial = Number(this.consecutivoIni);
      this.creacionResolucion.consecutivoFinal = Number(this.consecutivoFi);
      this.creacionResolucion.tipo = Number(this.creacionResolucion.tipo);
      this.creacionResolucion.idEstacion = localStorage.getItem('estacion');
      this.creacionResolucion.consecutivoActual = 0;
      const response = await this.resolucionService.addResolucion(this.creacionResolucion).toPromise()
      .then(x => {
         this.toast.success(`Resolución Agregada.`);
         this.openPage(`/dashboard/resoluciones`);
      });
    }
  }

  public back() {
    this.openPage(`/dashboard/resoluciones`);
  }
}
