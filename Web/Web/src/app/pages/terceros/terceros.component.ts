import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ErrorService } from 'src/app/errors/services/error.service';
import { TercerosService } from 'src/app/providers/terceros/terceros.service';
import { BaseComponent } from '../base/base.component';
import { TerceroUI } from 'src/app/models/UIModels/TerceroUI';
import { map, catchError } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-terceros',
  templateUrl: './Terceros.component.html',
  styleUrls: ['./Terceros.component.sass']
})
export class TercerosComponent extends BaseComponent implements OnInit {

  public tercerosList$: Observable<Array<TerceroUI>>;
  public columnas: string[] = ['Nombre', 'Identificacion', 'Tipo Identificacion'];
  public isLoading = false;
  public empty = false;

  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    private readonly tercerosService: TercerosService) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    this.isLoading = true;
    this.tercerosList$ = this.tercerosService.getTerceros().pipe(
      map(terceros => {
        if (terceros) {
          this.isLoading = false;
        }
        this.empty = terceros.length === 0;
        return terceros.map(tercero => {
          return {
            nombre: tercero.nombre,
            identificacion: tercero.identificacion,
            descripcionTipoIdentificacion: tercero.descripcionTipoIdentificacion,
            guid: tercero.guid
          } as TerceroUI;
        });
      }), catchError(error => {
        this.handleException(error);
        this.isLoading = false;
        return [];
      })
    );
  }

  showUpdate = (tercero: TerceroUI) => {
    this.openPage(`/dashboard/tercero/${tercero.guid}`);
  }
  showAdd  ()  {
    this.openPage(`/dashboard/tercero`);
  }
}
