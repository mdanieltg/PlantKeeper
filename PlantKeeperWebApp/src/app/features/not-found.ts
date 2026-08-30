import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  template: `
    <div class="card py-16 text-center">
      <p class="text-5xl" aria-hidden="true">🍂</p>
      <h1 class="mt-4 text-xl font-semibold text-bark-900">Page not found</h1>
      <p class="mt-1 text-sm text-bark-500">That route does not exist.</p>
      <a routerLink="/plants" class="btn btn-primary mt-6 inline-flex">Back to plants</a>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFound {}
