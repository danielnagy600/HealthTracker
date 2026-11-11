import { Meal } from '../../../core/modules/calories/models/meal';

/** What the food entry form edits (`calories` is null while the input is empty). */
export interface FoodEntryFormModel {
  name: string;
  calories: number | null;
  meal: Meal;
}
