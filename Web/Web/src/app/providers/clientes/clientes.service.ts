import { HttpService } from './../base/http.service';
import { Injectable } from '@angular/core';
import { Cliente } from 'src/app/models/Cliente';
import { environment as ENV } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ClientesService {

  private readonly url: string = `${ENV.serverUrl}/Clientes`;

  constructor(private readonly http: HttpService) { }

  getClientes(): Observable<Array<Cliente>> {
    return this.http.get<Array<Cliente>>(this.url);
  }
}
