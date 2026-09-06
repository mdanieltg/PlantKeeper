import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { SignedInKeeper } from './models';
import { PermissionName } from './permissions';

/**
 * Who is signed in, as a signal.
 *
 * There is no token to hold: the session is an HttpOnly cookie the browser sends on its
 * own and JavaScript cannot read. So "am I signed in?" is not a local question - it is
 * answered by asking the API, once at startup and again after each sign-in.
 *
 * `status` distinguishes *not yet asked* from *asked and nobody* on purpose. Treating
 * the first as signed-out is what makes an app flash the login page on every refresh.
 */
@Injectable({ providedIn: 'root' })
export class Auth {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiHost}/api/authentication`;

  private readonly state = signal<'unknown' | 'signed-in' | 'signed-out'>('unknown');
  private readonly current = signal<SignedInKeeper | null>(null);

  readonly keeper = this.current.asReadonly();
  readonly status = this.state.asReadonly();
  readonly isSignedIn = computed(() => this.state() === 'signed-in');

  readonly displayName = computed(() => this.current()?.displayName ?? '');

  /** True only for a permission the signed-in keeper actually holds. */
  has(permission: PermissionName): boolean {
    return this.current()?.permissions.includes(permission) ?? false;
  }

  /**
   * Asks the API who the cookie belongs to. A 401 is the expected answer for a visitor,
   * not an error, so it settles the state rather than propagating.
   */
  async refresh(): Promise<SignedInKeeper | null> {
    try {
      const keeper = await firstValueFrom(this.http.get<SignedInKeeper>(`${this.base}/me`));
      this.accept(keeper);
      return keeper;
    } catch {
      this.clear();
      return null;
    }
  }

  /** Resolves the session once, then reuses it. Used by the route guard. */
  async ensureResolved(): Promise<boolean> {
    if (this.state() === 'unknown') await this.refresh();
    return this.state() === 'signed-in';
  }

  async signIn(userName: string, password: string, rememberMe: boolean): Promise<SignedInKeeper> {
    const keeper = await firstValueFrom(
      this.http.post<SignedInKeeper>(`${this.base}/sign-in`, { userName, password, rememberMe }),
    );

    this.accept(keeper);
    return keeper;
  }

  /**
   * Ends the session. Never rejects.
   *
   * A sign-out that could not reach the API still means this browser should stop behaving
   * as though it has a session, and there is nothing useful for a caller to do about the
   * failure - so the local state is cleared and the error is dropped rather than handed
   * on to a click handler that would only swallow it anyway.
   */
  async signOut(): Promise<void> {
    try {
      await firstValueFrom(this.http.post<void>(`${this.base}/sign-out`, {}));
    } catch {
      // Deliberately ignored - see above.
    } finally {
      this.clear();
    }
  }

  /** Called by the interceptor when the API says the session is gone. */
  clear(): void {
    this.current.set(null);
    this.state.set('signed-out');
  }

  private accept(keeper: SignedInKeeper): void {
    this.current.set(keeper);
    this.state.set('signed-in');
  }
}
