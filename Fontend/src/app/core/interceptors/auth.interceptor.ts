import {
  HttpInterceptorFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthSessionService } from '../auth/auth-session.service';

function isAuthPublicUrl(url: string): boolean {
  const u = url.toLowerCase();
  return (
    u.includes('/auth/login') ||
    u.includes('/auth/register') ||
    u.includes('/auth/refresh-token') ||
    u.includes('/auth/forgot-password') ||
    u.includes('/auth/reset-password') ||
    u.includes('/account/login') ||
    u.includes('/account/register')
  );
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const session = inject(AuthSessionService);
  const token = session.getAccessToken();

  const authReq =
    token && !req.headers.has('Authorization')
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (
        err.status !== 401 ||
        isAuthPublicUrl(req.url) ||
        req.headers.has('X-Skip-Auth-Refresh')
      ) {
        return throwError(() => err);
      }

      return from(session.refreshAccessToken()).pipe(
        switchMap((ok) => {
          if (!ok) {
            session.logout(true);
            return throwError(() => err);
          }
          const nextToken = session.getAccessToken();
          const retried = req.clone({
            setHeaders: nextToken
              ? { Authorization: `Bearer ${nextToken}` }
              : {},
          });
          return next(retried);
        }),
      );
    }),
  );
};
