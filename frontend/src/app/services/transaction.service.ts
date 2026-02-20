import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

const API_BASE = 'http://localhost:5000/api';

export interface PayRequest {
  upiId: string;
  amount: number;
  note: string;
  deviceId: string;
  city: string;
}

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  constructor(private http: HttpClient) {}

  pay(request: PayRequest) {
    return this.http.post(`${API_BASE}/transactions/pay`, request);
  }

  getHistory() {
    return this.http.get(`${API_BASE}/transactions/history`);
  }
}
