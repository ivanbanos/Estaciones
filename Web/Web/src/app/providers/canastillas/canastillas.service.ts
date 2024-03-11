import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Canastilla } from 'src/app/models/Canastilla';
import { environment as ENV } from '../../../environments/environment';
import { HttpService } from '../base/http.service';

@Injectable({
  providedIn: 'root'
})
export class CanastillasService {
  private readonly url: string = `${ENV.serverUrl}/Canastilla`;

  constructor(private readonly http: HttpService) { }

  getCanastillas(): Observable<Array<Canastilla>> {
    return this.http.get<Array<Canastilla>>(this.url);
  }

  getCanastilla(idCanastilla: string): Observable<Canastilla> {
    return this.http.get<Canastilla>(`${this.url}/${idCanastilla}`);
  }


  addOrUpdate(canastillas: Array<Canastilla>): Observable<number> {
    return this.http.post<Array<Canastilla>, number>(this.url, canastillas);
  }
}
