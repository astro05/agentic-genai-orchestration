import { HttpErrorResponse } from '@angular/common/http';

export function isDeactivatedUserError(err: unknown): boolean {
  if (!(err instanceof HttpErrorResponse) || err.status !== 403) return false;
  const body = err.error;
  if (body == null) return false;
  if (typeof body === 'object' && 'message' in body) {
    const m = (body as { message?: unknown }).message;
    return m === 'user_deactivated';
  }
  if (typeof body === 'string') {
    return body.includes('user_deactivated');
  }
  return false;
}

export function readApiErrorMessage(err: unknown, fallback: string): string {
  if (err instanceof HttpErrorResponse) {
    const e = err.error;
    if (typeof e === 'object' && e !== null && 'message' in e) {
      const m = (e as { message?: unknown }).message;
      if (typeof m === 'string' && m.trim()) return m;
    }
    if (typeof e === 'string' && e.trim()) return e;
  }
  return fallback;
}
