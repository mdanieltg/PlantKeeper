import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Auth } from './auth';
import { SignedInKeeper } from './models';

const KEEPER: SignedInKeeper = {
  id: '00000000-0000-0000-0000-000000000001',
  userName: 'keeper',
  displayName: 'Keeper',
  email: null,
  roles: ['keeper'],
  permissions: ['plants.read', 'plants.write', 'almanac.read'],
};

describe('Auth', () => {
  let auth: Auth;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    auth = TestBed.inject(Auth);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('starts unknown, which is not the same as signed out', () => {
    expect(auth.status()).toBe('unknown');
    expect(auth.isSignedIn()).toBe(false);
  });

  it('accepts the keeper the API reports', async () => {
    const refreshed = auth.refresh();
    http.expectOne('/api/authentication/me').flush(KEEPER);

    expect(await refreshed).toEqual(KEEPER);
    expect(auth.isSignedIn()).toBe(true);
    expect(auth.displayName()).toBe('Keeper');
  });

  it('treats a 401 from /me as "nobody", not as an error', async () => {
    const refreshed = auth.refresh();
    http.expectOne('/api/authentication/me').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(await refreshed).toBeNull();
    expect(auth.status()).toBe('signed-out');
  });

  it('answers permission questions from the claims it was given', async () => {
    const refreshed = auth.refresh();
    http.expectOne('/api/authentication/me').flush(KEEPER);
    await refreshed;

    expect(auth.has('almanac.read')).toBe(true);
    expect(auth.has('almanac.approve')).toBe(false);
  });

  it('asks once, then reuses the answer', async () => {
    const first = auth.ensureResolved();
    http.expectOne('/api/authentication/me').flush(KEEPER);
    await first;

    expect(await auth.ensureResolved()).toBe(true);
    http.expectNone('/api/authentication/me');
  });

  it('clears the session even when signing out fails', async () => {
    const refreshed = auth.refresh();
    http.expectOne('/api/authentication/me').flush(KEEPER);
    await refreshed;

    const out = auth.signOut();
    http.expectOne('/api/authentication/sign-out').flush(null, { status: 500, statusText: 'Error' });
    await out;

    expect(auth.isSignedIn()).toBe(false);
  });
});
