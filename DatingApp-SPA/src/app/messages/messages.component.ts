import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { AlertifyService } from '../_services/alertify.service';
import { AuthService } from '../_services/auth.service';
import { ActivatedRoute } from '@angular/router';
import { UserService } from '../_services/user.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  messageContainer = 'Unread';

  constructor(
    private alertify: AlertifyService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private userService: UserService
  ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.messages = data[`messages`].result;
      this.pagination = data[`messages`].pagination;
    });
  }

  loadMessages() {
    this.userService.getMessages(
      this.authService.decodedToken.nameid,
      this.pagination.currentPage,
      this.pagination.itemsPerPage,
      this.messageContainer).subscribe((res) => {
        console.log(res.result);
        this.messages = res.result;
        this.pagination = res.pagination;
      }, error => this.alertify.error(error));
  }

  showInfo(message: Message): any | void {
    if (this.messageContainer === 'Outbox'
      && message.recipientId !== this.authService.currentUser.id) {
      return {
        photoUrl: message.recipientPhotoUrl,
        knownAs: message.recipientKnownAs
      };
    }
    if (this.messageContainer === 'Inbox' && message.senderId !== this.authService.currentUser.id
      || this.messageContainer === 'Unread' && message.senderId !== this.authService.currentUser.id
      && message.senderId !== this.authService.currentUser.id) {
      return {
        photoUrl: message.senderPhotoUrl,
        knownAs: message.senderKnownAs
      };
    }
  }

  deleteMessage(id: number) {
    this.alertify.confirm('Delete this message?', () => {
      this.userService.deleteMessage(id, this.authService.currentUser.id)
        .subscribe(() => {
          this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
          this.alertify.success('Message has been deleted');
        }, error => this.alertify.error(error));
    });
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadMessages();
  }

  routeToMember(message: Message) {
    let memberId: number;
    this.messageContainer === 'Outbox' ?
      memberId = message.recipientId :
      memberId = message.senderId;
    return memberId;
  }

}
