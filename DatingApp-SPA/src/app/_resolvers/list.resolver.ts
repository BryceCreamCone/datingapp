import { Injectable } from '@angular/core';
import { Resolve, Router } from '@angular/router';
import { User } from '../_models/user';
import { Observable, of } from 'rxjs';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ListResolver implements Resolve<User[]> {
  pageNumber = 1;
  pageSize = 5;
  likesParams = 'Likers';

  constructor(
    private userService: UserService,
    private router: Router,
    private alertify: AlertifyService) { }

  resolve(): Observable<User[]> {
    return this.userService.getUsers(this.pageNumber, this.pageSize, null, this.likesParams)
      .pipe(
        catchError(error => {
          this.alertify.error('Cannot retrieve Users at this time');
          this.router.navigate(['/']);
          return of(null);
        })
      );
  }
}