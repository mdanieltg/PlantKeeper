import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiClient } from '../../core/api-client';
import { PlantSpeciesDto } from '../../core/models';
import { LIGHT_LEVEL_LABELS, TOXICITY_LABELS, TOXICITY_TONE } from '../../core/enums';
import { environment } from '../../../environments/environment';
import { Badge } from '../../shared/ui/badge';
import { EmptyState } from '../../shared/ui/empty-state';
import { LoadState } from '../../shared/ui/load-state';
import { PageHeader } from '../../shared/ui/page-header';

@Component({
  selector: 'app-species-list',
  imports: [RouterLink, Badge, EmptyState, LoadState, PageHeader],
  templateUrl: './species-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpeciesList {
  private readonly api = inject(ApiClient);

  protected readonly apiHost = environment.apiHost;
  protected readonly lightLabels = LIGHT_LEVEL_LABELS;
  protected readonly toxicityLabels = TOXICITY_LABELS;
  protected readonly toxicityTone = TOXICITY_TONE;

  protected readonly species = this.api.listResource<PlantSpeciesDto>('plant-species');
}
