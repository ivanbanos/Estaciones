import { ReporteFiscalComponent } from './../pages/reporte-fiscal/reporte-fiscal.component';
import { TurnosComponent } from './../pages/turnos/turnos.component';
import { Routes } from '@angular/router';

import { LoginComponent } from '../pages/login/login.component';

import { RegisterComponent } from '../pages/register/register.component';

import { IndexComponent } from '../pages/Index/Index.component';

import { ClienteComponent } from '../pages/cliente/cliente.component';

import { OrdenDeDespachoComponent } from '../pages/orden-de-despacho/orden-de-despacho.component';
import { TercerosComponent } from '../pages/terceros/terceros.component';

import { FacturasComponent } from '../pages/facturas/facturas.component';
import { TerceroComponent } from '../pages/terceros/tercero/tercero.component';
import { BuscarEstacionesComponent } from '../pages/estacion/buscar/buscar-estaciones.component';
import { AgregarEstacionesComponent } from '../pages/estacion/agregar/agregar-estaciones.component';
import { ResolucionComponent } from '../pages/resolucion/resolucion.component';
import { AgregarResolucionComponent } from '../pages/resolucion/agregar-resolucion/agregar-resolucion.component';
import { EstacionGuardService as EstacionesGuard } from '../providers/estacion/estacion-guard.service';
import { CanastillaComponent } from '../pages/Canastillas/canastilla/canastilla.component';
import { CanastillasComponent } from '../pages/Canastillas/canastillas.component';
import { FacturasCanastillasComponent } from '../pages/facturascanastillas/facturascanastillas.component';
import { FacturaCanastillaDetalleComponent } from '../pages/facturascanastillas/factura-canastilla-detalle/factura-canastilla-detalle.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'dashboard', component: IndexComponent, children: [
      { path: 'facturas', component: FacturasComponent, canActivate: [EstacionesGuard] },
      { path: 'ordenes', component: ClienteComponent, canActivate: [EstacionesGuard] },
      { path: 'ordenesDeDespacho', component: OrdenDeDespachoComponent, canActivate: [EstacionesGuard] },
      { path: 'estaciones', component: BuscarEstacionesComponent },
      { path: 'estacion', component: AgregarEstacionesComponent },
      { path: 'estacion/:idEstacion', component: AgregarEstacionesComponent, canActivate: [EstacionesGuard] },
      { path: 'terceros', component: TercerosComponent, canActivate: [EstacionesGuard] },
      { path: 'tercero/:idTercero', component: TerceroComponent, canActivate: [EstacionesGuard] },
      { path: 'tercero', component: TerceroComponent, canActivate: [EstacionesGuard] },
      { path: 'canastillas', component: CanastillasComponent, canActivate: [EstacionesGuard] },
      { path: 'canastilla/:idCanastilla', component: CanastillaComponent, canActivate: [EstacionesGuard] },
      { path: 'facturascanastillas', component: FacturasCanastillasComponent, canActivate: [EstacionesGuard] },
      { path: 'facturacanastilla/:idCanastilla', component: FacturaCanastillaDetalleComponent, canActivate: [EstacionesGuard] },
      { path: 'canastilla', component: CanastillaComponent, canActivate: [EstacionesGuard] },
      { path: 'resoluciones', component: ResolucionComponent, canActivate: [EstacionesGuard] },
      { path: 'resolucion', component: AgregarResolucionComponent, canActivate: [EstacionesGuard] },
      { path: 'resolucion/:tipo', component: AgregarResolucionComponent, canActivate: [EstacionesGuard] },
      { path: 'reporte-consolidado', component: ReporteFiscalComponent, canActivate: [EstacionesGuard] },
      { path: 'reporte-turnos', component: TurnosComponent, canActivate: [EstacionesGuard] },
      { path: '**', redirectTo: 'estaciones' }
    ]
  },
  { path: '', component: LoginComponent },
  { path: '**', redirectTo: 'login' }
];
