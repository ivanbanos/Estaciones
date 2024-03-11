import { ReporteFiscalComponent } from './pages/reporte-fiscal/reporte-fiscal.component';
import { TurnosComponent } from './pages/turnos/turnos.component';
import { TerceroComponent } from './pages/terceros/tercero/tercero.component';
import { TercerosComponent } from './pages/terceros/terceros.component';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './navigation/app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './pages/login/login.component';
import { FlexLayoutModule } from '@angular/flex-layout';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BaseComponent } from './pages/base/base.component';
import { ErrorsModule } from './errors/errors.module';
import { ToastrModule } from 'ngx-toastr';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import {
  MatButtonModule, MatCardModule, MatDialogModule, MatInputModule, MatTableModule,
  MatToolbarModule, MatMenuModule, MatIconModule, MatProgressSpinnerModule, MatSidenavModule,
  MatCheckboxModule, MatListModule, MatGridList, MatGridListModule, MatDatepicker,
  MatDatepickerModule, MatNativeDateModule, MatSelectModule, MatRadioModule
} from '@angular/material';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorInterceptor } from './errors/services/http-error-interceptor';
import { HttpAuthInterceptor } from './providers/auth/HttpAuthInterceptor';
import { RegisterComponent } from './pages/register/register.component';
import { TableComponent } from './components/base/table/table.component';
import { MenuComponent } from './components/base/menu/menu.component';
import { IndexComponent } from './pages/Index/Index.component';
import { IslaComponent } from './pages/Isla/Isla.component';
import { ClienteComponent } from './pages/cliente/cliente.component';
import { OrdenDeDespachoComponent } from './pages/orden-de-despacho/orden-de-despacho.component';
import { FacturasComponent } from './pages/facturas/facturas.component';
import { InfoBusquedaComponent } from './components/base/info-busqueda/info-busqueda.component';
import { ResolucionComponent } from './pages/resolucion/resolucion.component';
import { BuscarEstacionesComponent } from './pages/estacion/buscar/buscar-estaciones.component';
import { AgregarEstacionesComponent } from './pages/estacion/agregar/agregar-estaciones.component';
import { FacturaItemComponent } from './components/factura-item/factura-item.component';
import { EstacionItemComponent } from './components/estacion-item/estacion-item.component';
import { AgregarResolucionComponent } from './pages/resolucion/agregar-resolucion/agregar-resolucion.component';
import { OrdenItemComponent } from './components/orden-item/orden-item.component';
import { CanastillasComponent } from './pages/Canastillas/canastillas.component';
import { CanastillaComponent } from './pages/Canastillas/canastilla/canastilla.component';
import { FacturasCanastillasComponent } from './pages/facturascanastillas/facturascanastillas.component';
import { FacturaCanastillaDetalleComponent } from './pages/facturascanastillas/factura-canastilla-detalle/factura-canastilla-detalle.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    BaseComponent,
    RegisterComponent,
    TableComponent,
    IndexComponent,
    MenuComponent,
    IslaComponent,
    ClienteComponent,
    OrdenDeDespachoComponent,
    TercerosComponent,
    TerceroComponent,
    CanastillasComponent,
    CanastillaComponent,
    FacturasCanastillasComponent,
    FacturaCanastillaDetalleComponent,
    FacturasComponent,
    InfoBusquedaComponent,
    BuscarEstacionesComponent,
    AgregarEstacionesComponent,
    FacturaItemComponent,
    OrdenItemComponent,
    EstacionItemComponent,
    ResolucionComponent,
    AgregarResolucionComponent,
    ReporteFiscalComponent,
    TurnosComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule,
    FlexLayoutModule,
    HttpClientModule,
    ErrorsModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot(),
    MatToolbarModule,
    MatButtonModule,
    MatCardModule,
    MatInputModule,
    MatDialogModule,
    MatTableModule,
    MatMenuModule,
    MatIconModule,
    MatRadioModule,
    MatProgressSpinnerModule,
    MatSidenavModule,
    MatCheckboxModule,
    MatListModule,
    MatGridListModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule
  ],
  providers: [{
    provide: HTTP_INTERCEPTORS,
    useClass: HttpErrorInterceptor,
    multi: true
  }, {
    provide: HTTP_INTERCEPTORS,
    useClass: HttpAuthInterceptor,
    multi: true
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
