import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from './use-auth';

/**
 * Csak bejelentkezett felhasználót enged az adott útvonalra, különben a login-ra
 * irányít. Az Angular `authGuard` (CanActivateFn) megfelelője – Reactben a védett
 * útvonal elemét csomagoljuk be vele.
 */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { isLoggedIn } = useAuth();

  if (!isLoggedIn) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}
