import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormField, applyWhen, form, max, maxLength, min, required, submit } from '@angular/forms/signals';
import { ApiClient } from '../../core/api-client';
import { messagesOf } from '../../core/field-errors';
import { ApiFailure, errorsFor, toApiFailure } from '../../core/problem-details';
import {
  ClimateDto,
  InputPlantSpecies,
  PlantSpeciesDto,
  PottingMixDto,
} from '../../core/models';
import {
  FLOWERING_HABITS,
  FLOWERING_HABIT_LABELS,
  FloweringHabit,
  LIGHT_LEVELS,
  LIGHT_LEVEL_LABELS,
  LightLevel,
  TOXICITIES,
  TOXICITY_LABELS,
  Toxicity,
  WIND_TOLERANCES,
  WIND_TOLERANCE_LABELS,
  WindTolerance,
  optionsOf,
} from '../../core/enums';
import { environment } from '../../../environments/environment';
import { Field } from '../../shared/ui/field';
import { LoadState } from '../../shared/ui/load-state';
import { PageHeader } from '../../shared/ui/page-header';

interface SpeciesFormModel {
  scientificName: string;
  name: string;
  nameInEnglish: string;
  climateId: string;
  pottingMixId: string;
  floweringHabit: FloweringHabit;
  fertilizationFrequency: string;
  fertilizationNotes: string;
  comments: string;
  care: {
    lightMin: LightLevel;
    lightMax: LightLevel;
    lightNotes: string;
    minTemperatureCelsius: number;
    maxTemperatureCelsius: number;
    wateringRequirement: string;
    soilPhMin: number;
    soilPhMax: number;
    soilPhNotes: string;
    windTolerance: WindTolerance;
    windToleranceNotes: string;
  };
  toxicity: {
    toHumans: Toxicity;
    toHumansNotes: string;
    toPets: Toxicity;
    toPetsNotes: string;
  };
  /** Drives whether a flowering profile is sent at all. */
  hasFlowering: boolean;
  flowering: {
    bloomSeason: string;
    bloomCareNotes: string;
    seedViability: string;
    seedHarvestTiming: string;
  };
}

function blank(): SpeciesFormModel {
  return {
    scientificName: '',
    name: '',
    nameInEnglish: '',
    climateId: '',
    pottingMixId: '',
    floweringHabit: 'FlowersInCultivation',
    fertilizationFrequency: '',
    fertilizationNotes: '',
    comments: '',
    care: {
      lightMin: 'Shade',
      lightMax: 'BrightIndirect',
      lightNotes: '',
      minTemperatureCelsius: 10,
      maxTemperatureCelsius: 30,
      wateringRequirement: '',
      soilPhMin: 6,
      soilPhMax: 7,
      soilPhNotes: '',
      windTolerance: 'Moderate',
      windToleranceNotes: '',
    },
    toxicity: { toHumans: 'NonToxic', toHumansNotes: '', toPets: 'NonToxic', toPetsNotes: '' },
    hasFlowering: false,
    flowering: { bloomSeason: '', bloomCareNotes: '', seedViability: '', seedHarvestTiming: '' },
  };
}

const nullIfBlank = (value: string): string | null => (value.trim() === '' ? null : value);

@Component({
  selector: 'app-species-form',
  imports: [RouterLink, FormField, Field, LoadState, PageHeader],
  templateUrl: './species-form.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpeciesForm {
  private readonly api = inject(ApiClient);
  private readonly router = inject(Router);

  readonly speciesId = input<string>();

  protected readonly apiHost = environment.apiHost;
  protected readonly editing = computed(() => !!this.speciesId());

  protected readonly lightOptions = optionsOf(LIGHT_LEVELS, LIGHT_LEVEL_LABELS);
  protected readonly windOptions = optionsOf(WIND_TOLERANCES, WIND_TOLERANCE_LABELS);
  protected readonly toxicityOptions = optionsOf(TOXICITIES, TOXICITY_LABELS);
  protected readonly habitOptions = optionsOf(FLOWERING_HABITS, FLOWERING_HABIT_LABELS);

  protected readonly climates = this.api.listResource<ClimateDto>('climates');
  protected readonly mixes = this.api.listResource<PottingMixDto>('potting-mixes');
  protected readonly existing = this.api.itemResource<PlantSpeciesDto>(
    'plant-species',
    () => this.speciesId(),
  );

  protected readonly saving = signal(false);
  protected readonly failure = signal<ApiFailure | null>(null);
  protected readonly model = signal<SpeciesFormModel>(blank());

  protected readonly missingPrerequisites = computed(
    () =>
      !this.climates.isLoading() &&
      !this.mixes.isLoading() &&
      (this.climates.value().length === 0 || this.mixes.value().length === 0),
  );

  /** A non-flowering species must not carry a profile - the API rejects it with a 422. */
  protected readonly floweringAllowed = computed(
    () => this.model().floweringHabit !== 'DoesNotFlower',
  );

  protected readonly species = form(this.model, (path) => {
    required(path.name, { message: 'A common name is required.' });
    maxLength(path.name, 50, { message: 'Name must be 50 characters or fewer.' });
    required(path.scientificName, { message: 'A scientific name is required.' });
    maxLength(path.scientificName, 100, { message: 'Scientific name must be 100 characters or fewer.' });
    maxLength(path.nameInEnglish, 50, { message: 'English name must be 50 characters or fewer.' });
    required(path.climateId, { message: 'Choose a climate.' });
    required(path.pottingMixId, { message: 'Choose a potting mix.' });
    required(path.fertilizationFrequency, { message: 'Describe how often this species is fed.' });
    maxLength(path.fertilizationFrequency, 50, { message: 'Must be 50 characters or fewer.' });
    maxLength(path.fertilizationNotes, 255, { message: 'Must be 255 characters or fewer.' });
    maxLength(path.comments, 255, { message: 'Must be 255 characters or fewer.' });

    // Care - required for every species; the database cannot enforce this, so the
    // form and the API aggregate do.
    required(path.care.wateringRequirement, { message: 'Describe the watering requirement.' });
    maxLength(path.care.wateringRequirement, 150, { message: 'Must be 150 characters or fewer.' });
    maxLength(path.care.lightNotes, 100, { message: 'Must be 100 characters or fewer.' });
    maxLength(path.care.soilPhNotes, 100, { message: 'Must be 100 characters or fewer.' });
    maxLength(path.care.windToleranceNotes, 100, { message: 'Must be 100 characters or fewer.' });
    min(path.care.minTemperatureCelsius, -90, { message: 'Too cold to be real.' });
    max(path.care.minTemperatureCelsius, 60, { message: 'Too hot to be real.' });
    min(path.care.maxTemperatureCelsius, -90, { message: 'Too cold to be real.' });
    max(path.care.maxTemperatureCelsius, 60, { message: 'Too hot to be real.' });
    min(path.care.soilPhMin, 0, { message: 'pH runs from 0 to 14.' });
    max(path.care.soilPhMin, 14, { message: 'pH runs from 0 to 14.' });
    min(path.care.soilPhMax, 0, { message: 'pH runs from 0 to 14.' });
    max(path.care.soilPhMax, 14, { message: 'pH runs from 0 to 14.' });

    // Toxicity - also required. An unresearched species must never read as harmless.
    maxLength(path.toxicity.toHumansNotes, 150, { message: 'Must be 150 characters or fewer.' });
    maxLength(path.toxicity.toPetsNotes, 150, { message: 'Must be 150 characters or fewer.' });

    // Flowering is optional as a whole, but complete when present - so its rules only
    // apply once the profile is switched on.
    applyWhen(
      path.flowering,
      ({ valueOf }) => valueOf(path.hasFlowering) && valueOf(path.floweringHabit) !== 'DoesNotFlower',
      (flowering) => {
        required(flowering.bloomSeason, { message: 'Bloom season is required.' });
        maxLength(flowering.bloomSeason, 150, { message: 'Must be 150 characters or fewer.' });
        required(flowering.bloomCareNotes, { message: 'Bloom care notes are required.' });
        maxLength(flowering.bloomCareNotes, 255, { message: 'Must be 255 characters or fewer.' });
        required(flowering.seedViability, { message: 'Seed viability is required.' });
        maxLength(flowering.seedViability, 255, { message: 'Must be 255 characters or fewer.' });
        required(flowering.seedHarvestTiming, { message: 'Seed harvest timing is required.' });
        maxLength(flowering.seedHarvestTiming, 150, { message: 'Must be 150 characters or fewer.' });
      },
    );
  });

  constructor() {
    effect(() => {
      const loaded = this.existing.hasValue() ? this.existing.value() : undefined;
      if (loaded) {
        this.model.set(fromDto(loaded));
        return;
      }

      if (!this.editing()) {
        const climate = this.climates.value()[0];
        const mix = this.mixes.value()[0];
        this.model.update((current) => ({
          ...current,
          climateId: current.climateId || (climate?.id ?? ''),
          pottingMixId: current.pottingMixId || (mix?.id ?? ''),
        }));
      }
    });

    // Marking a species non-flowering clears the profile rather than leaving a payload
    // the API would reject.
    effect(() => {
      if (this.model().floweringHabit === 'DoesNotFlower' && this.model().hasFlowering) {
        this.model.update((current) => ({ ...current, hasFlowering: false }));
      }
    });
  }

  protected errorsOf(
    control: () => { errors(): readonly { kind: string; message?: string }[]; touched(): boolean },
    serverKey?: string,
  ): readonly string[] {
    const local = control().touched() ? messagesOf(control().errors()) : [];
    return serverKey ? [...local, ...errorsFor(this.failure(), serverKey)] : local;
  }

  protected save(): void {
    submit(this.species, async () => {
      this.saving.set(true);
      this.failure.set(null);

      const value = this.model();
      const sendFlowering = value.hasFlowering && value.floweringHabit !== 'DoesNotFlower';

      const body: InputPlantSpecies = {
        scientificName: value.scientificName,
        name: value.name,
        nameInEnglish: nullIfBlank(value.nameInEnglish),
        climateId: value.climateId,
        pottingMixId: value.pottingMixId,
        floweringHabit: value.floweringHabit,
        fertilizationFrequency: value.fertilizationFrequency,
        fertilizationNotes: nullIfBlank(value.fertilizationNotes),
        comments: nullIfBlank(value.comments),
        care: {
          ...value.care,
          lightNotes: nullIfBlank(value.care.lightNotes),
          soilPhNotes: nullIfBlank(value.care.soilPhNotes),
          windToleranceNotes: nullIfBlank(value.care.windToleranceNotes),
        },
        toxicity: {
          ...value.toxicity,
          toHumansNotes: nullIfBlank(value.toxicity.toHumansNotes),
          toPetsNotes: nullIfBlank(value.toxicity.toPetsNotes),
        },
        flowering: sendFlowering ? { ...value.flowering } : null,
      };

      try {
        const id = this.speciesId();
        if (id) {
          await this.api.update('plant-species', id, body);
          await this.router.navigate(['/species', id]);
        } else {
          const created = await this.api.create<PlantSpeciesDto>('plant-species', body);
          await this.router.navigate(['/species', created.id]);
        }
      } catch (error) {
        this.failure.set(toApiFailure(error));
      } finally {
        this.saving.set(false);
      }
    });
  }
}

function fromDto(dto: PlantSpeciesDto): SpeciesFormModel {
  const empty = blank();
  return {
    scientificName: dto.scientificName,
    name: dto.name,
    nameInEnglish: dto.nameInEnglish ?? '',
    climateId: dto.climateId,
    pottingMixId: dto.pottingMixId,
    floweringHabit: dto.floweringHabit,
    fertilizationFrequency: dto.fertilizationFrequency,
    fertilizationNotes: dto.fertilizationNotes ?? '',
    comments: dto.comments ?? '',
    care: {
      lightMin: dto.care.lightMin,
      lightMax: dto.care.lightMax,
      lightNotes: dto.care.lightNotes ?? '',
      minTemperatureCelsius: dto.care.minTemperatureCelsius,
      maxTemperatureCelsius: dto.care.maxTemperatureCelsius,
      wateringRequirement: dto.care.wateringRequirement,
      soilPhMin: dto.care.soilPhMin,
      soilPhMax: dto.care.soilPhMax,
      soilPhNotes: dto.care.soilPhNotes ?? '',
      windTolerance: dto.care.windTolerance,
      windToleranceNotes: dto.care.windToleranceNotes ?? '',
    },
    toxicity: {
      toHumans: dto.toxicity.toHumans,
      toHumansNotes: dto.toxicity.toHumansNotes ?? '',
      toPets: dto.toxicity.toPets,
      toPetsNotes: dto.toxicity.toPetsNotes ?? '',
    },
    hasFlowering: dto.flowering !== null,
    flowering: dto.flowering
      ? {
          bloomSeason: dto.flowering.bloomSeason,
          bloomCareNotes: dto.flowering.bloomCareNotes,
          seedViability: dto.flowering.seedViability,
          seedHarvestTiming: dto.flowering.seedHarvestTiming,
        }
      : empty.flowering,
  };
}
