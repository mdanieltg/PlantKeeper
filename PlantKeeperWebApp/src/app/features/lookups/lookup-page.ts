import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormField, form, maxLength, required, submit } from '@angular/forms/signals';
import { ApiClient } from '../../core/api-client';
import { messagesOf } from '../../core/field-errors';
import { ApiFailure, errorsFor, toApiFailure } from '../../core/problem-details';
import { FERTILIZER_CATEGORY_LABELS } from '../../core/enums';
import { environment } from '../../../environments/environment';
import { ConfirmDelete } from '../../shared/ui/confirm-delete';
import { EmptyState } from '../../shared/ui/empty-state';
import { Field } from '../../shared/ui/field';
import { LoadState } from '../../shared/ui/load-state';
import { PageHeader } from '../../shared/ui/page-header';
import {
  LOOKUPS,
  LookupFieldSpec,
  blankRecord,
  lookupBySlug,
  toPayload,
  toRecord,
} from './lookup-spec';

type Row = Record<string, unknown> & { id: string };

@Component({
  selector: 'app-lookup-page',
  imports: [RouterLink, FormField, ConfirmDelete, EmptyState, Field, LoadState, PageHeader],
  templateUrl: './lookup-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LookupPage {
  private readonly api = inject(ApiClient);

  /** Bound from the :resource route parameter. */
  readonly resource = input<string>();

  protected readonly all = LOOKUPS;
  protected readonly apiHost = environment.apiHost;
  protected readonly spec = computed(() => lookupBySlug(this.resource()));

  protected readonly rows = this.api.listResource<Row>(() => this.spec()?.path);

  protected readonly editingId = signal<string | null>(null);
  protected readonly panelOpen = signal(false);
  protected readonly saving = signal(false);
  protected readonly failure = signal<ApiFailure | null>(null);
  protected readonly pendingDelete = signal<Row | null>(null);

  /**
   * The model is a flat string record so one page can serve five tables. Signal Forms
   * is typed against that record, so the schema indexes the path by key - the one place
   * this page trades static field names for configuration.
   */
  protected readonly model = signal<Record<string, string>>({});

  protected readonly entry = form(this.model, (path) => {
    for (const spec of LOOKUPS) {
      for (const field of spec.fields) {
        const target = (path as unknown as Record<string, never>)[field.key];
        if (!target) continue;
        if (field.required) {
          required(target, { message: `${field.label} is required.` });
        }
        if (field.maxLength) {
          maxLength(target, field.maxLength, {
            message: `${field.label} must be ${field.maxLength} characters or fewer.`,
          });
        }
      }
    }
  });

  protected fieldOf(key: string) {
    return (this.entry as unknown as Record<string, never>)[key];
  }

  protected errorsOf(field: LookupFieldSpec): readonly string[] {
    const control = this.fieldOf(field.key) as unknown as
      | (() => { errors(): readonly { kind: string; message?: string }[]; touched(): boolean })
      | undefined;

    const local = control && control().touched() ? messagesOf(control().errors()) : [];
    return [...local, ...errorsFor(this.failure(), field.key)];
  }

  protected display(row: Row, key: string): string {
    const value = row[key];
    if (value === null || value === undefined || value === '') return '—';
    if (key === 'category') {
      return (
        FERTILIZER_CATEGORY_LABELS[value as keyof typeof FERTILIZER_CATEGORY_LABELS] ??
        String(value)
      );
    }
    return String(value);
  }

  protected startCreate(): void {
    const spec = this.spec();
    if (!spec) return;
    this.failure.set(null);
    this.editingId.set(null);
    this.model.set(blankRecord(spec));
    this.panelOpen.set(true);
  }

  protected startEdit(row: Row): void {
    const spec = this.spec();
    if (!spec) return;
    this.failure.set(null);
    this.editingId.set(row.id);
    this.model.set(toRecord(spec, row));
    this.panelOpen.set(true);
  }

  protected closePanel(): void {
    this.panelOpen.set(false);
    this.failure.set(null);
  }

  protected save(): void {
    const spec = this.spec();
    if (!spec) return;

    submit(this.entry, async () => {
      this.saving.set(true);
      this.failure.set(null);
      const body = toPayload(spec, this.model());
      const id = this.editingId();

      try {
        if (id) {
          await this.api.update(spec.path, id, body);
        } else {
          await this.api.create(spec.path, body);
        }
        this.panelOpen.set(false);
        this.rows.reload();
      } catch (error) {
        this.failure.set(toApiFailure(error));
      } finally {
        this.saving.set(false);
      }
    });
  }

  protected async confirmDelete(): Promise<void> {
    const spec = this.spec();
    const row = this.pendingDelete();
    if (!spec || !row) return;

    try {
      await this.api.remove(spec.path, row.id);
      this.rows.reload();
    } catch (error) {
      this.failure.set(toApiFailure(error));
    } finally {
      this.pendingDelete.set(null);
    }
  }
}
