import { session } from '../authorization/session';

export const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:8080';

let onUnauthorized: (() => void) | null = null;

export function setUnauthorizedHandler(handler: (() => void) | null): void {
  onUnauthorized = handler;
}

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

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
    if (response.status === 401 && token) {
      onUnauthorized?.();
    }
    throw new ApiError(response.status, await describeError(response, init.method, path));
  }

  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

async function describeError(
  response: Response,
  method: string | undefined,
  path: string,
): Promise<string> {
  const fallback = `${method ?? 'GET'} ${path} → ${response.status}`;
  const text = await response.text().catch(() => '');
  if (!text) {
    return fallback;
  }

  try {
    const parsed: unknown = JSON.parse(text);
    if (typeof parsed === 'string') {
      return parsed;
    }
    if (parsed && typeof parsed === 'object') {
      return problemMessage(parsed) ?? fallback;
    }
  } catch {}

  return text;
}

// ProblemDetails (RFC 9457): the specific message is in `detail`; model-binding
// errors put theirs in `errors` and only a generic sentence in `title`.
function problemMessage(problem: object): string | undefined {
  if ('detail' in problem && typeof problem.detail === 'string' && problem.detail) {
    return problem.detail;
  }
  if ('errors' in problem && problem.errors && typeof problem.errors === 'object') {
    const first = Object.values(problem.errors).flat().find((m) => typeof m === 'string');
    if (first) {
      return first;
    }
  }
  if ('title' in problem && typeof problem.title === 'string') {
    return problem.title;
  }
  return undefined;
}
