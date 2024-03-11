import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { Estacion } from 'src/app/models/Estacion';
import { environment as ENV } from '../../../environments/environment';
import { HttpService } from '../base/http.service';
import { EstacionUI } from 'src/app/models/UIModels/EstacionUI';

@Injectable({
  providedIn: 'root'
})
export class EstacionService {

  private readonly url: string = `${ENV.serverUrl}/Estaciones`;
  private readonly ESTACION_KEY: string = 'estacion';
  private readonly ESTACION_NAME: string = 'estacion_nombre';

  private behaviorSubjects: Map<string, BehaviorSubject<any>>;

  constructor(private readonly http: HttpService) {
    this.behaviorSubjects = new Map<string, BehaviorSubject<any>>();
  }

  getEstaciones(): Observable<Array<Estacion>> {
    return this.http.get<Array<Estacion>>(`${this.url}`);
  }

  getEstacion(idEstacion: string): Observable<Estacion> {
    return this.http.get<Estacion>(`${this.url}/${idEstacion}`);
  }

  borrarEstaciones(estaciones: Array<any>): Observable<number> {
    return this.http.post<Array<any>, number>(`${this.url}/BorrarEstaciones`, estaciones);
  }

  addOrUpdate(estacion: Array<Estacion>): Observable<number> {
    return this.http.post<Array<Estacion>, number>(this.url, estacion);
  }

  guardarSeleccion(estacion: EstacionUI) {
    localStorage.setItem(this.ESTACION_KEY, estacion.guid);
    this.setItem(this.ESTACION_NAME, estacion.nombre);
  }

  obtenerEstacion() {
    return localStorage.getItem(this.ESTACION_KEY);
  }

  obtenerNombreEstacion(): Observable<string> {
    return this.getItem(this.ESTACION_NAME);
  }

  private getItem(identifier: string): BehaviorSubject<any> {
    const behaviorSubject = this.getBehaviorSubject(identifier);
    const item = localStorage.getItem(identifier);
    behaviorSubject.next(item);
    return behaviorSubject;
  }

  private getBehaviorSubject(identifier: string): BehaviorSubject<any> {
    let behaviorSubject: BehaviorSubject<any> = this.behaviorSubjects.get(identifier);
    if (!behaviorSubject) {
      behaviorSubject = new BehaviorSubject<any>(null);
      this.behaviorSubjects.set(identifier, behaviorSubject);
    }

    return behaviorSubject;
  }

  public setItem(identifier: string, object: string): void {
    localStorage.setItem(identifier, object);
    this.getBehaviorSubject(identifier).next(object);
  }


  borrarEstacionSeleccionada() {
    localStorage.removeItem(this.ESTACION_KEY);
  }

}
