import { createContext } from 'react';

/** A bejelentkezési állapot és a hozzá tartozó műveletek. */
export interface AuthContextValue {
  email: string | null;
  isLoggedIn: boolean;
  register: (email: string, password: string) => Promise<void>;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

// Külön fájlban a providertől és a hooktól: így minden fájl vagy komponenst
// exportál, vagy egyebet – ettől működik rendesen a Fast Refresh.
export const AuthContext = createContext<AuthContextValue | null>(null);
