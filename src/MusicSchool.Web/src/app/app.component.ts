import { AsyncPipe, NgFor } from '@angular/common';
import { BreakpointObserver } from '@angular/cdk/layout';
import { FormsModule } from '@angular/forms';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { I18nService } from './core/services/i18n.service';
import { ThemeService } from './core/services/theme.service';
import { TranslatePipe } from './shared/pipes/translate.pipe';

interface ShellNavItem {
  readonly path: string;
  readonly icon: string;
  readonly labelKey: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    AsyncPipe,
    FormsModule,
    NgFor,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatToolbarModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    TranslatePipe
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly router = inject(Router);
  protected readonly i18n = inject(I18nService);
  protected readonly theme = inject(ThemeService);
  protected readonly isAuthRoute = signal(this.getIsAuthRoute(this.router.url));

  protected readonly isMobile$ = this.breakpointObserver
    .observe('(max-width: 900px)')
    .pipe(map((state) => state.matches));

  protected globalSearch = '';

  protected readonly navItems: ShellNavItem[] = [
    { path: '/dashboard', icon: 'dashboard', labelKey: 'nav.dashboard' },
    { path: '/lessons', icon: 'calendar_month', labelKey: 'nav.lessons' },
    { path: '/families', icon: 'diversity_3', labelKey: 'nav.families' },
    { path: '/users', icon: 'manage_accounts', labelKey: 'nav.users' },
    { path: '/teachers', icon: 'school', labelKey: 'nav.teachers' },
    { path: '/curriculum', icon: 'library_music', labelKey: 'nav.curriculum' },
    { path: '/payments', icon: 'payments', labelKey: 'nav.payments' }
  ];

  constructor() {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.isAuthRoute.set(this.getIsAuthRoute(event.urlAfterRedirects)));
  }

  protected closeMobileNav(drawer: MatSidenav): void {
    if (drawer.mode === 'over') {
      drawer.close();
    }
  }

  private getIsAuthRoute(url: string): boolean {
    const cleanUrl = url.split('?')[0].split('#')[0];
    return cleanUrl === '/' || cleanUrl === '/login';
  }
}
