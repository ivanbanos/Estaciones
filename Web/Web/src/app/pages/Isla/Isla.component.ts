import { Component, OnInit, OnDestroy } from '@angular/core';
import { IslasService } from 'src/app/providers/Islas/Islas.service';
import { Router } from '@angular/router';
import { Isla } from 'src/app/models/Isla';

import { Subscription } from 'rxjs';
import { BaseComponent } from '../base/base.component';
import { ErrorService } from 'src/app/errors/services/error.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-Isla',
  templateUrl: './Isla.component.html',
  styleUrls: ['./Isla.component.sass']
})
export class IslaComponent extends BaseComponent implements OnInit {

  islasSubscription: Subscription;
  islas: Array<any>;
  columnas: string[];

  nombre: string;
  estado: number;

  constructor(
    private readonly islasService: IslasService,
    public router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService) {
    super(router, errorService, toast);
  }

  ngOnInit() {
    this.columnas = ['guid', 'nombre', 'estado'];
    this.islas = Array.of({
      guid: '1', nombre: '1', estado: '1'
    }, {
        guid: '1', nombre: '1', estado: '1'
      }, {
        guid: '1', nombre: '1', estado: '1'
      });
    // this.islas = [];
  }

  select(event) {
    console.log(event);
  }
}
