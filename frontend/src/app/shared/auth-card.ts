import { Component, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

/**
 * The centered card of the login / register pages.
 * Content projection: whatever the parent puts between the tags lands in <ng-content />,
 * elements marked with the `footer` attribute land in the second slot.
 */
@Component({
  selector: 'app-auth-card',
  imports: [MatCardModule, MatIconModule],
  template: `
    <mat-card appearance="outlined">
      <mat-card-header>
        <mat-card-title class="brand"
          ><mat-icon>health_and_safety</mat-icon> HealthTracker</mat-card-title
        >
        <mat-card-subtitle>{{ subtitle() }}</mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
        <ng-content />
      </mat-card-content>
      <mat-card-footer>
        <ng-content select="[footer]" />
      </mat-card-footer>
    </mat-card>
  `,
  styles: `
    mat-card {
      max-width: 400px;
      margin: 8vh auto;
      padding: 16px;
    }
    .brand {
      display: flex;
      align-items: center;
      gap: 6px;
      font: var(--mat-sys-headline-small);
    }
    .brand mat-icon {
      color: var(--mat-sys-primary);
    }
    mat-card-subtitle {
      margin: 4px 0 16px;
      font-style: italic;
    }
    mat-card-footer {
      padding: 0 16px 8px;
      color: var(--mat-sys-on-surface-variant);
    }
  `,
})
export class AuthCard {
  readonly subtitle = input.required<string>();
}
