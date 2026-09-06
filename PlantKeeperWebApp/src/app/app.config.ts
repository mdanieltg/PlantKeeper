import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // withComponentInputBinding lets route params arrive as signal inputs on the
    // component, which keeps the detail screens free of ActivatedRoute plumbing.
    provideRouter(routes, withComponentInputBinding()),
    // The interceptor attaches the session cookie to every request - httpResource
    // reads included, since it goes through the same client - and turns a 401 into a
    // redirect to the login page rather than a broken screen.
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
  ],
};
