/**
 * Signal Forms exposes errors as `{ kind, message? }`. Validators in this app always
 * pass an explicit `message`, so `kind` is only a fallback for anything that slips
 * through without one.
 */
interface ValidationLike {
  readonly kind: string;
  readonly message?: string;
}

const FALLBACKS: Record<string, string> = {
  required: 'This field is required.',
  min: 'Value is too small.',
  max: 'Value is too large.',
  minLength: 'Too short.',
  maxLength: 'Too long.',
  pattern: 'Invalid format.',
  email: 'Enter a valid email address.',
};

export function messagesOf(errors: readonly ValidationLike[]): string[] {
  return errors.map((error) => error.message ?? FALLBACKS[error.kind] ?? 'Invalid value.');
}
