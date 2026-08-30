import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

type QueryParams = Record<string, string | undefined>;
type PathSource = string | (() => string | undefined);

function resolve(path: PathSource): string | undefined {
  return typeof path === 'function' ? path() : path;
}

/**
 * Single entry point to the API.
 *
 * Reads and writes are split because Angular splits them: `httpResource` is GET-only by
 * design and the docs are explicit that mutations belong on `HttpClient`. Resources
 * fetch eagerly, expose signals, and refetch on their own when a signal read inside the
 * request function changes - which is what drives the per-plant log tabs.
 *
 * The resource factories must be called from an injection context (a field initializer
 * on a component or service).
 */
@Injectable({ providedIn: 'root' })
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiHost}/api`;

  /**
   * Collection resource. The path may itself be reactive, and returning `undefined`
   * anywhere leaves the resource idle rather than firing a request.
   */
  listResource<T>(path: PathSource, params?: () => QueryParams | undefined) {
    return httpResource<T[]>(
      () => {
        const resolved = resolve(path);
        if (resolved === undefined) return undefined;

        const query = params?.();
        if (params !== undefined && query === undefined) return undefined;

        return { url: `${this.base}/${resolved}`, params: pruned(query) };
      },
      { defaultValue: [] },
    );
  }

  /** Single-item resource. Stays idle until both path and id resolve. */
  itemResource<T>(path: PathSource, id: () => string | undefined) {
    return httpResource<T>(() => {
      const resolved = resolve(path);
      const value = id();
      return resolved && value ? `${this.base}/${resolved}/${value}` : undefined;
    });
  }

  create<TResult>(path: string, body: unknown): Promise<TResult> {
    return firstValueFrom(this.http.post<TResult>(`${this.base}/${path}`, body));
  }

  update(path: string, id: string, body: unknown): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${this.base}/${path}/${id}`, body));
  }

  /** Upsert for the singleton species profiles, which have no id of their own. */
  put<TResult>(path: string, body: unknown): Promise<TResult> {
    return firstValueFrom(this.http.put<TResult>(`${this.base}/${path}`, body));
  }

  remove(path: string, id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.base}/${path}/${id}`));
  }
}

/** HttpParams rejects undefined, and the API treats an absent filter as "all". */
function pruned(params: QueryParams | undefined): Record<string, string> {
  const result: Record<string, string> = {};
  for (const [key, value] of Object.entries(params ?? {})) {
    if (value !== undefined && value !== '') result[key] = value;
  }
  return result;
}
