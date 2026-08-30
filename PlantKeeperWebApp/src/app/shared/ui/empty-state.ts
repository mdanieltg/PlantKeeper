import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Every table in this app starts empty - nothing has been seeded - so the empty state
 * is a primary screen, not an edge case.
 */
@Component({
  selector: 'app-empty-state',
  template: `
    <div class="flex flex-col items-center justify-center px-6 py-14 text-center">
      <div
        class="mb-4 flex size-12 items-center justify-center rounded-full bg-leaf-100 text-2xl"
        aria-hidden="true"
      >
        {{ icon() }}
      </div>
      <p class="text-base font-medium text-bark-800">{{ heading() }}</p>
      @if (message()) {
        <p class="mt-1 max-w-md text-sm text-bark-500">{{ message() }}</p>
      }
      <div class="mt-5 flex items-center gap-2">
        <ng-content />
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyState {
  readonly heading = input.required<string>();
  readonly message = input<string>();
  readonly icon = input('🌱');
}
