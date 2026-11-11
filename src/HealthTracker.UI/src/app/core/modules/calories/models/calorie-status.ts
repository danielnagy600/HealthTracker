export type CalorieStatus = 'Under' | 'OnTarget' | 'Over';

/** Modifier class for the global `.banner`. */
export function statusClass(status: CalorieStatus): string {
  return status === 'Over' ? 'amber' : status === 'OnTarget' ? 'green' : '';
}
