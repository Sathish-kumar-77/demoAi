import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-balance-pin',
  standalone: false,
  templateUrl: './balance-pin.component.html'
})
export class BalancePinComponent {
  pin = '';
  loading = false;
  error = '';
  readonly maxDigits = 6;

  constructor(private accountService: AccountService, private router: Router) {}

  appendDigit(digit: string) {
    if (this.loading || this.pin.length >= this.maxDigits) return;
    this.pin += digit;
    this.error = '';
  }

  backspace() {
    if (this.loading || this.pin.length === 0) return;
    this.pin = this.pin.slice(0, -1);
  }

  clearPin() {
    if (this.loading) return;
    this.pin = '';
    this.error = '';
  }

  checkBalance() {
    if (this.loading || this.pin.length < 4) return;

    this.loading = true;
    this.error = '';

    this.accountService.getBalance(this.pin).subscribe({
      next: (response: any) => {
        this.loading = false;
        sessionStorage.setItem('latestBalance', String(response.balance));
        this.router.navigate(['/dashboard']);
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.error = typeof error.error === 'string' ? error.error : error.error?.message || 'Unable to fetch balance';
      }
    });
  }
}
