import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';

import { Observable } from 'rxjs';

export class HttpAuthInterceptor implements HttpInterceptor {

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token: string = localStorage.getItem('token');

        let req = request;

        if (token) {
        request = req.clone({
            setHeaders: {
            authorization: `Bearer ${ token }`
            }
        });
        }

        return next.handle(request);
    }
   }