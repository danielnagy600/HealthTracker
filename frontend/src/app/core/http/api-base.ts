import { InjectionToken } from '@angular/core';

/** The backend's address. A token (instead of the bare constant) so tests can provide their own. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => API_BASE,
});
