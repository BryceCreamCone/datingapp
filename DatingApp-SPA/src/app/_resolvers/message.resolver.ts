import { Injectable } from '@angular/core';
import { Resolve, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { catchError } from 'rxjs/operators';
import { Message } from '../_models/message';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MessageResolver implements Resolve<Message[]> {
  pageNumber = 1;
  pageSize = 5;
  messageContainer = 'Unread';

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private router: Router,
    private alertify: AlertifyService) { }

  resolve(): Observable<Message[]> {
    return this.userService.getMessages(
      this.authService.currentUser.id,
      this.pageNumber,
      this.pageSize,
      this.messageContainer).pipe(
        catchError(error => {
          this.alertify.error('Cannot retrieve Messages at this time');
          this.router.navigate(['/']);
          return of(null);
        })
      );
  }
}
