import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // withComponentInputBinding lets route params arrive as signal inputs on the
    // component, which keeps the detail screens free of ActivatedRoute plumbing.
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withFetch()),
  ],
};
