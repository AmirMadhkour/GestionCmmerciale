import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { APP_NAME } from '../../core/constants/app.constants';
import { Role } from '../../core/constants/roles';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

interface NavLink {
  path: string;
  label: string;
}

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html'
})
export class NavbarComponent {
  private readonly auth = inject(AuthService);
  private readonly notifier = inject(NotificationService);

  readonly appName = APP_NAME;
  readonly user = this.auth.currentUser;
  readonly roleLabel = this.auth.roleLabel;
  readonly menuOpen = signal(false);

  readonly links: NavLink[] = [
    { path: '/orders', label: 'Commandes' },
    { path: '/clients', label: 'Clients' },
    { path: '/products', label: 'Produits' }
  ];

  readonly initials = computed(() => {
    const user = this.user();
    if (!user) {
      return '';
    }
    const source = user.fullName.trim() || user.email;
    return source
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join('');
  });

  readonly roleBadgeClass = computed(() =>
    this.user()?.role === Role.Admin ? 'bg-amber-100 text-amber-800' : 'bg-sky-100 text-sky-800'
  );

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  logout(): void {
    this.closeMenu();
    this.auth.logout();
    this.notifier.info('Vous êtes déconnecté.');
  }
}
