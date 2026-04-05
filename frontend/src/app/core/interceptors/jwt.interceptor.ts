// src/app/core/interceptors/jwt.interceptor.ts
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

export const jwtInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  const token  = auth.getAccessToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // Auto-refresh on 401
      if (err.status === 401 && !req.url.includes('/auth/')) {
        return auth.refreshToken().pipe(
          switchMap(res => {
            if (res.success) {
              const retryReq = req.clone({
                setHeaders: { Authorization: `Bearer ${auth.getAccessToken()}` }
              });
              return next(retryReq);
            }
            auth.logout();
            return throwError(() => err);
          }),
          catchError(() => {
            auth.logout();
            return throwError(() => err);
          })
        );
      }
      return throwError(() => err);
    })
  );
};
