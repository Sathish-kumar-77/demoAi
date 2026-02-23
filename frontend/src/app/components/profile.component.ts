import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../services/account.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  email = 'upi.user@example.com';
  name = 'UPI User';
  linkedMessage = '';
  linkError = '';
  phoneNumber = '';
  upiPin = '';
  bankDirectory: any[] = [];

  constructor(private auth: AuthService, private router: Router, private accountService: AccountService) {}

  ngOnInit() {
    const token = this.auth.getToken();
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || this.email;
      this.name = this.email.split('@')[0];
    }

    this.accountService.getDirectory().subscribe((items: any) => {
      this.bankDirectory = items;
    });
  }

  linkBankAccount() {
    this.linkError = '';
    this.linkedMessage = '';

    this.accountService.linkAccount(this.phoneNumber, this.upiPin).subscribe({
      next: (response: any) => {
        this.linkedMessage = `Linked: ${response.bankName} - ${response.accountNumberMasked}`;
      },
      error: (error: HttpErrorResponse) => {
        this.linkError = typeof error.error === 'string' ? error.error : error.error?.message || 'Unable to link account';
      }
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
