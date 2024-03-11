import { ReporteConsolidado } from './../../models/ReporteConsolidado';
import { Observable } from 'rxjs';

import { Injectable } from '@angular/core';
import { environment as ENV } from '../../../environments/environment';
import { HttpService } from '../base/http.service';
import { Factura } from 'src/app/models/Factura';
import { FiltroBusqueda } from 'src/app/models/filtroOrdenesDeDespacho';

@Injectable({
  providedIn: 'root'
})
export class FacturaService {
  private readonly url: string = `${ENV.serverUrl}/Factura`;

  constructor(private readonly http: HttpService) { }

  CrearFacturaFacturas(facturasEntity: Array<any>): Observable<string> {
    return this.http.postText<Array<any>, string>(`${this.url}/CrearFacturaFacturas`, facturasEntity);
  }

  buscarFacturas = (filtroBusqueda: FiltroBusqueda): Observable<Array<Factura>> => {
    return this.http.post<FiltroBusqueda, Array<Factura>>(`${this.url}/GetFactura`, filtroBusqueda);
  }

  anularFacturas(ordenesDeDespacho: Array<any>): Observable<number> {
    return this.http.post<Array<any>, number>(`${this.url}/AnularFacturas`, ordenesDeDespacho);
  }

  imprimirFacturas(ordenesDeDespacho: Array<any>): Observable<number> {
    return this.http.post<Array<any>, number>(`${this.url}/AddFacturasImprimir`, ordenesDeDespacho);
  }

  buscarConsolidado(filtroBusqueda: FiltroBusqueda): Observable<ReporteConsolidado> {
    return this.http.post<FiltroBusqueda, ReporteConsolidado>(`${this.url}/GetConsolidado`, filtroBusqueda);
  }
  EnviarFacturacion (ordenGuid: string): Observable<string>  {
    
    return this.http.getText<string>(`${this.url}/EnviarFacturacion/${ordenGuid}`);
  }

}
