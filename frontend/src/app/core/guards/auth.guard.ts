// src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn()) return true;
  return router.createUrlTree(['/auth/login']);
};

export const guestGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  if (!auth.isLoggedIn()) return true;
  return router.createUrlTree(['/dashboard']);
};

// Role-based: usage → roleGuard(['Admin', 'Manager'])
export const roleGuard = (roles: string[]): CanActivateFn => () => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  const user   = auth.currentUser();
  if (user && roles.includes(user.role)) return true;
  return router.createUrlTree(['/dashboard']);
};
