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

  constructor(private txService: TransactionService, private router: Router) {}

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
      error: () => {
        this.loading = false;
        this.error = 'Payment failed. Try again.';
      }
    });
  }
}
