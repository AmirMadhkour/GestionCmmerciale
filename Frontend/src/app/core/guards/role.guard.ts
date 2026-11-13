import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { RoleInput } from '../constants/roles';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isSessionValid()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const roles = (route.data['roles'] as RoleInput[] | undefined) ?? [];

  if (roles.length === 0 || auth.hasRole(roles)) {
    return true;
  }

  return router.parseUrl('/forbidden');
};
