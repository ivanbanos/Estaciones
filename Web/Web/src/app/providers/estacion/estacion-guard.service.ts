import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { EstacionService } from './estacion.service';

@Injectable({
  providedIn: 'root'
})
export class EstacionGuardService implements CanActivate {

  constructor(private readonly estacionesService: EstacionService) { }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean | UrlTree | Observable<boolean | UrlTree> | Promise<boolean | UrlTree> {
    if (this.estacionesService.obtenerEstacion()) {
      return true;
    }
    return false;
  }
}
