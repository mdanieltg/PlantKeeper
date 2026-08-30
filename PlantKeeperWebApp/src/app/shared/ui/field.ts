import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Label, control and messages. Named Field rather than FormField to stay clear of
 * Signal Forms' own `FormField` directive, which is used on the control inside.
 */
@Component({
  selector: 'app-field',
  template: `
    <div [class]="wrapperClass()">
      <label class="label" [attr.for]="controlId()">
        {{ label() }}
        @if (optional()) {
          <span class="ml-1 font-normal text-bark-400">(optional)</span>
        }
      </label>

      <ng-content />

      @if (hint() && errors().length === 0) {
        <p class="mt-1.5 text-xs text-bark-500">{{ hint() }}</p>
      }

      @for (error of errors(); track error) {
        <p class="mt-1.5 text-xs font-medium text-red-600">{{ error }}</p>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Field {
  readonly label = input.required<string>();
  readonly controlId = input<string>();
  readonly hint = input<string>();
  readonly optional = input(false);
  readonly errors = input<readonly string[]>([]);
  readonly wrapperClass = input('');
}
