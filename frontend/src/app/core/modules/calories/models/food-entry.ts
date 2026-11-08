import { Meal } from './meal';

export interface FoodEntry {
  id: string;
  date: string;
  meal: Meal;
  name: string;
  calories: number;
  recordedAt: string;
}
