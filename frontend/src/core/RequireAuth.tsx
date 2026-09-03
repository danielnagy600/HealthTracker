import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from './use-auth';

export function RequireAuth({ children }: { children: ReactNode }) {
  const { isLoggedIn } = useAuth();

  if (!isLoggedIn) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}
