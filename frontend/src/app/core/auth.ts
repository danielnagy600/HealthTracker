import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { API_BASE } from './api';

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

/**
 * Bejelentkezés-kezelés. A tokent a localStorage-ban tároljuk, és signalokban
 * tartjuk a UI-nak. A HTTP-kérésekre az auth-interceptor teszi rá a tokent.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly tokenKey = 'ht_token';
  private readonly emailKey = 'ht_email';

  readonly token = signal<string | null>(localStorage.getItem(this.tokenKey));
  readonly email = signal<string | null>(localStorage.getItem(this.emailKey));
  readonly isLoggedIn = computed(() => this.token() !== null);

  register(email: string, password: string): Observable<unknown> {
    // Az ASP.NET Core Identity /register végpontja.
    return this.http.post(`${API_BASE}/api/auth/register`, { email, password });
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${API_BASE}/api/auth/login`, { email, password })
      .pipe(tap((res) => this.store(res.accessToken, email)));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.emailKey);
    this.token.set(null);
    this.email.set(null);
  }

  private store(token: string, email: string): void {
    localStorage.setItem(this.tokenKey, token);
    localStorage.setItem(this.emailKey, email);
    this.token.set(token);
    this.email.set(email);
  }
}
