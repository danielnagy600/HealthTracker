import { apiFetch } from './api';

export const MEALS = ['Breakfast', 'Lunch', 'Dinner', 'Snack'] as const;

export type Meal = (typeof MEALS)[number];

export const MEAL_LABELS: Record<Meal, string> = {
  Breakfast: 'Breakfast',
  Lunch: 'Lunch',
  Dinner: 'Dinner',
  Snack: 'Snack'
};

export const MEAL_ICONS: Record<Meal, string> = {
  Breakfast: '🌅',
  Lunch: '🍽️',
  Dinner: '🌙',
  Snack: '🍎'
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

export const calories = {
  getDay: (date: string) => apiFetch<DayCalories>(`/api/calories/day?date=${date}`),

  add: (entry: SaveFoodEntryRequest) =>
    apiFetch<FoodEntry>('/api/calories/entries', {
      method: 'POST',
      body: JSON.stringify(entry)
    }),

  update: (id: string, entry: SaveFoodEntryRequest) =>
    apiFetch<FoodEntry>(`/api/calories/entries/${id}`, {
      method: 'PUT',
      body: JSON.stringify(entry)
    }),

  remove: (id: string) => apiFetch<void>(`/api/calories/entries/${id}`, { method: 'DELETE' }),

  getGoal: () => apiFetch<Goal>('/api/calories/goal'),

  updateGoal: (dailyTargetKcal: number) =>
    apiFetch<Goal>('/api/calories/goal', {
      method: 'PUT',
      body: JSON.stringify({ dailyTargetKcal })
    })
};

export function statusClass(status: CalorieStatus): string {
  return status === 'Over' ? 'bg-amber' : status === 'OnTarget' ? 'bg-green' : 'bg-blue';
}
