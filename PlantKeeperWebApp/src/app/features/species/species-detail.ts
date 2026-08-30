import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApiClient } from '../../core/api-client';
import { ApiFailure, toApiFailure } from '../../core/problem-details';
import { ClimateDto, PlantDto, PlantSpeciesDto, PottingMixDto } from '../../core/models';
import {
  FLOWERING_HABIT_LABELS,
  LIGHT_LEVEL_LABELS,
  TOXICITY_LABELS,
  TOXICITY_TONE,
  WIND_TOLERANCE_LABELS,
} from '../../core/enums';
import { environment } from '../../../environments/environment';
import { Badge } from '../../shared/ui/badge';
import { ConfirmDelete } from '../../shared/ui/confirm-delete';
import { LoadState } from '../../shared/ui/load-state';
import { PageHeader } from '../../shared/ui/page-header';

@Component({
  selector: 'app-species-detail',
  imports: [RouterLink, Badge, ConfirmDelete, LoadState, PageHeader],
  templateUrl: './species-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpeciesDetail {
  private readonly api = inject(ApiClient);
  private readonly router = inject(Router);

  readonly speciesId = input.required<string>();

  protected readonly apiHost = environment.apiHost;
  protected readonly lightLabels = LIGHT_LEVEL_LABELS;
  protected readonly windLabels = WIND_TOLERANCE_LABELS;
  protected readonly toxicityLabels = TOXICITY_LABELS;
  protected readonly toxicityTone = TOXICITY_TONE;
  protected readonly habitLabels = FLOWERING_HABIT_LABELS;

  protected readonly species = this.api.itemResource<PlantSpeciesDto>('plant-species', () =>
    this.speciesId(),
  );
  protected readonly climates = this.api.listResource<ClimateDto>('climates');
  protected readonly mixes = this.api.listResource<PottingMixDto>('potting-mixes');
  protected readonly plants = this.api.listResource<PlantDto>('plants', () => ({
    speciesId: this.speciesId(),
  }));

  protected readonly climateName = computed(() => {
    // Hoisted: narrowing from hasValue() does not survive into the callback below.
    const entry = this.species.hasValue() ? this.species.value() : undefined;
    if (!entry) return '—';
    return this.climates.value().find((climate) => climate.id === entry.climateId)?.name ?? '—';
  });

  protected readonly mixName = computed(() => {
    const entry = this.species.hasValue() ? this.species.value() : undefined;
    if (!entry) return '—';
    return this.mixes.value().find((mix) => mix.id === entry.pottingMixId)?.name ?? '—';
  });

  protected readonly pendingDelete = signal(false);
  protected readonly failure = signal<ApiFailure | null>(null);

  protected async confirmDelete(): Promise<void> {
    try {
      await this.api.remove('plant-species', this.speciesId());
      await this.router.navigate(['/species']);
    } catch (error) {
      this.failure.set(toApiFailure(error));
    } finally {
      this.pendingDelete.set(false);
    }
  }
}
