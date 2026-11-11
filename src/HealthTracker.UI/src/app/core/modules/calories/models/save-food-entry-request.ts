import { Meal } from './meal';

export interface SaveFoodEntryRequest {
  date: string;
  meal: Meal;
  name: string;
  calories: number;
}
