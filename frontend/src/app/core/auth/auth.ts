import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LoginResponse } from './models/login-response';
import { session } from './session';

/** The signed-in state of the app. One instance for the whole app (`providedIn: 'root'`). */
@Injectable({ providedIn: 'root' })
export class Auth {
  private readonly http = inject(HttpClient);

  // Writable signals stay private; outside code only reads them.
  private readonly token = signal<string | null>(session.getToken());
  private readonly emailState = signal<string | null>(session.getEmail());

  readonly email = this.emailState.asReadonly();
  readonly isLoggedIn = computed(() => this.token() !== null);

  async register(email: string, password: string): Promise<void> {
    await firstValueFrom(this.http.post<void>('/api/auth/register', { email, password }));
  }

  async login(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>('/api/auth/login', { email, password }),
    );

    session.save(response.accessToken, email);
    this.token.set(response.accessToken);
    this.emailState.set(email);
  }

  logout(): void {
    session.clear();
    this.token.set(null);
    this.emailState.set(null);
  }
}
