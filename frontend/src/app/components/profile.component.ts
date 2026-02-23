import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  email = 'upi.user@example.com';
  name = 'UPI User';
  balance = 25430.75;

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit() {
    const token = this.auth.getToken();
    if (!token) {
      return;
    }

    const payload = JSON.parse(atob(token.split('.')[1]));
    this.email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || this.email;
    this.name = this.email.split('@')[0];
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
