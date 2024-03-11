import { Injectable } from '@angular/core';
import { environment as ENV } from '../../../environments/environment';
import { HttpService } from '../base/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TipoIdentificacionService {
  private readonly url: string = `${ENV.serverUrl}/ManejadorInformacionLocal/GetTipos`;

  constructor(private readonly http: HttpService) { }

  getTipos(): Observable<Array<string>> {
    return this.http.get<Array<string>>(this.url);
  }

}
