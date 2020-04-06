import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-member-details',
  templateUrl: './member-details.component.html',
  styleUrls: ['./member-details.component.css']
})
export class MemberDetailsComponent implements OnInit {
  user: User;

  constructor(private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data[`user`];
    });
  }

  // loadUser() {
  //   this.userService.getUser(+this.route.snapshot.params[`id`])
  //     .subscribe((user: User) => {
  //       this.user = user;
  //     }, error => {
  //       this.alertify.error(error);
  //     });
  // }

}
