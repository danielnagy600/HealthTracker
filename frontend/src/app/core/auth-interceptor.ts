import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth';

/**
 * Minden kimenő HTTP-kérésre ráteszi a bearer tokent, ha be vagyunk jelentkezve.
 * Így a komponenseknek nem kell a tokennel foglalkozniuk.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).token();
  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(req);
};
