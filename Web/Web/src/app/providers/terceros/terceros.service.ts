import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Tercero } from 'src/app/models/Tercero';
import { environment as ENV } from '../../../environments/environment';
import { HttpService } from '../base/http.service';

@Injectable({
  providedIn: 'root'
})
export class TercerosService {

  private readonly url: string = `${ENV.serverUrl}/Terceros`;

  constructor(private readonly http: HttpService) { }

  getTerceros(): Observable<Array<Tercero>> {
    return this.http.get<Array<Tercero>>(this.url);
  }

  getTercero(idTercero: string): Observable<Tercero> {
    return this.http.get<Tercero>(`${this.url}/${idTercero}`);
  }

  addOrUpdate(terceros: Array<Tercero>): Observable<number> {
    return this.http.post<Array<Tercero>, number>(this.url, terceros);
  }
}
