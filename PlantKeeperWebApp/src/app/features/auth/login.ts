import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormField, form, maxLength, required, submit } from '@angular/forms/signals';
import { Auth } from '../../core/auth';
import { messagesOf } from '../../core/field-errors';
import { ApiFailure, errorsFor, toApiFailure } from '../../core/problem-details';
import { Field } from '../../shared/ui/field';

interface SignInModel {
  userName: string;
  password: string;
  rememberMe: boolean;
}

@Component({
  selector: 'app-login',
  imports: [FormField, Field],
  templateUrl: './login.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  /** Where the guard turned the keeper away from, bound from the query string. */
  readonly returnUrl = input<string>();

  protected readonly busy = signal(false);
  protected readonly failure = signal<ApiFailure | null>(null);

  protected readonly model = signal<SignInModel>({ userName: '', password: '', rememberMe: false });

  protected readonly credentials = form(this.model, (path) => {
    required(path.userName, { message: 'Enter your user name.' });
    maxLength(path.userName, 256, { message: 'User name must be 256 characters or fewer.' });
    required(path.password, { message: 'Enter your password.' });
    // No length rule. The server owns the password policy, and asserting one here would
    // tell a visitor which guesses were the wrong shape before any of them were tried.
  });

  protected errorsOf(
    control: () => { errors(): readonly { kind: string; message?: string }[]; touched(): boolean },
    serverKey: string,
  ): readonly string[] {
    const local = control().touched() ? messagesOf(control().errors()) : [];
    return [...local, ...errorsFor(this.failure(), serverKey)];
  }

  /**
   * The native `submit` event, not `ngSubmit`.
   *
   * `ngSubmit` is an output of `NgForm`, which comes from `FormsModule` - not in use here,
   * so binding it would silently never fire. The rest of the app sidesteps this with a
   * plain button, but a login form should submit on Enter, so it keeps the `<form>` and
   * listens to the DOM event directly.
   */
  protected signIn(event?: Event): void {
    event?.preventDefault();

    submit(this.credentials, async () => {
      this.busy.set(true);
      this.failure.set(null);

      const value = this.model();

      try {
        await this.auth.signIn(value.userName, value.password, value.rememberMe);
        await this.router.navigateByUrl(this.returnUrl() ?? '/plants');
      } catch (error) {
        this.failure.set(toApiFailure(error));
      } finally {
        this.busy.set(false);
      }
    });
  }
}
