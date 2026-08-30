import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-badge',
  template: `
    <span
      class="inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ring-1 ring-inset"
      [class]="tone()"
    >
      <ng-content />
    </span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Badge {
  readonly tone = input('bg-bark-100 text-bark-700 ring-bark-500/20');
}
