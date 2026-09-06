import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-delete',
  template: `
    <div
      class="fixed inset-0 z-50 flex items-center justify-center bg-bark-900/40 p-4"
      role="dialog"
      aria-modal="true"
    >
      <div class="card w-full max-w-sm p-5">
        <h2 class="text-base font-semibold text-bark-900">Delete {{ subject() }}?</h2>
        <p class="mt-2 text-sm text-bark-600">
          This cannot be undone.
          @if (consequence()) {
            <span class="mt-1 block text-bark-500">{{ consequence() }}</span>
          }
        </p>
        <div class="mt-5 flex justify-end gap-2">
          <button type="button" class="btn btn-secondary" (click)="cancelled.emit()">Cancel</button>
          <button type="button" class="btn btn-danger" (click)="confirmed.emit()">Delete</button>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDelete {
  readonly subject = input.required<string>();

  /**
   * What actually happens, which since the API restricted almanac deletes is one of two
   * different things. Inside one keeper's own collection a delete still cascades and the
   * note says what goes with it. Across the shared almanac the delete is *refused* while
   * anything still references the row, and the note says that instead - the old cascade
   * warnings described data loss that can no longer occur.
   */
  readonly consequence = input<string>();
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
}
