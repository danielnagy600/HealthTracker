import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="card">
      <h1>💧 HealthTracker</h1>
      <h2>Sign in</h2>

      <form (ngSubmit)="submit()">
        <label>Email
          <input type="email" name="email" [(ngModel)]="email" required autocomplete="username" />
        </label>
        <label>Password
          <input type="password" name="password" [(ngModel)]="password" required autocomplete="current-password" />
        </label>

        @if (error) { <p class="error">{{ error }}</p> }

        <button type="submit" [disabled]="loading">
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>

      <p class="muted">No account yet? <a routerLink="/register">Register</a></p>
    </div>
  `
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  error: string | null = null;
  loading = false;

  submit(): void {
    this.loading = true;
    this.error = null;
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => {
        this.error = 'Invalid email or password.';
        this.loading = false;
      }
    });
  }
}
