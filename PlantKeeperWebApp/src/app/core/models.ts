import { FertilizerCategory, FloweringHabit, LightLevel, Toxicity, WindTolerance } from './enums';

/**
 * Mirrors of the API's read shapes (`*Dto`) and write shapes (`Input*`). Property
 * names must match the C# ones exactly - the backend maps by convention and a
 * mismatch here becomes a silently empty field there.
 */

// --- Reference data -------------------------------------------------------

export interface ClimateDto {
  id: string;
  name: string;
  temperature: string;
  precipitation: string;
  humidity: string;
  sun: string;
  wind: string;
  description: string | null;
}
export type InputClimate = Omit<ClimateDto, 'id'>;

export interface PottingMixDto {
  id: string;
  name: string;
  description: string | null;
}
export type InputPottingMix = Omit<PottingMixDto, 'id'>;

export interface WateringMethodDto {
  id: string;
  name: string;
  description: string | null;
}
export type InputWateringMethod = Omit<WateringMethodDto, 'id'>;

export interface TreatmentDto {
  id: string;
  name: string;
  description: string | null;
}
export type InputTreatment = Omit<TreatmentDto, 'id'>;

export interface FertilizerDto {
  id: string;
  name: string;
  description: string | null;
  npkRatio: string | null;
  category: FertilizerCategory;
}
export type InputFertilizer = Omit<FertilizerDto, 'id'>;

// --- Species and its profiles ---------------------------------------------

export interface SpeciesCareProfileDto {
  speciesId: string;
  lightMin: LightLevel;
  lightMax: LightLevel;
  lightNotes: string | null;
  minTemperatureCelsius: number;
  maxTemperatureCelsius: number;
  wateringRequirement: string;
  soilPhMin: number;
  soilPhMax: number;
  soilPhNotes: string | null;
  windTolerance: WindTolerance;
  windToleranceNotes: string | null;
}
export type InputSpeciesCareProfile = Omit<SpeciesCareProfileDto, 'speciesId'>;

export interface SpeciesToxicityProfileDto {
  speciesId: string;
  toHumans: Toxicity;
  toHumansNotes: string | null;
  toPets: Toxicity;
  toPetsNotes: string | null;
}
export type InputSpeciesToxicityProfile = Omit<SpeciesToxicityProfileDto, 'speciesId'>;

export interface SpeciesFloweringProfileDto {
  speciesId: string;
  bloomSeason: string;
  bloomCareNotes: string;
  seedViability: string;
  seedHarvestTiming: string;
}
export type InputSpeciesFloweringProfile = Omit<SpeciesFloweringProfileDto, 'speciesId'>;

export interface PlantSpeciesDto {
  id: string;
  scientificName: string;
  name: string;
  nameInEnglish: string | null;
  climateId: string;
  pottingMixId: string;
  floweringHabit: FloweringHabit;
  fertilizationFrequency: string;
  fertilizationNotes: string | null;
  comments: string | null;
  /** Always present - required by the schema. */
  care: SpeciesCareProfileDto;
  /** Always present - required by the schema. */
  toxicity: SpeciesToxicityProfileDto;
  /** Null when the species does not flower. */
  flowering: SpeciesFloweringProfileDto | null;
}

export interface InputPlantSpecies {
  scientificName: string;
  name: string;
  nameInEnglish: string | null;
  climateId: string;
  pottingMixId: string;
  floweringHabit: FloweringHabit;
  fertilizationFrequency: string;
  fertilizationNotes: string | null;
  comments: string | null;
  care: InputSpeciesCareProfile;
  toxicity: InputSpeciesToxicityProfile;
  flowering: InputSpeciesFloweringProfile | null;
}

// --- Plants ---------------------------------------------------------------

export interface PlantDto {
  id: string;
  alias: string;
  speciesId: string;
  comments: string | null;
}
export type InputPlant = Omit<PlantDto, 'id'>;

// --- Logs -----------------------------------------------------------------

export interface WateringLogDto {
  id: string;
  plantId: string;
  wateringMethodId: string;
  date: string;
  comments: string | null;
}
export type InputWateringLog = Omit<WateringLogDto, 'id'>;

export interface FertilizationLogDto {
  id: string;
  plantId: string;
  fertilizerId: string;
  date: string;
  dose: string | null;
  comments: string | null;
}
export type InputFertilizationLog = Omit<FertilizationLogDto, 'id'>;

export interface TreatmentLogDto {
  id: string;
  plantId: string;
  treatmentId: string;
  date: string;
  comments: string | null;
}
export type InputTreatmentLog = Omit<TreatmentLogDto, 'id'>;

export interface RepottingLogDto {
  id: string;
  plantId: string;
  date: string;
  dimensions: string;
  volume: string;
  material: string;
  comments: string | null;
}
export type InputRepottingLog = Omit<RepottingLogDto, 'id'>;

export interface ObservationLogDto {
  id: string;
  plantId: string;
  date: string;
  notes: string;
}
export type InputObservationLog = Omit<ObservationLogDto, 'id'>;

export interface GrowthLogDto {
  id: string;
  plantId: string;
  date: string;
  heightCm: number | null;
  heightToLastNodeCm: number | null;
  notes: string | null;
}
export type InputGrowthLog = Omit<GrowthLogDto, 'id'>;

// --- Session -------------------------------------------------------------

/** `SignedInKeeperDto`. Roles are for display; authorize against `permissions`. */
export interface SignedInKeeper {
  id: string;
  userName: string;
  displayName: string;
  email: string | null;
  roles: string[];
  permissions: string[];
}

export interface InputSignIn {
  userName: string;
  password: string;
  rememberMe: boolean;
}

// --- Almanac change control ----------------------------------------------

export type AlmanacChangeOperation = 'Create' | 'Update' | 'Delete';
export type AlmanacProposalStatus = 'Pending' | 'Applied' | 'Rejected';

/**
 * `AlmanacChangeProposalDto`. Every almanac write produces one, so this table is the
 * almanac's history as well as its review queue.
 *
 * `proposedState` is the request body the proposer sent, in the shape of the matching
 * `Input*` model - so its type depends on `targetType` and is left unknown here.
 */
export interface AlmanacChangeProposalDto {
  id: string;
  targetType: string;
  targetId: string | null;
  operation: AlmanacChangeOperation;
  proposedState: unknown;
  targetVersion: number | null;
  status: AlmanacProposalStatus;
  autoApproved: boolean;
  proposedById: string;
  proposedAt: string;
  reviewedById: string | null;
  reviewedAt: string | null;
  reviewNote: string | null;
}

export interface InputAlmanacReview {
  note: string | null;
}
