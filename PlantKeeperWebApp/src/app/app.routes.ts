import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'plants' },

  {
    path: 'plants',
    loadComponent: () => import('./features/plants/plant-list').then((m) => m.PlantList),
  },
  {
    path: 'plants/new',
    loadComponent: () => import('./features/plants/plant-form').then((m) => m.PlantForm),
  },
  {
    path: 'plants/:plantId/edit',
    loadComponent: () => import('./features/plants/plant-form').then((m) => m.PlantForm),
  },
  {
    path: 'plants/:plantId',
    loadComponent: () => import('./features/plants/plant-detail').then((m) => m.PlantDetail),
  },

  {
    path: 'species',
    loadComponent: () => import('./features/species/species-list').then((m) => m.SpeciesList),
  },
  {
    path: 'species/new',
    loadComponent: () => import('./features/species/species-form').then((m) => m.SpeciesForm),
  },
  {
    path: 'species/:speciesId/edit',
    loadComponent: () => import('./features/species/species-form').then((m) => m.SpeciesForm),
  },
  {
    path: 'species/:speciesId',
    loadComponent: () => import('./features/species/species-detail').then((m) => m.SpeciesDetail),
  },

  {
    path: 'lookups',
    loadComponent: () => import('./features/lookups/lookup-page').then((m) => m.LookupPage),
  },
  {
    path: 'lookups/:resource',
    loadComponent: () => import('./features/lookups/lookup-page').then((m) => m.LookupPage),
  },

  {
    path: '**',
    loadComponent: () => import('./features/not-found').then((m) => m.NotFound),
  },
];
