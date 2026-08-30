import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApiClient } from '../../core/api-client';
import { toApiFailure, ApiFailure } from '../../core/problem-details';
import { PlantDto, PlantSpeciesDto } from '../../core/models';
import { LIGHT_LEVEL_LABELS, TOXICITY_LABELS, TOXICITY_TONE } from '../../core/enums';
import { environment } from '../../../environments/environment';
import { Badge } from '../../shared/ui/badge';
import { ConfirmDelete } from '../../shared/ui/confirm-delete';
import { LoadState } from '../../shared/ui/load-state';
import { PageHeader } from '../../shared/ui/page-header';
import { LogPanel } from './logs/log-panel';
import { LOG_SPECS } from './logs/log-spec';

@Component({
  selector: 'app-plant-detail',
  imports: [RouterLink, Badge, ConfirmDelete, LoadState, PageHeader, LogPanel],
  templateUrl: './plant-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlantDetail {
  private readonly api = inject(ApiClient);
  private readonly router = inject(Router);

  readonly plantId = input.required<string>();

  protected readonly apiHost = environment.apiHost;
  protected readonly lightLabels = LIGHT_LEVEL_LABELS;
  protected readonly toxicityLabels = TOXICITY_LABELS;
  protected readonly toxicityTone = TOXICITY_TONE;
  protected readonly logSpecs = LOG_SPECS;

  protected readonly plant = this.api.itemResource<PlantDto>('plants', () => this.plantId());

  /** Follows the plant's speciesId once it arrives. */
  protected readonly species = this.api.itemResource<PlantSpeciesDto>('plant-species', () =>
    this.plant.hasValue() ? this.plant.value().speciesId : undefined,
  );

  protected readonly activeTab = signal(LOG_SPECS[0].slug);
  protected readonly activeSpec = computed(
    () => LOG_SPECS.find((spec) => spec.slug === this.activeTab()) ?? LOG_SPECS[0],
  );

  protected readonly pendingDelete = signal(false);
  protected readonly failure = signal<ApiFailure | null>(null);

  protected async confirmDelete(): Promise<void> {
    try {
      await this.api.remove('plants', this.plantId());
      await this.router.navigate(['/plants']);
    } catch (error) {
      this.failure.set(toApiFailure(error));
    } finally {
      this.pendingDelete.set(false);
    }
  }
}
