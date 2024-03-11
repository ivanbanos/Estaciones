import { Component, OnInit } from '@angular/core';
import { BaseComponent } from '../base/base.component';
import { Router } from '@angular/router';
import { ErrorService } from 'src/app/errors/services/error.service';
import { UserService } from 'src/app/providers/user/user.service';
import { ToastrService } from 'ngx-toastr';

export interface PeriodicElement {
  name: string;
  position: number;
  weight: number;
  symbol: string;
}

const ELEMENT_DATA: PeriodicElement[] = [
  { position: 1, name: 'Hydrogen', weight: 1.0079, symbol: 'H' },
  { position: 2, name: 'Helium', weight: 4.0026, symbol: 'He' },
  { position: 3, name: 'Lithium', weight: 6.941, symbol: 'Li' },
  { position: 4, name: 'Beryllium', weight: 9.0122, symbol: 'Be' },
  { position: 5, name: 'Boron', weight: 10.811, symbol: 'B' },
  { position: 6, name: 'Carbon', weight: 12.0107, symbol: 'C' },
  { position: 7, name: 'Nitrogen', weight: 14.0067, symbol: 'N' },
  { position: 8, name: 'Oxygen', weight: 15.9994, symbol: 'O' },
  { position: 9, name: 'Fluorine', weight: 18.9984, symbol: 'F' },
  { position: 10, name: 'Neon', weight: 20.1797, symbol: 'Ne' },
];

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent extends BaseComponent {

  username: string;
  password: string;
  name: string;
  showSpinner = false;
  displayedColumns: string[] = ['position', 'name', 'weight', 'symbol'];
  dataSource = ELEMENT_DATA;

  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    private readonly loginService: UserService,
    public readonly toast: ToastrService) {
    super(router, errorService, toast);
  }

  register() {
    if (this.username && this.password && this.name) {
      this.showSpinner = true;
      this.loginService.register(this.username, this.name, this.password)
        .subscribe(res => {
          this.showSpinner = false;
          this.toast.success('usuario creado exitosamente');
        }, error => {
          this.handleException(error);
          this.showSpinner = false;
        });
    }
  }
}
