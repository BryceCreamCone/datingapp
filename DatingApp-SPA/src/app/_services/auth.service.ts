import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;
  photoUrl = new BehaviorSubject<string>('../../assets/darshni-priya-ms-49bDBMQ04jg-unsplash.jpg');
  currentPhotoUrl = this.photoUrl.asObservable();

  constructor(private http: HttpClient) { }

  changeMainPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl);
  }

  login(user: User) {
    return this.http.post(this.baseUrl + 'login', user)
      .pipe(
        map((response: any) => {
          const userObject = response;
          if (userObject) {
            localStorage.setItem('token', userObject.token);
            localStorage.setItem('user', JSON.stringify(userObject.user));
            this.decodedToken = this.jwtHelper.decodeToken(userObject.token);
            this.currentUser = userObject.user;
            this.changeMainPhoto(this.currentUser.photoUrl);
          }
        })
      );
  }

  register(user: User) {
    return this.http.post(this.baseUrl + 'register', user);
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }
}
