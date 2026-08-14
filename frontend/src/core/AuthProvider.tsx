import { useCallback, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { apiFetch } from './api';
import { AuthContext } from './auth-context';
import type { AuthContextValue } from './auth-context';
import { session } from './session';

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

/**
 * Bejelentkezés-kezelés. A tokent a localStorage-ban tároljuk (lásd `session`),
 * és React-állapotban is tartjuk, hogy a UI újrarendelődjön be-/kijelentkezéskor.
 * A HTTP-kérésekre az `apiFetch` teszi rá a tokent.
 *
 * Ez az Angular `AuthService` (signalos, `providedIn: 'root'`) megfelelője:
 * egyetlen példány az egész alkalmazásra, itt Context formájában.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => session.getToken());
  const [email, setEmail] = useState<string | null>(() => session.getEmail());

  const register = useCallback(async (email: string, password: string): Promise<void> => {
    // Az ASP.NET Core Identity /register végpontja.
    await apiFetch<void>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });
  }, []);

  const login = useCallback(async (email: string, password: string): Promise<void> => {
    const response = await apiFetch<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });

    session.save(response.accessToken, email);
    setToken(response.accessToken);
    setEmail(email);
  }, []);

  const logout = useCallback((): void => {
    session.clear();
    setToken(null);
    setEmail(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ email, isLoggedIn: token !== null, register, login, logout }),
    [email, token, register, login, logout]
  );

  return <AuthContext value={value}>{children}</AuthContext>;
}
