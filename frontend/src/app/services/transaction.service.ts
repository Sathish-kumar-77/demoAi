import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

const API_BASES = ['http://localhost:5000/api', 'https://localhost:5001/api'];

export interface PayRequest {
  payeeUpiId: string;
  payeePhone: string;
  amount: number;
  remark: string;
  deviceId: string;
  hourOfDay: number;
  channel: string;
  faceImageBase64?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  constructor(private http: HttpClient) {}

  private postWithFallback(path: string, body: unknown) {
    return this.http.post(`${API_BASES[0]}${path}`, body).pipe(
      catchError((error) => {
        if (error.status === 0 || error.status === 404) {
          return this.http.post(`${API_BASES[1]}${path}`, body);
        }
        return throwError(() => error);
      })
    );
  }

  private getWithFallback(path: string) {
    return this.http.get(`${API_BASES[0]}${path}`).pipe(
      catchError((error) => {
        if (error.status === 0 || error.status === 404) {
          return this.http.get(`${API_BASES[1]}${path}`);
        }
        return throwError(() => error);
      })
    );
  }

  pay(request: PayRequest) {
    return this.postWithFallback('/transactions/pay', request);
  }

  refund(transactionId: number) {
    return this.postWithFallback(`/transactions/${transactionId}/refund`, {});
  }

  getHistory() {
    return this.getWithFallback('/transactions/history');
  }
}
