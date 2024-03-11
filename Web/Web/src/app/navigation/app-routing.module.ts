import { ClienteComponent } from '../pages/cliente/cliente.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoginComponent } from '../pages/login/login.component';
import { IndexComponent } from '../pages/Index/Index.component';
import { IslaComponent } from '../pages/Isla/Isla.component';
import { RegisterComponent } from '../pages/register/register.component';
import { routes } from '../navigation/routes';

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
