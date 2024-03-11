import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreacionResolucion } from 'src/app/models/CreacionResolucion';
import { Resolucion } from 'src/app/models/Resolucion';
import { environment as ENV } from '../../../environments/environment';
import { HttpService } from '../base/http.service';

@Injectable({
  providedIn: 'root'
})
export class ResolucionService {

  private readonly url: string = `${ENV.serverUrl}/Resolucion`;

  constructor(private readonly http: HttpService) { }

  getResolucion(idEstacion: string): Observable<Resolucion[]> {
    return this.http.get<Resolucion[]>(`${this.url}/${idEstacion}`);
  }

  addResolucion(resolucion: CreacionResolucion): Observable<number> {
    const requestUrl = `${this.url}/AddNuevaResolucion`;
    return this.http.post<CreacionResolucion, number>(requestUrl, resolucion);
  }

  habilitarResolucion(resolucion: string, fechaVencimiento: Date): Observable<number> {
    const requestUrl = `${this.url}/HabilitarResolucion/${resolucion}`;
    return this.http.post<Date, number>(requestUrl, fechaVencimiento);
  }

  
  anularResolucion(resolucion: string){
    const requestUrl = `${this.url}/AnularResolucion/${resolucion}`;
   this.http.get(requestUrl);
  }

}
