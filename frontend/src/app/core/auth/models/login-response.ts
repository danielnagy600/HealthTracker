/** Body of POST /api/auth/login (ASP.NET Core Identity bearer token). */
export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}
