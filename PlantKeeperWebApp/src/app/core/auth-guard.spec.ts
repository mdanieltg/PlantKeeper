import { TestBed } from '@angular/core/testing';
import { UrlTree, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { requiresPermission, signedIn } from './auth-guard';
import { Permission } from './permissions';
import { SignedInKeeper } from './models';

const KEEPER: SignedInKeeper = {
  id: '00000000-0000-0000-0000-000000000001',
  userName: 'keeper',
  displayName: 'Keeper',
  email: null,
  roles: ['keeper'],
  permissions: ['plants.read', 'almanac.read'],
};

/** The guards only read `url` off the state snapshot. */
const state = { url: '/species/abc' } as never;
const route = {} as never;

function run<T>(guard: () => T): T {
  return TestBed.runInInjectionContext(guard);
}

describe('auth guards', () => {
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('lets a signed-in keeper through', async () => {
    const result = run(() => signedIn(route, state));
    backend.expectOne('/api/authentication/me').flush(KEEPER);

    expect(await result).toBe(true);
  });

  it('sends a visitor to the login page, remembering where they were going', async () => {
    const result = run(() => signedIn(route, state));
    backend
      .expectOne('/api/authentication/me')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    const tree = await result;
    expect(tree).toBeInstanceOf(UrlTree);
    expect((tree as UrlTree).toString()).toBe('/login?returnUrl=%2Fspecies%2Fabc');
  });

  it('turns away a signed-in keeper who lacks the permission', async () => {
    const guard = requiresPermission(Permission.almanacApprove);
    const result = run(() => guard(route, state));
    backend.expectOne('/api/authentication/me').flush(KEEPER);

    const tree = await result;
    expect(tree).toBeInstanceOf(UrlTree);
    expect((tree as UrlTree).toString()).toBe('/plants');
  });

  it('lets through a keeper who holds it', async () => {
    const guard = requiresPermission(Permission.almanacApprove);
    const result = run(() => guard(route, state));
    backend
      .expectOne('/api/authentication/me')
      .flush({ ...KEEPER, permissions: [...KEEPER.permissions, 'almanac.approve'] });

    expect(await result).toBe(true);
  });

  it('sends an unauthenticated visitor to login rather than to the plant list', async () => {
    const guard = requiresPermission(Permission.almanacApprove);
    const result = run(() => guard(route, state));
    backend
      .expectOne('/api/authentication/me')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    expect((await result as UrlTree).toString()).toBe('/login?returnUrl=%2Fspecies%2Fabc');
  });
});
