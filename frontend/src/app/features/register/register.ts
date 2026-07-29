import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="card">
      <h1>💧 HealthTracker</h1>
      <h2>Create account</h2>

      <form (ngSubmit)="submit()">
        <label>Email
          <input type="email" name="email" [(ngModel)]="email" required autocomplete="username" />
        </label>
        <label>Password
          <input type="password" name="password" [(ngModel)]="password" required autocomplete="new-password" />
        </label>

        <p class="hint">
          Min. 6 characters, with upper- &amp; lowercase, a digit and a symbol (e.g. <code>Passw0rd!</code>).
        </p>

        @if (error) { <p class="error">{{ error }}</p> }

        <button type="submit" [disabled]="loading">
          {{ loading ? 'Creating…' : 'Register' }}
        </button>
      </form>

      <p class="muted">Already registered? <a routerLink="/login">Sign in</a></p>
    </div>
  `
})
export class Register {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  error: string | null = null;
  loading = false;

  submit(): void {
    this.loading = true;
    this.error = null;
    this.auth.register(this.email, this.password).subscribe({
      next: () => {
        // Sikeres regisztráció után rögtön be is jelentkeztetjük.
        this.auth.login(this.email, this.password).subscribe({
          next: () => this.router.navigate(['/']),
          error: () => this.router.navigate(['/login'])
        });
      },
      error: () => {
        this.error = 'Registration failed. Check the email format and password rules.';
        this.loading = false;
      }
    });
  }
}
