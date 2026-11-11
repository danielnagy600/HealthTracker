import { Component, inject, signal } from '@angular/core';
import { FormField, FormRoot, email, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../core/auth/auth';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AuthCard } from '../../shared/auth-card';

@Component({
  selector: 'app-register',
  imports: [
    FormField,
    FormRoot,
    RouterLink,
    AuthCard,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  templateUrl: './register.html',
})
export default class Register {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly error = signal<string | null>(null);

  protected readonly registerForm = form(
    signal({ email: '', password: '' }),
    (path) => {
      required(path.email, { message: 'Enter your email.' });
      email(path.email, { message: 'This is not a valid email address.' });
      required(path.password, { message: 'Enter your password.' });
    },
    {
      submission: {
        action: async (fields) => {
          const { email: address, password } = fields().value();
          this.error.set(null);

          try {
            await this.auth.register(address, password);
          } catch {
            this.error.set('Registration failed. Check the email format and password rules.');
            return undefined;
          }

          try {
            await this.auth.login(address, password);
            await this.router.navigate(['/']);
          } catch {
            await this.router.navigate(['/login']);
          }
          return undefined;
        },
      },
    },
  );
}
