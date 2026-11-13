import { HttpErrorResponse, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '../models/api-error.model';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

const LOGIN_ENDPOINT = '/auth/login';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const notifier = inject(NotificationService);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      const apiError = toApiError(error);
      const isLoginRequest = request.url.endsWith(LOGIN_ENDPOINT);

      if (error.status === 0) {
        notifier.error('Impossible de joindre le serveur. Vérifiez que l’API est démarrée.');
      } else if (error.status === HttpStatusCode.Unauthorized) {
        if (!isLoginRequest) {
          notifier.warning('Votre session a expiré, veuillez vous reconnecter.');
          auth.logout();
        }
      } else if (error.status === HttpStatusCode.Forbidden) {
        notifier.error('Accès refusé : vous n’avez pas les droits nécessaires pour cette action.');
      } else if (error.status >= HttpStatusCode.InternalServerError) {
        notifier.error(apiError.detail ?? 'Une erreur interne est survenue.');
      } else if (!apiError.errors) {
        notifier.error(apiError.detail ?? apiError.title);
      }

      return throwError(() => apiError);
    })
  );
};

function toApiError(error: HttpErrorResponse): ApiError {
  const body: unknown = error.error;

  if (body && typeof body === 'object') {
    const problem = body as Partial<ApiError>;
    return {
      status: error.status,
      title: problem.title ?? (error.statusText || 'Erreur'),
      detail: problem.detail,
      errors: problem.errors
    };
  }

  return {
    status: error.status,
    title: error.statusText || 'Erreur',
    detail: typeof body === 'string' && body.length > 0 ? body : undefined
  };
}
