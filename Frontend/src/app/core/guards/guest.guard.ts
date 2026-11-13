import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { DEFAULT_REDIRECT_URL } from '../constants/app.constants';
import { AuthService } from '../services/auth.service';

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.isSessionValid() ? router.parseUrl(DEFAULT_REDIRECT_URL) : true;
};
