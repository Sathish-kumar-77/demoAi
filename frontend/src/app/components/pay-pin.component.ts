import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { PaymentSessionService } from '../services/payment-session.service';
import { TransactionService } from '../services/transaction.service';

@Component({
  selector: 'app-pay-pin',
  standalone: false,
  templateUrl: './pay-pin.component.html'
})
export class PayPinComponent {
  pin = '';
  loading = false;
  error = '';
  draft = this.paymentSession.getDraft();

  constructor(
    private paymentSession: PaymentSessionService,
    private txService: TransactionService,
    private router: Router
  ) {
    if (!this.draft) {
      this.router.navigate(['/pay']);
    }
  }

  confirmPay() {
    if (!this.draft || !this.pin) {
      return;
    }

    this.loading = true;
    this.error = '';

    this.txService.pay({
      payeeUpiId: this.draft.payeeUpiId,
      payeePhone: this.draft.payeePhone,
      amount: this.draft.amount,
      remark: this.draft.remark,
      deviceId: 'demo-device-1',
      hourOfDay: new Date().getHours(),
      channel: 'UPI_APP'
    }).subscribe({
      next: (response) => {
        this.loading = false;
        this.paymentSession.clear();
        localStorage.setItem('lastResult', JSON.stringify(response));
        this.router.navigate(['/result']);
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.error = typeof error.error === 'string' ? error.error : error.error?.message || 'Payment failed';
      }
    });
  }
}
