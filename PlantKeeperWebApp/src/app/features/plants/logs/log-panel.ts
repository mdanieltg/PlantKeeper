import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  FormField,
  applyWhen,
  form,
  max,
  maxLength,
  min,
  required,
  submit,
} from '@angular/forms/signals';
import { ApiClient } from '../../../core/api-client';
import { messagesOf } from '../../../core/field-errors';
import { ApiFailure, errorsFor, toApiFailure } from '../../../core/problem-details';
import { environment } from '../../../../environments/environment';
import { ConfirmDelete } from '../../../shared/ui/confirm-delete';
import { EmptyState } from '../../../shared/ui/empty-state';
import { Field } from '../../../shared/ui/field';
import { LoadState } from '../../../shared/ui/load-state';
import { LOG_SPECS, LogFieldSpec, LogSpec, blankLog, toLogPayload } from './log-spec';

type LogRow = Record<string, unknown> & { id: string };
interface NamedRow {
  id: string;
  name: string;
}

@Component({
  selector: 'app-log-panel',
  imports: [RouterLink, FormField, ConfirmDelete, EmptyState, Field, LoadState],
  templateUrl: './log-panel.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogPanel {
  private readonly api = inject(ApiClient);

  readonly spec = input.required<LogSpec>();
  readonly plantId = input.required<string>();

  protected readonly apiHost = environment.apiHost;

  /** Refetches whenever the tab or the plant changes - both are read inside. */
  protected readonly rows = this.api.listResource<LogRow>(
    () => this.spec().path,
    () => ({ plantId: this.plantId() }),
  );

  /** The lookup this log references, if any (method, fertilizer, treatment). */
  protected readonly refField = computed(() => this.spec().fields.find((f) => f.kind === 'ref'));
  protected readonly refRows = this.api.listResource<NamedRow>(() => this.refField()?.refPath);
  protected readonly refById = computed(
    () => new Map(this.refRows.value().map((row) => [row.id, row.name])),
  );
  protected readonly refMissing = computed(
    () => !!this.refField() && !this.refRows.isLoading() && this.refRows.value().length === 0,
  );

  protected readonly adding = signal(false);
  protected readonly saving = signal(false);
  protected readonly failure = signal<ApiFailure | null>(null);
  protected readonly pendingDelete = signal<LogRow | null>(null);

  protected readonly model = signal<Record<string, string>>({});

  protected readonly entry = form(this.model, (path) => {
    // One schema serves every log type, so rules are indexed by key. Two specs can share a
    // key with conflicting rules - observation's `notes` is required, growth's optional -
    // and a flat registration unions them onto the shared `notes` path, so growth inherited
    // observation's `required` and showed a stale "Notes is required." Gate each spec's
    // rules on its being the active tab, so only the active spec's version of a key applies.
    for (const spec of LOG_SPECS) {
      for (const field of spec.fields) {
        const target = (path as unknown as Record<string, never>)[field.key];
        if (!target) continue;

        applyWhen(
          target,
          () => this.spec().slug === spec.slug,
          (active) => {
            // Same `never` cast the flat version used: one schema serves every log shape, so
            // the field path is deliberately untyped and the validators accept any target.
            const t = active as unknown as never;
            if (field.required) required(t, { message: `${field.label} is required.` });
            if (field.maxLength) {
              maxLength(t, field.maxLength, {
                message: `${field.label} must be ${field.maxLength} characters or fewer.`,
              });
            }
            if (field.min !== undefined)
              min(t, field.min, { message: `${field.label} cannot be negative.` });
            if (field.max !== undefined)
              max(t, field.max, { message: `${field.label} is too large.` });
          },
        );
      }
    }
  });

  constructor() {
    // Switching tabs resets the draft so a half-filled form never leaks across types.
    effect(() => {
      this.spec();
      this.adding.set(false);
      this.failure.set(null);
    });
  }

  protected fieldOf(key: string) {
    return (this.entry as unknown as Record<string, never>)[key];
  }

  protected errorsOf(field: LogFieldSpec): readonly string[] {
    const control = this.fieldOf(field.key) as unknown as
      | (() => { errors(): readonly { kind: string; message?: string }[]; touched(): boolean })
      | undefined;

    const local = control && control().touched() ? messagesOf(control().errors()) : [];
    return [...local, ...errorsFor(this.failure(), field.key)];
  }

  protected display(row: LogRow, key: string): string {
    const value = row[key];
    if (value === null || value === undefined || value === '') return '—';
    if (key === 'date') return new Date(String(value)).toLocaleString();
    if (this.refField()?.key === key) return this.refById().get(String(value)) ?? '—';
    return String(value);
  }

  protected startAdd(): void {
    this.failure.set(null);
    this.model.set(this.withRefDefault(blankLog(this.spec())));
    this.adding.set(true);
  }

  private withRefDefault(draft: Record<string, string>): Record<string, string> {
    const ref = this.refField();
    const first = this.refRows.value()[0];
    if (ref && first) draft[ref.key] = first.id;
    return draft;
  }

  protected save(): void {
    submit(this.entry, async () => {
      this.saving.set(true);
      this.failure.set(null);

      try {
        await this.api.create(
          this.spec().path,
          toLogPayload(this.spec(), this.plantId(), this.model()),
        );
        this.adding.set(false);
        this.rows.reload();
      } catch (error) {
        this.failure.set(toApiFailure(error));
      } finally {
        this.saving.set(false);
      }
    });
  }

  protected async confirmDelete(): Promise<void> {
    const row = this.pendingDelete();
    if (!row) return;

    try {
      await this.api.remove(this.spec().path, row.id);
      this.rows.reload();
    } catch (error) {
      this.failure.set(toApiFailure(error));
    } finally {
      this.pendingDelete.set(null);
    }
  }
}
