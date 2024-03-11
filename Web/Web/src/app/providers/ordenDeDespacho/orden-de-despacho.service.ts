import { HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FiltroBusqueda } from 'src/app/models/filtroOrdenesDeDespacho';
import { OrdenDeDespacho } from 'src/app/models/OrdenDeDespacho';
import { environment as ENV } from '../../../environments/environment';
import { HttpService } from '../base/http.service';

@Injectable({
  providedIn: 'root'
})
export class OrdenDeDespachoService {
  EnviarFacturacion (ordenGuid: string): Observable<string>  {
    
    return this.http.getText<string>(`${this.url}/EnviarFacturacion/${ordenGuid}`);
  }

  private readonly url: string = `${ENV.serverUrl}/OrdenesDeDespacho`;

  constructor(private readonly http: HttpService) { }

  postOrdenesDeDespacho(filtroOrdenesDeDespacho: FiltroBusqueda): Observable<Array<OrdenDeDespacho>> {
    return this.http.post<FiltroBusqueda, Array<OrdenDeDespacho>>(this.url, filtroOrdenesDeDespacho);
  }

  CrearFacturaOrdenesDeDespacho(ordenesEntity: Array<any>): Observable<string> {
    return this.http.postText<Array<any>, string>(`${this.url}/CrearFacturaOrdenesDeDespacho`, ordenesEntity);
  }


  anularOrdenesDeDespacho(ordenesDeDespacho: Array<any>): Observable<number> {
    return this.http.post<Array<any>, number>( `${this.url}/AnularOrdenes`, ordenesDeDespacho);
  }

  imprimirOrdenesDeDespacho(ordenesDeDespacho: Array<any>): Observable<number> {
    return this.http.post<Array<any>, number>( `${this.url}/AddOrdenesImprimir`, ordenesDeDespacho);
  }

}
