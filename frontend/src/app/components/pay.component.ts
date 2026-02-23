import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../services/account.service';
import { TransactionService } from '../services/transaction.service';

@Component({
  selector: 'app-pay',
  standalone: false,
  templateUrl: './pay.component.html'
})
export class PayComponent implements OnInit {
  upiId = '';
  amount = 0;
  note = '';
  deviceId = 'demo-device-1';
  city = 'Bengaluru';
  loading = false;
  error = '';
  quickAmounts = [199, 499, 999, 1999];
  upiDirectory: any[] = [];

  constructor(private txService: TransactionService, private accountService: AccountService, private router: Router) {}

  ngOnInit() {
    this.accountService.getUpiDirectory().subscribe((items: any) => {
      this.upiDirectory = items;
    });
  }

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
        const serverMessage = typeof error.error === 'string'
          ? error.error
          : error.error?.message || error.error?.detail;

        this.error = serverMessage || `Payment failed (HTTP ${error.status}).`;
      }
    });
  }
}
