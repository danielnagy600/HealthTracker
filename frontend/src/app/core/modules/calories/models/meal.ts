/** The meals the API accepts. `as const` keeps the exact strings, so the type below can be derived. */
export const MEALS = ['Breakfast', 'Lunch', 'Dinner', 'Snack'] as const;

/** 'Breakfast' | 'Lunch' | 'Dinner' | 'Snack' */
export type Meal = (typeof MEALS)[number];

export const MEAL_ICONS: Record<Meal, string> = {
  Breakfast: '🌅',
  Lunch: '🍽️',
  Dinner: '🌙',
  Snack: '🍎',
};
