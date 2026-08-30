import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormField, form, maxLength, required, submit } from '@angular/forms/signals';
import { ApiClient } from '../../core/api-client';
import { messagesOf } from '../../core/field-errors';
import { ApiFailure, errorsFor, toApiFailure } from '../../core/problem-details';
import { InputPlant, PlantDto, PlantSpeciesDto } from '../../core/models';
import { environment } from '../../../environments/environment';
import { Field } from '../../shared/ui/field';
import { LoadState } from '../../shared/ui/load-state';
import { PageHeader } from '../../shared/ui/page-header';

interface PlantFormModel {
  alias: string;
  speciesId: string;
  comments: string;
}

@Component({
  selector: 'app-plant-form',
  imports: [RouterLink, FormField, Field, LoadState, PageHeader],
  templateUrl: './plant-form.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlantForm {
  private readonly api = inject(ApiClient);
  private readonly router = inject(Router);

  readonly plantId = input<string>();

  protected readonly apiHost = environment.apiHost;
  protected readonly editing = computed(() => !!this.plantId());

  protected readonly species = this.api.listResource<PlantSpeciesDto>('plant-species');
  protected readonly existing = this.api.itemResource<PlantDto>('plants', () => this.plantId());

  protected readonly saving = signal(false);
  protected readonly failure = signal<ApiFailure | null>(null);

  // Never null or undefined: Signal Forms requires concrete initial values.
  protected readonly model = signal<PlantFormModel>({ alias: '', speciesId: '', comments: '' });

  protected readonly plant = form(this.model, (path) => {
    required(path.alias, { message: 'Give the plant a name you will recognise.' });
    maxLength(path.alias, 50, { message: 'Alias must be 50 characters or fewer.' });
    required(path.speciesId, { message: 'Choose the species this plant belongs to.' });
    maxLength(path.comments, 255, { message: 'Comments must be 255 characters or fewer.' });
  });

  constructor() {
    // Fill the form once the record arrives, and default the select when creating.
    effect(() => {
      const loaded = this.existing.hasValue() ? this.existing.value() : undefined;
      if (loaded) {
        this.model.set({
          alias: loaded.alias,
          speciesId: loaded.speciesId,
          comments: loaded.comments ?? '',
        });
        return;
      }

      if (!this.editing() && this.model().speciesId === '') {
        const first = this.species.value()[0];
        if (first) this.model.update((current) => ({ ...current, speciesId: first.id }));
      }
    });
  }

  protected errorsOf(
    control: () => { errors(): readonly { kind: string; message?: string }[]; touched(): boolean },
    serverKey: string,
  ): readonly string[] {
    const local = control().touched() ? messagesOf(control().errors()) : [];
    return [...local, ...errorsFor(this.failure(), serverKey)];
  }

  protected save(): void {
    submit(this.plant, async () => {
      this.saving.set(true);
      this.failure.set(null);

      const value = this.model();
      const body: InputPlant = {
        alias: value.alias,
        speciesId: value.speciesId,
        comments: value.comments === '' ? null : value.comments,
      };

      try {
        const id = this.plantId();
        if (id) {
          await this.api.update('plants', id, body);
          await this.router.navigate(['/plants', id]);
        } else {
          const created = await this.api.create<PlantDto>('plants', body);
          await this.router.navigate(['/plants', created.id]);
        }
      } catch (error) {
        this.failure.set(toApiFailure(error));
      } finally {
        this.saving.set(false);
      }
    });
  }
}
