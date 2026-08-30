import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  template: `
    <header class="mb-6 flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 class="text-2xl font-semibold tracking-tight text-bark-900">{{ heading() }}</h1>
        @if (subheading()) {
          <p class="mt-1 text-sm text-bark-500">{{ subheading() }}</p>
        }
      </div>
      <div class="flex items-center gap-2">
        <ng-content />
      </div>
    </header>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeader {
  readonly heading = input.required<string>();
  readonly subheading = input<string>();
}
