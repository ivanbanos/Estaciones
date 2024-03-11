import { Injectable } from '@angular/core';
import { HttpService } from '../base/http.service';
import {  HttpHeaders } from '@angular/common/http';
 

import { Observable } from 'rxjs';
import { environment as ENV } from '../../../environments/environment';
import { tap, map } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { Isla } from 'src/app/models/Isla';
import { GuidService } from '../base/guid.service';

@Injectable({
  providedIn: 'root'
})
export class IslasService {
  url: string = ENV.serverUrl + '/Islas';

constructor(private readonly http: HttpService, 
  private readonly guidService: GuidService) { }


  Get (): Observable<Isla[]> {
    debugger;
    return this.http.get<Isla[]>(this.url);
  }

  
  /** POST: add a new Insurrance to the server */
  Agregar (isla: Isla) {
    debugger;
    const islas : Isla[] = [];
    isla.guid = this.guidService.generateGuid();
    console.log(this.url);
    console.log(isla);
    islas.push(isla);
   console.log(islas);
    return this.http.post(this.url, islas);
    
  }


}
