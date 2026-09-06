import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { Auth } from './core/auth';
import { signal } from '@angular/core';

/**
 * A stand-in for the session, so these tests are about what the shell renders rather
 * than about how the session is fetched. `Auth`'s own behaviour is covered in auth.spec.
 */
function fakeAuth(permissions: string[] | null) {
  const signedIn = signal(permissions !== null);
  return {
    isSignedIn: signedIn,
    displayName: signal(permissions === null ? '' : 'Keeper'),
    has: (permission: string) => permissions?.includes(permission) ?? false,
    signOut: () => Promise.resolve(),
  };
}

async function navLabels(permissions: string[] | null): Promise<string[]> {
  await TestBed.configureTestingModule({
    imports: [App],
    providers: [provideRouter([]), { provide: Auth, useValue: fakeAuth(permissions) }],
  }).compileComponents();

  const fixture = TestBed.createComponent(App);
  await fixture.whenStable();

  const links = (fixture.nativeElement as HTMLElement).querySelectorAll('nav a');
  return Array.from(links).map((link) => link.textContent?.trim() ?? '');
}

describe('App', () => {
  beforeEach(() => TestBed.resetTestingModule());

  it('creates the app', async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), { provide: Auth, useValue: fakeAuth(null) }],
    }).compileComponents();

    expect(TestBed.createComponent(App).componentInstance).toBeTruthy();
  });

  it('shows no navigation to a visitor who is not signed in', async () => {
    expect(await navLabels(null)).toEqual([]);
  });

  it('shows only what the keeper has permission to reach', async () => {
    expect(await navLabels(['plants.read', 'almanac.read'])).toEqual([
      'Plants',
      'Species',
      'Reference data',
    ]);
  });

  it('adds the review queue only for a keeper who can approve', async () => {
    expect(await navLabels(['plants.read', 'almanac.read', 'almanac.approve'])).toEqual([
      'Plants',
      'Species',
      'Reference data',
      'Review',
    ]);
  });

  it('hides the almanac from a keeper who cannot read it', async () => {
    expect(await navLabels(['plants.read'])).toEqual(['Plants']);
  });
});
