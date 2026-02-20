import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

const API_BASE = 'http://localhost:5000/api';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient) {}

  login(email: string, password: string) {
    return this.http.post<{ token: string }>(`${API_BASE}/auth/login`, {
      email,
      password
    }).pipe(
      tap((response) => localStorage.setItem('jwt', response.token))
    );
  }

  register(email: string, password: string) {
    return this.http.post(`${API_BASE}/auth/register`, {
      email,
      password
    });
  }

  logout() {
    localStorage.removeItem('jwt');
  }

  getToken(): string | null {
    return localStorage.getItem('jwt');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
