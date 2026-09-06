import { FERTILIZER_CATEGORIES, FERTILIZER_CATEGORY_LABELS, optionsOf } from '../../core/enums';

export interface LookupFieldSpec {
  readonly key: string;
  readonly label: string;
  readonly kind: 'text' | 'textarea' | 'select';
  readonly required?: boolean;
  /** Mirrors the [StringLength] on the matching Input* model in the API. */
  readonly maxLength?: number;
  readonly hint?: string;
  readonly options?: readonly { value: string; label: string }[];
}

export interface LookupSpec {
  /** Route segment. */
  readonly slug: string;
  /** API path segment. */
  readonly path: string;
  readonly title: string;
  readonly singular: string;
  readonly blurb: string;
  readonly icon: string;
  /**
   * What a delete does. These are all shared-almanac rows, and since the API's
   * RestrictAlmanacDeletes migration a delete no longer cascades through other keepers'
   * collections - it is refused with 409 while anything still references the row. The
   * dialog says so up front; the 409's own message names what is in the way.
   */
  readonly consequence: string;
  readonly columns: readonly { key: string; label: string }[];
  readonly fields: readonly LookupFieldSpec[];
}

const DESCRIPTION: LookupFieldSpec = {
  key: 'description',
  label: 'Description',
  kind: 'textarea',
  maxLength: 255,
};

/**
 * Reference tables. These are in the MVP because nothing else can be created without
 * them: a species needs a climate and a potting mix, a watering log needs a method.
 * Lengths come from the [StringLength] attributes in PlantKeeperAPI/src/Models/.
 */
export const LOOKUPS: readonly LookupSpec[] = [
  {
    slug: 'climates',
    path: 'climates',
    title: 'Climates',
    singular: 'climate',
    blurb: 'Ambient conditions a species is suited to.',
    icon: '🌤️',
    consequence:
      'Refused while any species still uses this climate — reassign them first.',
    columns: [
      { key: 'name', label: 'Name' },
      { key: 'temperature', label: 'Temperature' },
      { key: 'precipitation', label: 'Precipitation' },
      { key: 'humidity', label: 'Humidity' },
      { key: 'sun', label: 'Sun' },
      { key: 'wind', label: 'Wind' },
    ],
    fields: [
      { key: 'name', label: 'Name', kind: 'text', required: true, maxLength: 50 },
      {
        key: 'temperature',
        label: 'Temperature',
        kind: 'text',
        required: true,
        maxLength: 50,
        hint: 'e.g. 10–26 °C',
      },
      { key: 'precipitation', label: 'Precipitation', kind: 'text', required: true, maxLength: 50 },
      { key: 'humidity', label: 'Humidity', kind: 'text', required: true, maxLength: 50 },
      { key: 'sun', label: 'Sun', kind: 'text', required: true, maxLength: 50 },
      { key: 'wind', label: 'Wind', kind: 'text', required: true, maxLength: 50 },
      DESCRIPTION,
    ],
  },
  {
    slug: 'potting-mixes',
    path: 'potting-mixes',
    title: 'Potting mixes',
    singular: 'potting mix',
    blurb: 'Substrate recipes used across the collection.',
    icon: '🪨',
    consequence:
      'Refused while any species still uses this mix — reassign them first.',
    columns: [
      { key: 'name', label: 'Name' },
      { key: 'description', label: 'Description' },
    ],
    fields: [
      { key: 'name', label: 'Name', kind: 'text', required: true, maxLength: 30 },
      DESCRIPTION,
    ],
  },
  {
    slug: 'watering-methods',
    path: 'watering-methods',
    title: 'Watering methods',
    singular: 'watering method',
    blurb: 'How water is delivered — referenced by every watering log.',
    icon: '💧',
    consequence: 'Refused while any watering log still records this method.',
    columns: [
      { key: 'name', label: 'Name' },
      { key: 'description', label: 'Description' },
    ],
    fields: [
      { key: 'name', label: 'Name', kind: 'text', required: true, maxLength: 30 },
      DESCRIPTION,
    ],
  },
  {
    slug: 'fertilizers',
    path: 'fertilizers',
    title: 'Fertilizers',
    singular: 'fertilizer',
    blurb: 'Products on the shelf, mapped onto the almanac fertilization matrix.',
    icon: '🧪',
    consequence: 'Refused while any fertilization log still uses this fertilizer.',
    columns: [
      { key: 'name', label: 'Name' },
      { key: 'npkRatio', label: 'NPK' },
      { key: 'category', label: 'Category' },
    ],
    fields: [
      { key: 'name', label: 'Name', kind: 'text', required: true, maxLength: 30 },
      {
        key: 'category',
        label: 'Category',
        kind: 'select',
        required: true,
        options: optionsOf(FERTILIZER_CATEGORIES, FERTILIZER_CATEGORY_LABELS),
      },
      { key: 'npkRatio', label: 'NPK ratio', kind: 'text', maxLength: 15, hint: 'e.g. 17-17-17' },
      DESCRIPTION,
    ],
  },
  {
    slug: 'treatments',
    path: 'treatments',
    title: 'Treatments',
    singular: 'treatment',
    blurb: 'Pest and disease treatments — referenced by every treatment log.',
    icon: '🛡️',
    consequence:
      'Refused while any treatment log or species recommendation still references it.',
    columns: [
      { key: 'name', label: 'Name' },
      { key: 'description', label: 'Description' },
    ],
    fields: [
      { key: 'name', label: 'Name', kind: 'text', required: true, maxLength: 30 },
      DESCRIPTION,
    ],
  },
];

export function lookupBySlug(slug: string | undefined): LookupSpec | undefined {
  return LOOKUPS.find((entry) => entry.slug === slug);
}

/** Signal Forms requires '' rather than null for absent values. */
export function blankRecord(spec: LookupSpec): Record<string, string> {
  return Object.fromEntries(spec.fields.map((field) => [field.key, defaultFor(field)]));
}

function defaultFor(field: LookupFieldSpec): string {
  return field.kind === 'select' && field.required ? (field.options?.[0]?.value ?? '') : '';
}

/** Widens an API row into the form's string record, turning nulls into ''. */
export function toRecord(spec: LookupSpec, row: Record<string, unknown>): Record<string, string> {
  return Object.fromEntries(
    spec.fields.map((field) => [field.key, (row[field.key] as string | null) ?? '']),
  );
}

/** Narrows the form record back to a request body, turning '' into null. */
export function toPayload(
  spec: LookupSpec,
  record: Record<string, string>,
): Record<string, unknown> {
  return Object.fromEntries(
    spec.fields.map((field) => {
      const value = record[field.key] ?? '';
      return [field.key, field.required ? value : value === '' ? null : value];
    }),
  );
}
