import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Login } from './login';
import { SignedInKeeper } from '../../core/models';

const KEEPER: SignedInKeeper = {
  id: '00000000-0000-0000-0000-000000000001',
  userName: 'keeper',
  displayName: 'Keeper',
  email: null,
  roles: ['keeper'],
  permissions: ['plants.read'],
};

/** Renders the real component against a fake backend - the template included. */
async function render() {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [Login],
    providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
  }).compileComponents();

  const fixture = TestBed.createComponent(Login);
  await fixture.whenStable();

  return { fixture, backend: TestBed.inject(HttpTestingController) };
}

function element<T extends HTMLElement>(fixture: { nativeElement: unknown }, selector: string): T {
  return (fixture.nativeElement as HTMLElement).querySelector<T>(selector)!;
}

async function fill(fixture: Awaited<ReturnType<typeof render>>['fixture'], selector: string, value: string) {
  const input = element<HTMLInputElement>(fixture, selector);
  input.value = value;
  input.dispatchEvent(new Event('input'));
  await fixture.whenStable();
}

describe('Login', () => {
  it('renders a user name and password form', async () => {
    const { fixture } = await render();

    expect(element(fixture, '#userName')).toBeTruthy();
    expect(element<HTMLInputElement>(fixture, '#password').type).toBe('password');
  });

  it('posts the credentials and does not send them anywhere else', async () => {
    const { fixture, backend } = await render();

    await fill(fixture, '#userName', 'keeper');
    await fill(fixture, '#password', 'CorrectHorseBattery1!');

    element<HTMLFormElement>(fixture, 'form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const request = backend.expectOne('/api/authentication/sign-in');
    expect(request.request.body).toEqual({
      userName: 'keeper',
      password: 'CorrectHorseBattery1!',
      rememberMe: false,
    });

    request.flush(KEEPER);
    backend.verify();
  });

  it('shows what the API said when the password is wrong', async () => {
    const { fixture, backend } = await render();

    await fill(fixture, '#userName', 'keeper');
    await fill(fixture, '#password', 'wrong');

    element<HTMLFormElement>(fixture, 'form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    backend.expectOne('/api/authentication/sign-in').flush(
      { title: 'Unauthorized', detail: 'The user name or password is incorrect.' },
      { status: 401, statusText: 'Unauthorized' },
    );

    // A macrotask first: the rejection has to travel through firstValueFrom and the
    // component's catch before there is anything for change detection to render.
    await new Promise((resolve) => setTimeout(resolve, 0));
    await fixture.whenStable();

    expect(element(fixture, '[role="alert"]').textContent).toContain(
      'The user name or password is incorrect.',
    );
    backend.verify();
  });

  it('does not post at all when the form is empty', async () => {
    const { fixture, backend } = await render();

    element<HTMLFormElement>(fixture, 'form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    backend.expectNone('/api/authentication/sign-in');
  });
});
