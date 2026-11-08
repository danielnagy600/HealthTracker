import { CalorieStatus } from './calorie-status';
import { Meal } from './meal';
import { MealGroup } from './meal-group';

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
