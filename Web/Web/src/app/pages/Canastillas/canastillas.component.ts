import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ErrorService } from 'src/app/errors/services/error.service';
import { CanastillasService } from 'src/app/providers/canastillas/canastillas.service';
import { BaseComponent } from '../base/base.component';
import { CanastillaUI } from 'src/app/models/UIModels/CanastillaUI';
import { map, catchError } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-canastillas',
  templateUrl: './canastillas.component.html',
  styleUrls: ['./canastillas.component.sass']
})
export class CanastillasComponent extends BaseComponent implements OnInit {

  public canastillasList$: Observable<Array<CanastillaUI>>;
  public columnas: string[] = ['Descripcion', 'Unidad', 'Precio total'];
  public isLoading = false;
  public empty = false;

  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    private readonly canastillasService: CanastillasService) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    this.isLoading = true;
    this.canastillasList$ = this.canastillasService.getCanastillas().pipe(
      map(canastillas => {
        if (canastillas) {
          this.isLoading = false;
        }
        this.empty = canastillas.length === 0;
        return canastillas.map(canastilla => {
          return {
            descripcion: canastilla.descripcion,
            unidad: canastilla.unidad,
            precio: canastilla.precio,
deleted: canastilla.deleted,
guid: canastilla.guid,
iva: canastilla.iva
          } as CanastillaUI;
        });
      }), catchError(error => {
        this.handleException(error);
        this.isLoading = false;
        return [];
      })
    );
  }

  showUpdate = (canastilla: CanastillaUI) => {
    this.openPage(`/dashboard/canastilla/${canastilla.guid}`);
  }
  showAdd  ()  {
    this.openPage(`/dashboard/canastilla`);
  }
}
