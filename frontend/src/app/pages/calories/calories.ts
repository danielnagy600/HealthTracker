import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { todayIso } from '../../core/common/dates';
import { CaloriesApi } from '../../core/modules/calories/calories-api';
import { FoodEntry } from '../../core/modules/calories/models/food-entry';
import { MEAL_ICONS, Meal } from '../../core/modules/calories/models/meal';
import { statusClass } from '../../core/modules/calories/models/calorie-status';
import { DateNav } from '../../shared/date-nav';
import { FoodEntryForm } from './food-entry-form';
import { GoalEditor } from './goal-editor';
import { Panel } from './models/panel';

@Component({
  selector: 'app-calories',
  imports: [
    DatePipe,
    DateNav,
    FoodEntryForm,
    GoalEditor,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatListModule,
    MatProgressBarModule,
  ],
  templateUrl: './calories.html',
  styleUrl: './calories.css',
})
export default class Calories {
  private readonly calories = inject(CaloriesApi);

  protected readonly date = signal(todayIso());
  // Reads `date()` inside, so changing the date re-fetches automatically.
  protected readonly day = this.calories.day(this.date);

  protected readonly panel = signal<Panel>({ kind: 'none' });
  protected readonly mealIcons = MEAL_ICONS;
  protected readonly statusClass = statusClass;

  // Note: value() throws while the resource is in an error state, so check hasValue() first.
  protected readonly progress = computed(() =>
    this.day.hasValue() ? Math.min(100, this.day.value().percentOfTarget) : 0,
  );

  protected openEntry(meal: Meal, editing: FoodEntry | null = null): void {
    this.panel.set({ kind: 'entry', editing, meal });
  }

  protected openGoal(): void {
    this.panel.set({ kind: 'goal' });
  }

  protected close(): void {
    this.panel.set({ kind: 'none' });
  }

  protected onSaved(): void {
    this.close();
    this.day.reload();
  }
}
