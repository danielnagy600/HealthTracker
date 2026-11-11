import { FoodEntry } from '../../../core/modules/calories/models/food-entry';
import { Meal } from '../../../core/modules/calories/models/meal';

/**
 * Which panel is open below the meals. Only one at a time, so one signal instead of three.
 * A discriminated union: `kind` tells TypeScript which shape the rest has.
 */
export type Panel =
  { kind: 'none' } | { kind: 'goal' } | { kind: 'entry'; editing: FoodEntry | null; meal: Meal };
