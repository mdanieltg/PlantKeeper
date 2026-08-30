import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiClient } from '../../core/api-client';
import { PlantDto, PlantSpeciesDto } from '../../core/models';
import { environment } from '../../../environments/environment';
import { EmptyState } from '../../shared/ui/empty-state';
import { LoadState } from '../../shared/ui/load-state';
import { PageHeader } from '../../shared/ui/page-header';

@Component({
  selector: 'app-plant-list',
  imports: [RouterLink, EmptyState, LoadState, PageHeader],
  templateUrl: './plant-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlantList {
  private readonly api = inject(ApiClient);

  protected readonly apiHost = environment.apiHost;
  protected readonly plants = this.api.listResource<PlantDto>('plants');
  protected readonly species = this.api.listResource<PlantSpeciesDto>('plant-species');

  /** The API returns speciesId only; names come from the species collection. */
  protected readonly speciesById = computed(
    () => new Map(this.species.value().map((entry) => [entry.id, entry])),
  );

  protected readonly loading = computed(() => this.plants.isLoading() || this.species.isLoading());
  protected readonly noSpecies = computed(
    () => !this.species.isLoading() && this.species.value().length === 0,
  );
}
