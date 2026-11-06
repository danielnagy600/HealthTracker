import { Component, inject, input, linkedSignal, output } from '@angular/core';
import { FormField, FormRoot, form, max, min, required } from '@angular/forms/signals';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';
import { errorMessage } from '../../core/http/error-message';
import { CaloriesApi } from '../../core/modules/calories';
import { Autofocus } from '../../shared/autofocus';
import { FieldError } from '../../shared/field-error';

const RANGE_MESSAGE = 'The daily target must be between 500 and 10000 kcal.';

/** Small form for the daily calorie target. An inline template, since it is short. */
@Component({
  selector: 'app-goal-editor',
  imports: [
    FormField,
    FormRoot,
    FieldError,
    Autofocus,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  template: `
    <mat-card appearance="outlined">
      <mat-card-header>
        <mat-card-title>Daily calorie target</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <form class="form-stack" [formRoot]="goalForm">
          <mat-form-field appearance="outline">
            <mat-label>Target</mat-label>
            <input matInput type="number" step="50" [formField]="goalForm.target" appAutofocus />
            <span matTextSuffix>kcal</span>
            <mat-error>{{ goalForm.target().errors()[0]?.message }}</mat-error>
          </mat-form-field>

          <app-field-error [field]="goalForm" />

          <div class="form-actions">
            <button mat-flat-button type="submit" [disabled]="goalForm().submitting()">
              {{ goalForm().submitting() ? 'Saving…' : 'Save' }}
            </button>
            <button
              mat-button
              type="button"
              (click)="cancelled.emit()"
              [disabled]="goalForm().submitting()"
            >
              Cancel
            </button>
          </div>
        </form>
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    mat-card-header {
      margin-bottom: 16px;
    }
  `,
})
export class GoalEditor {
  private readonly calories = inject(CaloriesApi);

  readonly current = input.required<number>();
  readonly saved = output();
  readonly cancelled = output();

  private readonly model = linkedSignal<{ target: number | null }>(() => ({
    target: this.current(),
  }));

  protected readonly goalForm = form(
    this.model,
    (path) => {
      required(path.target, { message: RANGE_MESSAGE });
      min(path.target, 500, { message: RANGE_MESSAGE });
      max(path.target, 10000, { message: RANGE_MESSAGE });
    },
    {
      submission: {
        action: async (fields) => {
          const target = Math.round(fields.target().value() ?? 0);
          try {
            await firstValueFrom(this.calories.updateGoal(target));
          } catch (error) {
            console.error(error);
            return { kind: 'server', message: errorMessage(error, 'Saving failed.') };
          }
          this.saved.emit();
          return undefined;
        },
      },
    },
  );
}
