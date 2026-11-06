import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min } from '@angular/forms/signals';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { errorMessage } from '../../core/http/error-message';
import { ReminderStatus, WaterApi } from '../../core/modules/water';

@Component({
  selector: 'app-water-tracker',
  imports: [
    DatePipe,
    FormField,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatProgressBarModule,
  ],
  templateUrl: './water-tracker.html',
  styleUrl: './water-tracker.css',
})
export default class WaterTracker {
  private readonly water = inject(WaterApi);
  private readonly snackBar = inject(MatSnackBar);

  // httpResource: starts loading immediately, exposes value()/isLoading()/error() as signals.
  protected readonly summary = this.water.today();
  protected readonly reminder = this.water.reminder();

  // Note: value() throws while the resource is in an error state, so check hasValue() first.
  protected readonly progress = computed(() =>
    this.summary.hasValue() ? Math.min(100, this.summary.value().percentComplete) : 0,
  );

  // A one-field form: the field tree *is* the number input.
  protected readonly customAmount = form(signal<number | null>(250), (path) => min(path, 1));

  protected add(amountMl: number | null): void {
    if (!amountMl || amountMl <= 0) {
      return;
    }
    this.water.addIntake(amountMl).subscribe({
      next: () => this.refresh(),
      error: (error: unknown) => this.showError(error, 'Could not add the intake.'),
    });
  }

  protected remove(id: string): void {
    this.water.removeIntake(id).subscribe({
      next: () => this.refresh(),
      error: (error: unknown) => this.showError(error, 'Could not delete the intake.'),
    });
  }

  protected statusClass(status: ReminderStatus): string {
    return status === 'Behind' ? 'amber' : status === 'GoalReached' ? 'green' : '';
  }

  private refresh(): void {
    this.summary.reload();
    this.reminder.reload();
  }

  private showError(error: unknown, fallback: string): void {
    console.error(error);
    this.snackBar.open(errorMessage(error, fallback), 'OK', { duration: 5000 });
  }
}
