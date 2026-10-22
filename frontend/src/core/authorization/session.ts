const tokenKey = 'ht_token';
const emailKey = 'ht_email';

export const session = {
  getToken: (): string | null => localStorage.getItem(tokenKey),

  getEmail: (): string | null => localStorage.getItem(emailKey),

  save(token: string, email: string): void {
    localStorage.setItem(tokenKey, token);
    localStorage.setItem(emailKey, email);
  },

  clear(): void {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(emailKey);
  }
};
