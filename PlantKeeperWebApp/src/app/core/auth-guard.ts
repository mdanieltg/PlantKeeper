import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from './auth';
import { PermissionName } from './permissions';

/**
 * Every route except the login page.
 *
 * Resolves the session once - the cookie is HttpOnly, so the only way to know is to ask -
 * and sends a visitor to the login page carrying where they were headed.
 */
export const signedIn: CanActivateFn = async (_route, state) => {
  const auth = inject(Auth);
  const router = inject(Router);

  if (await auth.ensureResolved()) return true;

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/**
 * A route that also needs one permission. Redirects to the plant list rather than to
 * login: the keeper is signed in, they simply cannot go here, and bouncing them to a
 * login form they have already passed would be a lie about what went wrong.
 */
export function requiresPermission(permission: PermissionName): CanActivateFn {
  return async (_route, state) => {
    const auth = inject(Auth);
    const router = inject(Router);

    // The same resolution `signedIn` does, inlined rather than delegated: calling one
    // CanActivateFn from another leaks its wider return type (it may hand back an
    // Observable) into this one, which no longer type-checks as a guard.
    if (!(await auth.ensureResolved()))
      return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });

    return auth.has(permission) ? true : router.createUrlTree(['/plants']);
  };
}
