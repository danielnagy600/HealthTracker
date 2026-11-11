import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { API_BASE_URL } from '../http/api-base';
import { Auth } from './auth';
import { session } from './session';

/**
 * Runs for every HttpClient request (including httpResource):
 *  - turns `/api/...` into `${API_BASE}/api/...`,
 *  - adds the bearer token,
 *  - on 401 signs the user out and sends them to /login.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(Auth);
  const router = inject(Router);
  const token = session.getToken();

  const authorized = request.clone({
    url: request.url.startsWith('/') ? `${inject(API_BASE_URL)}${request.url}` : request.url,
    setHeaders: token ? { Authorization: `Bearer ${token}` } : {},
  });

  return next(authorized).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && token) {
        auth.logout();
        void router.navigate(['/login']);
      }
      return throwError(() => error);
    }),
  );
};
