import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, tap, throwError } from 'rxjs';

const API_BASES = ['http://localhost:5000/api', 'https://localhost:5001/api'];

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient) {}

  private postWithFallback<T>(path: string, body: unknown) {
    return this.http.post<T>(`${API_BASES[0]}${path}`, body).pipe(
      catchError((error) => {
        if (error.status === 0 || error.status === 404) {
          return this.http.post<T>(`${API_BASES[1]}${path}`, body);
        }
        return throwError(() => error);
      })
    );
  }

  login(email: string, password: string) {
    return this.postWithFallback<{ token: string }>('/auth/login', {
      email,
      password
    }).pipe(
      tap((response) => localStorage.setItem('jwt', response.token))
    );
  }

  register(email: string, password: string) {
    return this.postWithFallback('/auth/register', {
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
