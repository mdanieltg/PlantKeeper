import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { Auth } from './auth';

/** Requests that answer 401 as a matter of course, rather than because the session lapsed. */
const AUTH_ENDPOINTS = ['/api/authentication/sign-in', '/api/authentication/me'];

/**
 * Sends the session cookie, and treats a 401 as "the session is gone".
 *
 * `withCredentials` is required even though development is now same-origin: the flag
 * governs whether the browser attaches cookies at all for `fetch`, and Angular's client
 * leaves it off by default.
 *
 * The redirect carries the URL that was refused, so signing back in returns the keeper to
 * what they were looking at rather than to the plant list.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(Auth);
  const router = inject(Router);

  return next(request.clone({ withCredentials: true })).pipe(
    catchError((error: unknown) => {
      const is401 = error instanceof HttpErrorResponse && error.status === 401;
      const isAuthCall = AUTH_ENDPOINTS.some((path) => request.url.includes(path));

      if (is401 && !isAuthCall) {
        auth.clear();

        // The current URL, not the failed request's - the keeper wants the page back,
        // not the endpoint.
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      }

      return throwError(() => error);
    }),
  );
};
