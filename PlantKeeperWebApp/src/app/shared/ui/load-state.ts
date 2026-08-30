import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Spinner while a resource loads, a readable message when it fails. */
@Component({
  selector: 'app-load-state',
  template: `
    @if (loading()) {
      <div class="flex items-center justify-center gap-3 px-6 py-14 text-sm text-bark-500">
        <span
          class="size-4 animate-spin rounded-full border-2 border-bark-300 border-t-leaf-600"
          aria-hidden="true"
        ></span>
        Loading…
      </div>
    } @else if (failed()) {
      <div class="m-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
        <p class="font-medium">Could not load this data.</p>
        <p class="mt-1 text-red-700">
          Check that the API is running at <code class="font-mono">{{ apiHost() }}</code
          >.
        </p>
      </div>
    } @else {
      <ng-content />
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadState {
  readonly loading = input(false);
  readonly failed = input(false);
  readonly apiHost = input('');
}
