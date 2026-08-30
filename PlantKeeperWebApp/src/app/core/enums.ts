/**
 * The API's closed scales. Values are serialized as PascalCase strings and the
 * converter is configured with allowIntegerValues: false, so anything not in these
 * lists comes back as a 400. Keep them in step with PlantKeeperAPI/src/Enums/.
 */

export const LIGHT_LEVELS = [
  'LowLight',
  'Shade',
  'PartialShade',
  'BrightIndirect',
  'FullSun',
] as const;
export type LightLevel = (typeof LIGHT_LEVELS)[number];

export const WIND_TOLERANCES = ['Low', 'Moderate', 'High'] as const;
export type WindTolerance = (typeof WIND_TOLERANCES)[number];

export const TOXICITIES = ['NonToxic', 'Irritant', 'MildlyToxic', 'Toxic'] as const;
export type Toxicity = (typeof TOXICITIES)[number];

export const FLOWERING_HABITS = [
  'FlowersInCultivation',
  'RarelyFlowersInCultivation',
  'DoesNotFlower',
] as const;
export type FloweringHabit = (typeof FLOWERING_HABITS)[number];

export const FERTILIZER_CATEGORIES = ['NitrogenOnly', 'Balanced', 'Bloom', 'Organic'] as const;
export type FertilizerCategory = (typeof FERTILIZER_CATEGORIES)[number];

/** Display text. The domain data is Spanish; the interface chrome is English. */
export const LIGHT_LEVEL_LABELS: Record<LightLevel, string> = {
  LowLight: 'Low light',
  Shade: 'Shade',
  PartialShade: 'Partial shade',
  BrightIndirect: 'Bright indirect',
  FullSun: 'Full sun',
};

export const WIND_TOLERANCE_LABELS: Record<WindTolerance, string> = {
  Low: 'Low',
  Moderate: 'Moderate',
  High: 'High',
};

export const TOXICITY_LABELS: Record<Toxicity, string> = {
  NonToxic: 'Non-toxic',
  Irritant: 'Irritant',
  MildlyToxic: 'Mildly toxic',
  Toxic: 'Toxic',
};

export const FLOWERING_HABIT_LABELS: Record<FloweringHabit, string> = {
  FlowersInCultivation: 'Flowers in cultivation',
  RarelyFlowersInCultivation: 'Rarely flowers in cultivation',
  DoesNotFlower: 'Does not flower',
};

export const FERTILIZER_CATEGORY_LABELS: Record<FertilizerCategory, string> = {
  NitrogenOnly: 'Nitrogen only',
  Balanced: 'Balanced NPK',
  Bloom: 'Bloom',
  Organic: 'Organic',
};

/** Tailwind classes per toxicity level, so the badge colour carries the severity. */
export const TOXICITY_TONE: Record<Toxicity, string> = {
  NonToxic: 'bg-leaf-100 text-leaf-800 ring-leaf-600/20',
  Irritant: 'bg-amber-100 text-amber-800 ring-amber-600/20',
  MildlyToxic: 'bg-orange-100 text-orange-800 ring-orange-600/20',
  Toxic: 'bg-red-100 text-red-800 ring-red-600/20',
};

export function optionsOf<T extends string>(
  values: readonly T[],
  labels: Record<T, string>,
): readonly { value: T; label: string }[] {
  return values.map((value) => ({ value, label: labels[value] }));
}
