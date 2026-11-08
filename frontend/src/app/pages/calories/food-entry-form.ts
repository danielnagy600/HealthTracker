import { Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import {
  FormField,
  FormRoot,
  form,
  max,
  maxLength,
  min,
  required,
  validate,
} from '@angular/forms/signals';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';
import { errorMessage } from '../../core/http/error-message';
import { CaloriesApi } from '../../core/modules/calories/calories-api';
import { FoodEntry } from '../../core/modules/calories/models/food-entry';
import { MEAL_ICONS, MEALS, Meal } from '../../core/modules/calories/models/meal';
import { FoodEntryFormModel } from './models/food-entry-form-model';
import { Autofocus } from '../../shared/autofocus';
import { FieldError } from '../../shared/field-error';

const CALORIES_MESSAGE = 'Calories must be a positive number.';

/** Add / edit / delete one food entry. Talks to the API itself and reports back via outputs. */
@Component({
  selector: 'app-food-entry-form',
  imports: [
    FormField,
    FormRoot,
    FieldError,
    Autofocus,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './food-entry-form.html',
  styleUrl: './food-entry-form.css',
})
export class FoodEntryForm {
  private readonly calories = inject(CaloriesApi);

  readonly editing = input<FoodEntry | null>(null);
  readonly defaultMeal = input<Meal>('Breakfast');
  readonly date = input.required<string>();

  readonly saved = output();
  readonly cancelled = output();

  protected readonly meals = MEALS;
  protected readonly mealIcons = MEAL_ICONS;

  // Resets whenever `editing` or `defaultMeal` changes.
  private readonly model = linkedSignal<FoodEntryFormModel>(() => {
    const entry = this.editing();
    return entry
      ? { name: entry.name, calories: entry.calories, meal: entry.meal }
      : { name: '', calories: null, meal: this.defaultMeal() };
  });

  protected readonly entryForm = form(
    this.model,
    (path) => {
      validate(path.name, ({ value }) =>
        value().trim()
          ? undefined
          : { kind: 'required', message: 'The food name cannot be empty.' },
      );
      maxLength(path.name, 120);
      required(path.calories, { message: CALORIES_MESSAGE });
      min(path.calories, 1, { message: CALORIES_MESSAGE });
      max(path.calories, 10000, { message: 'At most 10000 kcal per entry.' });
    },
    {
      submission: {
        action: async (fields) => {
          const value = fields().value();
          const request = {
            date: this.date(),
            meal: value.meal,
            name: value.name.trim(),
            calories: Math.round(value.calories ?? 0),
          };
          const editing = this.editing();
          try {
            await firstValueFrom(
              editing ? this.calories.update(editing.id, request) : this.calories.add(request),
            );
          } catch (error) {
            console.error(error);
            // A returned error is attached to the form (shown by <app-field-error [field]="entryForm" />).
            return {
              kind: 'server',
              message: errorMessage(error, 'Saving failed. Check the values and try again.'),
            };
          }
          this.saved.emit();
          return undefined;
        },
      },
    },
  );

  private readonly deleting = signal(false);
  protected readonly deleteError = signal<string | null>(null);
  protected readonly busy = computed(() => this.entryForm().submitting() || this.deleting());

  protected async remove(): Promise<void> {
    const editing = this.editing();
    if (!editing) return;

    this.deleting.set(true);
    this.deleteError.set(null);
    try {
      await firstValueFrom(this.calories.remove(editing.id));
      this.saved.emit();
    } catch (error) {
      console.error(error);
      this.deleteError.set(errorMessage(error, 'Deleting failed.'));
    } finally {
      this.deleting.set(false);
    }
  }
}
