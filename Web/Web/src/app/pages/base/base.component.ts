import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ErrorService } from 'src/app/errors/services/error.service';

@Component({
  selector: 'app-base',
  templateUrl: './base.component.html',
  styleUrls: ['./base.component.sass']
})
export class BaseComponent {

  errorMessage: string;

  constructor(
    public router: Router,
    public errorService: ErrorService,
    public toast: ToastrService) { }

  openPage(routename: string) {
      this.router.navigateByUrl(`${routename}`);
  }

  handleException(error: Error) {
    const handledError = this.errorService.handleException(error);
    if (handledError.toastNotifiation) {
      this.toast.error(handledError.message);
    } else {
      this.errorMessage = handledError.message;
    }
  }

}
