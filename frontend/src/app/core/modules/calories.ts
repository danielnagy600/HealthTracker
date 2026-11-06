import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export const MEALS = ['Breakfast', 'Lunch', 'Dinner', 'Snack'] as const;

export type Meal = (typeof MEALS)[number];

export const MEAL_ICONS: Record<Meal, string> = {
  Breakfast: '🌅',
  Lunch: '🍽️',
  Dinner: '🌙',
  Snack: '🍎',
};

export type CalorieStatus = 'Under' | 'OnTarget' | 'Over';

export interface FoodEntry {
  id: string;
  date: string;
  meal: Meal;
  name: string;
  calories: number;
  recordedAt: string;
}

export interface MealGroup {
  meal: Meal;
  kcal: number;
  entryCount: number;
  entries: FoodEntry[];
}

export interface DayCalories {
  date: string;
  consumedKcal: number;
  targetKcal: number;
  remainingKcal: number;
  overKcal: number;
  percentOfTarget: number;
  status: CalorieStatus;
  message: string;
  largestMeal: Meal | null;
  meals: MealGroup[];
}

export interface SaveFoodEntryRequest {
  date: string;
  meal: Meal;
  name: string;
  calories: number;
}

export interface Goal {
  dailyTargetKcal: number;
}

const base = '/api/calories';

@Injectable({ providedIn: 'root' })
export class CaloriesApi {
  private readonly http = inject(HttpClient);

  /** Re-fetches by itself whenever the `date` signal changes. */
  day(date: () => string) {
    return httpResource<DayCalories>(() => `${base}/day?date=${date()}`);
  }

  add(entry: SaveFoodEntryRequest) {
    return this.http.post<FoodEntry>(`${base}/entries`, entry);
  }

  update(id: string, entry: SaveFoodEntryRequest) {
    return this.http.put<FoodEntry>(`${base}/entries/${id}`, entry);
  }

  remove(id: string) {
    return this.http.delete<void>(`${base}/entries/${id}`);
  }

  getGoal() {
    return this.http.get<Goal>(`${base}/goal`);
  }

  updateGoal(dailyTargetKcal: number) {
    return this.http.put<Goal>(`${base}/goal`, { dailyTargetKcal });
  }
}

/** Modifier class for the global `.banner`. */
export function statusClass(status: CalorieStatus): string {
  return status === 'Over' ? 'amber' : status === 'OnTarget' ? 'green' : '';
}
