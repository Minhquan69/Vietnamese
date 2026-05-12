import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthSessionService } from './auth-session.service';

export const roleGuard: CanActivateFn = (route) => {
  const roles = route.data['roles'] as string[] | undefined;
  if (!roles?.length) {
    return true;
  }
  const session = inject(AuthSessionService);
  const router = inject(Router);
  const role = session.getRoleFromAccessToken();
  if (!role || !roles.includes(role)) {
    return router.createUrlTree(['/dashboard']);
  }
  return true;
};
