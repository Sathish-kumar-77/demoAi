import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TransactionService } from '../services/transaction.service';

@Component({
  selector: 'app-pay',
  standalone: false,
  templateUrl: './pay.component.html'
})
export class PayComponent {
  upiId = '';
  amount = 0;
  note = '';
  deviceId = 'demo-device-1';
  city = 'Bengaluru';
  loading = false;
  error = '';
  quickAmounts = [199, 499, 999, 1999];

  constructor(private txService: TransactionService, private router: Router) {}

  pickAmount(value: number) {
    this.amount = value;
  }

  submit() {
    this.loading = true;
    this.error = '';
    this.txService.pay({
      upiId: this.upiId,
      amount: this.amount,
      note: this.note,
      deviceId: this.deviceId,
      city: this.city
    }).subscribe({
      next: (response) => {
        this.loading = false;
        localStorage.setItem('lastResult', JSON.stringify(response));
        this.router.navigate(['/result']);
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        if (error.status === 0) {
          this.error = 'API unreachable. Ensure backend is running at http://localhost:5000.';
          return;
        }

        const serverMessage = typeof error.error === 'string'
          ? error.error
          : error.error?.message || error.error?.detail;

        this.error = serverMessage || `Payment failed (HTTP ${error.status}).`;
      }
    });
  }
}
