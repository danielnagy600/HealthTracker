import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { DayCalories } from './models/day-calories';
import { FoodEntry } from './models/food-entry';
import { Goal } from './models/goal';
import { SaveFoodEntryRequest } from './models/save-food-entry-request';

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
