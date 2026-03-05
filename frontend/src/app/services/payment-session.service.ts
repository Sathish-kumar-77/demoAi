import { Injectable } from '@angular/core';

export interface PaymentDraft {
  payeeName: string;
  payeePhone: string;
  payeeUpiId: string;
  amount: number;
  remark: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentSessionService {
  private draft: PaymentDraft | null = null;

  setDraft(draft: PaymentDraft) {
    this.draft = draft;
  }

  getDraft() {
    return this.draft;
  }

  clear() {
    this.draft = null;
  }
}
