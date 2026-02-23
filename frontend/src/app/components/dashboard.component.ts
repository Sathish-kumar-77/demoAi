import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../services/account.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  balance: number | null = null;
  balanceError = '';

  constructor(private router: Router, private auth: AuthService, private accountService: AccountService) {}

  navigate(path: string) {
    this.router.navigate([path]);
  }

  viewBalance() {
    this.balanceError = '';
    const pin = prompt('Enter UPI PIN to view balance');
    if (!pin) {
      return;
    }

    this.accountService.getBalance(pin).subscribe({
      next: (response: any) => {
        this.balance = response.balance;
      },
      error: (error: HttpErrorResponse) => {
        this.balance = null;
        this.balanceError = typeof error.error === 'string' ? error.error : error.error?.message || 'Unable to fetch balance';
      }
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
