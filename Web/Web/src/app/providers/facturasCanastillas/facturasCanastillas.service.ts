import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Factura } from 'src/app/models/Factura';
import { FacturaCanastilla } from 'src/app/models/FacturaCanastilla';
import { FacturaCanastillaDetalle } from 'src/app/models/FacturaCanastillaDetalle';
import { FiltroBusqueda } from 'src/app/models/filtroOrdenesDeDespacho';
import { environment as ENV } from '../../../environments/environment';
import { HttpService } from '../base/http.service';

@Injectable({
  providedIn: 'root'
})
export class FacturasCanastillasService {
  private readonly url: string = `${ENV.serverUrl}/FacturasCanastilla`;

  constructor(private readonly http: HttpService) { }

  buscarFacturas = (filtroBusqueda: FiltroBusqueda): Observable<Array<FacturaCanastilla>> => {
    return this.http.post<FiltroBusqueda, Array<FacturaCanastilla>>(`${this.url}/GetFactura`, filtroBusqueda);
  }

  getFacturaCanastilla(idCanastilla: string): Observable<FacturaCanastilla> {
    return this.http.get<FacturaCanastilla>(`${this.url}/${idCanastilla}`);
  }


  getFacturaCanastillaDetalle(idCanastilla: string): Observable<FacturaCanastillaDetalle[]> {
    return this.http.get<FacturaCanastillaDetalle[]>(`${this.url}/detalle/${idCanastilla}`);
  }

  
  ColocarEspera(idFactura: string, idEstacion: string): Observable<number> {
    return this.http.get<number>(`${this.url}/ColocarEspera/${idFactura}/Estacion/${idEstacion}`);
  }
}
