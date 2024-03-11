import { Injectable } from '@angular/core';
import { HttpService } from '../base/http.service';
import { Observable } from 'rxjs';
import { environment as ENV } from '../../../environments/environment';
import { tap, map } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { User } from 'src/app/models/User';
import { GuidService } from '../base/guid.service';
import { Token } from 'src/app/models/Token';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  url: string = ENV.serverUrl + '/usuarios';

  constructor(
    private readonly http: HttpService,
    private readonly guidService: GuidService) { }

  login(username: string, password: string): Observable<string> {
    return this.http.getResource<Token>(this.url, username, password)
      .pipe(map(resp => {
        localStorage.setItem('token', resp.token);
        localStorage.setItem('username', username);
        return resp.token;
      }));
  }

  register(username: string, name: string, password: string) {
    const user = new User(this.guidService.generateGuid(),
      username, name, password);
    return this.http.post(this.url, user);
  }

  logout() {
    localStorage.clear();
  }
}
