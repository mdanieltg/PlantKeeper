import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Auth } from './auth';
import { authInterceptor } from './auth-interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let navigations: { commands: unknown[]; extras?: unknown }[];

  beforeEach(() => {
    TestBed.resetTestingModule();
    navigations = [];

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        {
          provide: Router,
          useValue: {
            url: '/species/abc',
            navigate: (commands: unknown[], extras?: unknown) => {
              navigations.push({ commands, extras });
              return Promise.resolve(true);
            },
          },
        },
      ],
    });

    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('sends the session cookie on every request', () => {
    http.get('/api/plants').subscribe({ next: () => undefined, error: () => undefined });

    const request = backend.expectOne('/api/plants');
    expect(request.request.withCredentials).toBe(true);
    request.flush([]);
  });

  it('sends a lapsed session to the login page, carrying where it was', async () => {
    const response = firstValueFrom(http.get('/api/plants'));
    backend.expectOne('/api/plants').flush(null, { status: 401, statusText: 'Unauthorized' });

    await expect(response).rejects.toBeDefined();

    expect(navigations).toHaveLength(1);
    expect(navigations[0].commands).toEqual(['/login']);
    expect(navigations[0].extras).toEqual({ queryParams: { returnUrl: '/species/abc' } });
    expect(TestBed.inject(Auth).isSignedIn()).toBe(false);
  });

  /**
   * `/me` answers 401 for every visitor who has not signed in yet. Redirecting on that
   * would bounce the login page to itself on first load.
   */
  it('does not redirect when it is the session check itself that 401s', async () => {
    const response = firstValueFrom(http.get('/api/authentication/me'));
    backend
      .expectOne('/api/authentication/me')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    await expect(response).rejects.toBeDefined();
    expect(navigations).toEqual([]);
  });

  it('does not redirect on a wrong password', async () => {
    const response = firstValueFrom(http.post('/api/authentication/sign-in', {}));
    backend
      .expectOne('/api/authentication/sign-in')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    await expect(response).rejects.toBeDefined();
    expect(navigations).toEqual([]);
  });

  it('leaves other failures alone', async () => {
    const response = firstValueFrom(http.delete('/api/climates/abc'));
    backend.expectOne('/api/climates/abc').flush(null, { status: 409, statusText: 'Conflict' });

    await expect(response).rejects.toBeDefined();
    expect(navigations).toEqual([]);
  });
});
