/** The colors the API accepts. `as const` keeps the exact strings, so the type below can be derived. */
export const ACTIVITY_COLORS = ['Blue', 'Green', 'Amber', 'Red', 'Purple', 'Teal'] as const;

/** 'Blue' | 'Green' | ... — like a C# enum, but the values are the strings themselves. */
export type ActivityColor = (typeof ACTIVITY_COLORS)[number];

/** CSS color per activity color (the variables are defined in styles.scss). */
export const ACTIVITY_COLOR_CSS: Record<ActivityColor, string> = {
  Blue: 'var(--ht-act-blue)',
  Green: 'var(--ht-act-green)',
  Amber: 'var(--ht-act-amber)',
  Red: 'var(--ht-act-red)',
  Purple: 'var(--ht-act-purple)',
  Teal: 'var(--ht-act-teal)',
};
