import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

const API_BASES = ['http://localhost:5000/api', 'https://localhost:5001/api'];

@Injectable({
  providedIn: 'root'
})
export class AccountService {
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

  getDirectory() {
    return this.getWithFallback('/accounts/directory');
  }

  linkAccount(phoneNumber: string, upiPin: string) {
    return this.postWithFallback('/accounts/link', { phoneNumber, upiPin });
  }

  getBalance(upiPin: string) {
    return this.postWithFallback('/accounts/balance', { phoneNumber: '', upiPin });
  }
}
