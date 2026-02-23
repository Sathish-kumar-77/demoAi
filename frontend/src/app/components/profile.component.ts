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

  phoneNumber = '';
  upiPin = '';
  otpCode = '';

  bankDirectory: any[] = [];
  linkedAccounts: any[] = [];

  infoMessage = '';
  errorMessage = '';
  snackbarMessage = '';

  constructor(private auth: AuthService, private router: Router, private accountService: AccountService) {}

  ngOnInit() {
    const token = this.auth.getToken();
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || this.email;
      this.name = this.email.split('@')[0];
    }

    this.loadBankDirectory();
    this.loadLinkedAccounts();
  }

  loadBankDirectory() {
    this.accountService.getBankDirectory().subscribe((items: any) => {
      this.bankDirectory = items;
    });
  }

  loadLinkedAccounts() {
    this.accountService.getLinkedAccounts().subscribe((items: any) => {
      this.linkedAccounts = items;
    });
  }

  requestOtp() {
    this.errorMessage = '';
    this.infoMessage = '';

    this.accountService.requestOtp(this.phoneNumber, this.upiPin).subscribe({
      next: (response: any) => {
        this.infoMessage = `${response.message}. Demo OTP: ${response.demoOtp}`;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.extractError(error);
      }
    });
  }

  verifyOtpAndLink() {
    this.errorMessage = '';
    this.infoMessage = '';

    this.accountService.verifyOtp(this.phoneNumber, this.otpCode).subscribe({
      next: (response: any) => {
        this.infoMessage = `${response.message} | UPI ID: ${response.upiId}`;
        this.showSnackbar(`UPI ID created successfully: ${response.upiId}`);
        this.loadLinkedAccounts();
        this.otpCode = '';
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.extractError(error);
      }
    });
  }

  removeLinkedAccount(linkedId: number) {
    this.accountService.removeLinkedAccount(linkedId).subscribe({
      next: () => {
        this.loadLinkedAccounts();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.extractError(error);
      }
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  private extractError(error: HttpErrorResponse) {
    return typeof error.error === 'string' ? error.error : error.error?.message || error.error?.detail || 'Something went wrong';
  }

  private showSnackbar(message: string) {
    this.snackbarMessage = message;
    setTimeout(() => {
      this.snackbarMessage = '';
    }, 3000);
  }
}
