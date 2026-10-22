import { createContext } from 'react';

export interface AuthContextValue {
  email: string | null;
  isLoggedIn: boolean;
  register: (email: string, password: string) => Promise<void>;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
