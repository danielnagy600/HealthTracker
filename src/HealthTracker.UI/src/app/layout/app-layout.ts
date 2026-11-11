import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from '../core/auth/auth';

/** Shared toolbar + tab navigation around every protected page. */
@Component({
  selector: 'app-layout',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.css',
})
export class AppLayout {
  protected readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly links = [
    { path: '/', label: 'Water', icon: 'water_drop', exact: true },
    { path: '/schedule', label: 'Schedule', icon: 'calendar_month', exact: false },
    { path: '/calories', label: 'Calories', icon: 'restaurant', exact: false },
  ];

  protected logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
