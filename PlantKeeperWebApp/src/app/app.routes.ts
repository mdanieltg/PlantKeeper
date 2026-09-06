import { Routes } from '@angular/router';
import { requiresPermission, signedIn } from './core/auth-guard';
import { Permission } from './core/permissions';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'plants' },

  // The only route without the guard. withComponentInputBinding turns ?returnUrl= into
  // the component's input, so the guard's redirect needs no extra plumbing.
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login').then((m) => m.Login),
  },

  {
    path: 'almanac-review',
    canActivate: [requiresPermission(Permission.almanacApprove)],
    loadComponent: () =>
      import('./features/almanac/proposal-queue').then((m) => m.ProposalQueue),
  },

  {
    path: 'plants',
    canActivate: [signedIn],
    loadComponent: () => import('./features/plants/plant-list').then((m) => m.PlantList),
  },
  {
    path: 'plants/new',
    canActivate: [signedIn],
    loadComponent: () => import('./features/plants/plant-form').then((m) => m.PlantForm),
  },
  {
    path: 'plants/:plantId/edit',
    canActivate: [signedIn],
    loadComponent: () => import('./features/plants/plant-form').then((m) => m.PlantForm),
  },
  {
    path: 'plants/:plantId',
    canActivate: [signedIn],
    loadComponent: () => import('./features/plants/plant-detail').then((m) => m.PlantDetail),
  },

  {
    path: 'species',
    canActivate: [signedIn],
    loadComponent: () => import('./features/species/species-list').then((m) => m.SpeciesList),
  },
  {
    path: 'species/new',
    canActivate: [signedIn],
    loadComponent: () => import('./features/species/species-form').then((m) => m.SpeciesForm),
  },
  {
    path: 'species/:speciesId/edit',
    canActivate: [signedIn],
    loadComponent: () => import('./features/species/species-form').then((m) => m.SpeciesForm),
  },
  {
    path: 'species/:speciesId',
    canActivate: [signedIn],
    loadComponent: () => import('./features/species/species-detail').then((m) => m.SpeciesDetail),
  },

  {
    path: 'lookups',
    canActivate: [signedIn],
    loadComponent: () => import('./features/lookups/lookup-page').then((m) => m.LookupPage),
  },
  {
    path: 'lookups/:resource',
    canActivate: [signedIn],
    loadComponent: () => import('./features/lookups/lookup-page').then((m) => m.LookupPage),
  },

  {
    path: '**',
    loadComponent: () => import('./features/not-found').then((m) => m.NotFound),
  },
];
