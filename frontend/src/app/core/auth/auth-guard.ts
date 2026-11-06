import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from './auth';

/** Protected routes: without a session, redirect to /login. */
export const authGuard: CanActivateFn = () =>
  inject(Auth).isLoggedIn() || inject(Router).createUrlTree(['/login']);
