import { HttpErrorResponse } from '@angular/common/http';

/** Field name (camelCase, as the API returns it) to the messages against it. */
export type FieldErrors = Readonly<Record<string, readonly string[]>>;

export interface ApiFailure {
  /** Message suitable for a banner when no field owns the problem. */
  readonly message: string;
  readonly fields: FieldErrors;
  readonly status: number;
}

interface ProblemDetailsBody {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

/**
 * Turns a failed write into something a form can render.
 *
 * The API answers 400 and 422 with `ValidationProblemDetails`, whose `errors` keys are
 * camelCase field names - `speciesId`, `climateId`, `flowering` - and 409 with a plain
 * `ProblemDetails`. Mapping the keys back onto controls is what lets a rejected foreign
 * key mark the offending select instead of raising an anonymous banner.
 */
export function toApiFailure(error: unknown): ApiFailure {
  if (!(error instanceof HttpErrorResponse)) {
    return { message: 'Something went wrong.', fields: {}, status: 0 };
  }

  if (error.status === 0) {
    return {
      message: 'Cannot reach the API. Is it running on ' + location.origin + '?',
      fields: {},
      status: 0,
    };
  }

  const body = (error.error ?? {}) as ProblemDetailsBody;
  const fields: Record<string, readonly string[]> = {};

  for (const [key, messages] of Object.entries(body.errors ?? {})) {
    // System.Text.Json reports its own parse failures as "$.propertyName".
    fields[key.replace(/^\$\./, '')] = messages;
  }

  const message =
    body.detail ??
    (Object.keys(fields).length > 0 ? 'Please correct the highlighted fields.' : null) ??
    body.title ??
    `Request failed with status ${error.status}.`;

  return { message, fields, status: error.status };
}

/** Messages for one field, including the `$.field` form the JSON parser emits. */
export function errorsFor(failure: ApiFailure | null, field: string): readonly string[] {
  return failure?.fields[field] ?? [];
}
