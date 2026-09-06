import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth';
import { Permission, PermissionName } from './core/permissions';

interface NavItem {
  readonly path: string;
  readonly label: string;
  readonly icon: string;
  readonly permission: PermissionName;
}

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  private readonly allNav: readonly NavItem[] = [
    { path: '/plants', label: 'Plants', icon: '🪴', permission: Permission.plantsRead },
    { path: '/species', label: 'Species', icon: '🌿', permission: Permission.almanacRead },
    { path: '/lookups', label: 'Reference data', icon: '⚙️', permission: Permission.almanacRead },
    {
      path: '/almanac-review',
      label: 'Review',
      icon: '📋',
      permission: Permission.almanacApprove,
    },
  ];

  /**
   * Only what this keeper can actually reach. Hiding a link is a courtesy - the route
   * guard refuses it and the API refuses it again - but a menu full of dead ends is
   * worse than a short one.
   */
  protected readonly nav = computed(() =>
    this.auth.isSignedIn() ? this.allNav.filter((item) => this.auth.has(item.permission)) : [],
  );

  protected readonly signedIn = this.auth.isSignedIn;
  protected readonly displayName = this.auth.displayName;

  protected async signOut(): Promise<void> {
    await this.auth.signOut();
    await this.router.navigate(['/login']);
  }
}
