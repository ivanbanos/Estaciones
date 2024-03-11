import { Component } from '@angular/core';
import { UserService } from 'src/app/providers/user/user.service';
import { BaseComponent } from '../base/base.component';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ErrorService } from 'src/app/errors/services/error.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent extends BaseComponent {

  username: string;
  password: string;
  showSpinner = false;

  constructor(
    public readonly router: Router,
    public readonly errorService: ErrorService,
    private readonly loginService: UserService,
    public readonly toast: ToastrService) {
    super(router, errorService, toast);
  }

  login() {
    if (this.username && this.password) {
      this.showSpinner = true;
      this.loginService.login(this.username, this.password)
        .subscribe(token => {
          this.showSpinner = false;
          this.router.navigate(['/dashboard']);
        }, error => {
          this.handleException(error);
          this.showSpinner = false;
        }
        );
    }
  }

}
