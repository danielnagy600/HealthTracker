import { FoodEntry } from './food-entry';
import { Meal } from './meal';

export interface MealGroup {
  meal: Meal;
  kcal: number;
  entryCount: number;
  entries: FoodEntry[];
}
