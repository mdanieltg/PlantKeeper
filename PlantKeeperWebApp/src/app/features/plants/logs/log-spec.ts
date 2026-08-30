export interface LogFieldSpec {
  readonly key: string;
  readonly label: string;
  readonly kind: 'text' | 'textarea' | 'number' | 'datetime' | 'ref';
  readonly required?: boolean;
  /** Mirrors the [StringLength] on the matching Input* model in the API. */
  readonly maxLength?: number;
  readonly hint?: string;
  /** For 'ref': the API path whose rows populate the select. */
  readonly refPath?: string;
  readonly refLabel?: string;
  readonly min?: number;
  readonly max?: number;
  readonly step?: string;
}

export interface LogSpec {
  readonly slug: string;
  readonly path: string;
  readonly title: string;
  readonly singular: string;
  readonly icon: string;
  readonly columns: ReadonlyArray<{ key: string; label: string }>;
  readonly fields: readonly LogFieldSpec[];
}

const DATE: LogFieldSpec = { key: 'date', label: 'Date', kind: 'datetime', required: true };
const COMMENTS: LogFieldSpec = { key: 'comments', label: 'Comments', kind: 'textarea', maxLength: 255 };

/**
 * The six log types. All are flat collections filtered by `?plantId=`, so one panel
 * serves them all; only the field list differs.
 */
export const LOG_SPECS: readonly LogSpec[] = [
  {
    slug: 'watering',
    path: 'watering-logs',
    title: 'Watering',
    singular: 'watering log',
    icon: '💧',
    columns: [
      { key: 'date', label: 'Date' },
      { key: 'wateringMethodId', label: 'Method' },
      { key: 'comments', label: 'Comments' },
    ],
    fields: [
      DATE,
      {
        key: 'wateringMethodId',
        label: 'Method',
        kind: 'ref',
        required: true,
        refPath: 'watering-methods',
        refLabel: 'watering method',
      },
      COMMENTS,
    ],
  },
  {
    slug: 'fertilization',
    path: 'fertilization-logs',
    title: 'Fertilization',
    singular: 'fertilization log',
    icon: '🧪',
    columns: [
      { key: 'date', label: 'Date' },
      { key: 'fertilizerId', label: 'Fertilizer' },
      { key: 'dose', label: 'Dose' },
      { key: 'comments', label: 'Comments' },
    ],
    fields: [
      DATE,
      {
        key: 'fertilizerId',
        label: 'Fertilizer',
        kind: 'ref',
        required: true,
        refPath: 'fertilizers',
        refLabel: 'fertilizer',
      },
      { key: 'dose', label: 'Dose', kind: 'text', maxLength: 100, hint: 'e.g. 5 ml/L' },
      COMMENTS,
    ],
  },
  {
    slug: 'treatment',
    path: 'treatment-logs',
    title: 'Treatments',
    singular: 'treatment log',
    icon: '🛡️',
    columns: [
      { key: 'date', label: 'Date' },
      { key: 'treatmentId', label: 'Treatment' },
      { key: 'comments', label: 'Comments' },
    ],
    fields: [
      DATE,
      {
        key: 'treatmentId',
        label: 'Treatment',
        kind: 'ref',
        required: true,
        refPath: 'treatments',
        refLabel: 'treatment',
      },
      COMMENTS,
    ],
  },
  {
    slug: 'repotting',
    path: 'repotting-logs',
    title: 'Repotting',
    singular: 'repotting log',
    icon: '🪴',
    columns: [
      { key: 'date', label: 'Date' },
      { key: 'dimensions', label: 'Dimensions' },
      { key: 'volume', label: 'Volume' },
      { key: 'material', label: 'Material' },
    ],
    fields: [
      DATE,
      { key: 'dimensions', label: 'Dimensions', kind: 'text', required: true, maxLength: 50, hint: 'e.g. 30 × 25 cm' },
      { key: 'volume', label: 'Volume', kind: 'text', required: true, maxLength: 30, hint: 'e.g. 12 L' },
      { key: 'material', label: 'Material', kind: 'text', required: true, maxLength: 50, hint: 'e.g. Barro' },
      COMMENTS,
    ],
  },
  {
    slug: 'observation',
    path: 'observation-logs',
    title: 'Observations',
    singular: 'observation',
    icon: '👁️',
    columns: [
      { key: 'date', label: 'Date' },
      { key: 'notes', label: 'Notes' },
    ],
    fields: [
      DATE,
      { key: 'notes', label: 'Notes', kind: 'textarea', required: true, maxLength: 300 },
    ],
  },
  {
    slug: 'growth',
    path: 'growth-logs',
    title: 'Growth',
    singular: 'growth log',
    icon: '📏',
    columns: [
      { key: 'date', label: 'Date' },
      { key: 'heightCm', label: 'Height (cm)' },
      { key: 'heightToLastNodeCm', label: 'To last node (cm)' },
      { key: 'notes', label: 'Notes' },
    ],
    fields: [
      DATE,
      { key: 'heightCm', label: 'Height (cm)', kind: 'number', min: 0, max: 99999.9, step: '0.1' },
      {
        key: 'heightToLastNodeCm',
        label: 'Height to last node (cm)',
        kind: 'number',
        min: 0,
        max: 99999.9,
        step: '0.1',
      },
      { key: 'notes', label: 'Notes', kind: 'textarea', maxLength: 300 },
    ],
  },
];

/** Signal Forms wants '' rather than null, so the model is all strings. */
export function blankLog(spec: LogSpec): Record<string, string> {
  return Object.fromEntries(
    spec.fields.map((field) => [field.key, field.kind === 'datetime' ? nowLocal() : '']),
  );
}

export function toLogPayload(
  spec: LogSpec,
  plantId: string,
  record: Record<string, string>,
): Record<string, unknown> {
  const body: Record<string, unknown> = { plantId };

  for (const field of spec.fields) {
    const raw = record[field.key] ?? '';
    if (field.kind === 'number') {
      body[field.key] = raw === '' ? null : Number(raw);
    } else if (field.required) {
      body[field.key] = raw;
    } else {
      body[field.key] = raw === '' ? null : raw;
    }
  }

  return body;
}

/** `datetime-local` wants `YYYY-MM-DDTHH:mm` in local time, not an ISO UTC string. */
export function nowLocal(): string {
  const now = new Date();
  const offset = now.getTimezoneOffset() * 60_000;
  return new Date(now.getTime() - offset).toISOString().slice(0, 16);
}
