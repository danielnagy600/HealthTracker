import { session } from './session';

// A backend API alapcíme. Build-időben cserélhető: VITE_API_BASE env-változó
// (lásd .env.development). Ha nincs megadva, a helyi fejlesztői cím az alapértelmezés.
export const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:8080';

/** Sikertelen HTTP-válasz. A `status`-ból tudják a komponensek, mi történt. */
export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

/**
 * Az egyetlen hely, ahol HTTP-t hívunk. Minden kimenő kérésre ráteszi a bearer
 * tokent, ha be vagyunk jelentkezve – ez az Angular auth-interceptor megfelelője –,
 * így a komponenseknek nem kell a tokennel foglalkozniuk.
 */
export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  if (init.body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }

  const token = session.getToken();
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE}${path}`, { ...init, headers });
  if (!response.ok) {
    throw new ApiError(response.status, `${init.method ?? 'GET'} ${path} → ${response.status}`);
  }

  // Van végpont, ami üres törzzsel válaszol (pl. /api/auth/register), ezért nem
  // hívhatunk vakon response.json()-t.
  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}
