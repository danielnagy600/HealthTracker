import { HttpErrorResponse } from '@angular/common/http';

/**
 * A user-facing message for a failed request. Server errors are ProblemDetails
 * (RFC 9457): the specific message is in `detail`; model-binding errors put theirs
 * in `errors` and only a generic sentence in `title`. Anything else gets `fallback`.
 */
export function errorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse) || error.status === 0) {
    return fallback;
  }

  const body: unknown = error.error;
  if (typeof body === 'string' && body) {
    return body;
  }
  if (body && typeof body === 'object') {
    return problemMessage(body) ?? fallback;
  }
  return fallback;
}

function problemMessage(problem: object): string | undefined {
  if ('detail' in problem && typeof problem.detail === 'string' && problem.detail) {
    return problem.detail;
  }
  if ('errors' in problem && problem.errors && typeof problem.errors === 'object') {
    const first = Object.values(problem.errors)
      .flat()
      .find((m) => typeof m === 'string');
    if (first) {
      return first;
    }
  }
  if ('title' in problem && typeof problem.title === 'string') {
    return problem.title;
  }
  return undefined;
}
