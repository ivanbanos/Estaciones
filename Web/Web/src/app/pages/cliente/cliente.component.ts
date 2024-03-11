import { Cliente } from './../../models/Cliente';
import { ClientesService } from './../../providers/clientes/clientes.service';
import { BaseComponent } from './../base/base.component';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ErrorService } from 'src/app/errors/services/error.service';
import { ToastrService } from 'ngx-toastr';
import { Subscription, of } from 'rxjs';
import { ClientesUI } from 'src/app/models/UIModels/ClientesUI';

@Component({
  selector: 'app-cliente',
  templateUrl: './cliente.component.html',
  styleUrls: ['./cliente.component.css']
})
export class ClienteComponent extends BaseComponent implements OnInit, OnDestroy  {

  clientes: Array<ClientesUI>;
  clienteSubscription: Subscription;
  columnas: string[] = ['ID', 'Nombre', 'Numero Documento', 'Tipo De Documento'];

  constructor(public readonly router: Router,
              public readonly errorService: ErrorService,
              public readonly toast: ToastrService,
              private readonly clientesService: ClientesService
    ) {
    super(router, errorService, toast);
  }

  async ngOnInit() {
    this.clienteSubscription = this.clientesService.getClientes().subscribe((c: Array<Cliente>) => {
      for (const cliente of c) {
        this.clientes.push(new ClientesUI(cliente.guid, cliente.nombre, cliente.numero, cliente.tipoDeDocumentoDescripcion));
      }
    }, error => this.handleException(error));
  }

  ngOnDestroy() {
    if (this.clienteSubscription) {
      this.clienteSubscription.unsubscribe();
    }
  }
}
