import { Component, OnDestroy, ChangeDetectorRef, OnInit } from '@angular/core';
import { BaseComponent } from 'src/app/pages/base/base.component';
import { Router } from '@angular/router';
import { ErrorService } from 'src/app/errors/services/error.service';
import { ToastrService } from 'ngx-toastr';
import { routes } from '../../../navigation/routes';
import { MediaMatcher } from '@angular/cdk/layout';
import { UserService } from 'src/app/providers/user/user.service';
import { Observable } from 'rxjs';
import { EstacionService } from 'src/app/providers/estacion/estacion.service';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent extends BaseComponent implements OnDestroy, OnInit {

  opened = true;
  availableroutes = routes;
  mobileQuery: MediaQueryList;
  estacion: string;

  private mobileQueryListener: () => void;

  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    public readonly toast: ToastrService,
    private readonly changeDetectorRef: ChangeDetectorRef,
    private readonly userService: UserService,
    private readonly estacionService: EstacionService,
    private readonly media: MediaMatcher) {
    super(router, errorService, toast);
    this.mobileQuery = media.matchMedia('(max-width: 600px)');
    this.mobileQueryListener = () => changeDetectorRef.detectChanges();
    this.mobileQuery.addListener(this.mobileQueryListener);
  }

  ngOnInit(): void {
    this.estacionService.obtenerNombreEstacion().subscribe(estacion => this.estacion = estacion, error => this.handleException(error));
  }

  getRoutes(): string[] {
    return this.availableroutes
      .find(route => route.path === 'dashboard')
      .children.map(route => route.path);
  }

  getTitle(): string {
    return this.router.url === 'dashboard' ? 'Dashboard' : this.router.url.slice(11);
  }

  ngOnDestroy(): void {
    this.mobileQuery.removeListener(this.mobileQueryListener);
  }

  logout(): void {
    this.userService.logout();
    this.openPage('login');
  }

}
