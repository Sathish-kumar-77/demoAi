import { Component, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs';
import { AccountService } from '../services/account.service';
import { PaymentSessionService } from '../services/payment-session.service';

@Component({
  selector: 'app-pay',
  standalone: false,
  templateUrl: './pay.component.html'
})
export class PayComponent implements OnDestroy {
  query = '';
  amount = 0;
  remark = '';
  error = '';
  resolvedPayee: any = null;
  resolving = false;

  private query$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private accountService: AccountService,
    private paymentSession: PaymentSessionService
  ) {
    this.query$.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap((q) => this.accountService.resolvePayee(q)),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (payee: any) => {
        this.error = '';
        this.resolvedPayee = payee;
        this.resolving = false;
      },
      error: (error) => {
        this.resolvedPayee = null;
        this.resolving = false;
        this.error = typeof error.error === 'string' ? error.error : error.error?.message || 'Payee not found';
      }
    });
  }

  onQueryChange(value: string) {
    this.query = value;
    this.resolvedPayee = null;
    this.error = '';

    const trimmed = value.trim();
    const isPhone = trimmed.length === 10 && /^\d+$/.test(trimmed);
    const isUpi = trimmed.includes('@');

    if (isPhone || isUpi) {
      this.resolving = true;
      this.query$.next(trimmed);
    } else {
      this.resolving = false;
    }
  }

  pay() {
    if (this.amount <= 0) {
      this.error = 'Enter valid amount';
      return;
    }

    if (this.resolvedPayee) {
      this.goToPin();
      return;
    }

    const trimmed = this.query.trim();
    const isPhone = trimmed.length === 10 && /^\d+$/.test(trimmed);
    const isUpi = trimmed.includes('@');
    if (!isPhone && !isUpi) {
      this.error = 'Enter valid phone number (10 digits) or UPI ID (name@upi).';
      return;
    }

    this.resolving = true;
    this.accountService.resolvePayee(trimmed).subscribe({
      next: (payee: any) => {
        this.resolving = false;
        this.resolvedPayee = payee;
        this.error = '';
        this.goToPin();
      },
      error: (error) => {
        this.resolving = false;
        this.resolvedPayee = null;
        this.error = typeof error.error === 'string' ? error.error : error.error?.message || 'Payee not found';
      }
    });
  }

  private goToPin() {
    this.paymentSession.setDraft({
      payeeName: this.resolvedPayee.name,
      payeePhone: this.resolvedPayee.phone,
      payeeUpiId: this.resolvedPayee.upiId,
      amount: this.amount,
      remark: this.remark
    });

    this.router.navigate(['/pay/pin']);
  }

  get isPhoneQuery() {
    return this.query.trim().length === 10 && /^\d+$/.test(this.query.trim());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
