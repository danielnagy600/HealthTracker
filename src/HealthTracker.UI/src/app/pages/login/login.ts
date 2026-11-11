import { Component, inject, signal } from '@angular/core';
import { FormField, FormRoot, email, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../core/auth/auth';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AuthCard } from '../../shared/auth-card';

@Component({
  selector: 'app-login',
  imports: [
    FormField,
    FormRoot,
    RouterLink,
    AuthCard,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  templateUrl: './login.html',
})
export default class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly error = signal<string | null>(null);

  // The model is a plain signal; `form()` builds a field tree on top of it.
  // The second argument is the schema: validation rules + what to do on submit.
  protected readonly loginForm = form(
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
            await this.auth.login(address, password);
            await this.router.navigate(['/']);
          } catch {
            this.error.set('Invalid email or password.');
          }
          return undefined;
        },
      },
    },
  );
}
