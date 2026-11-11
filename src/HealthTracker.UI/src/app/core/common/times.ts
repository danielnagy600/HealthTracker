/** `09:30:00` → 570 (minutes since midnight) */
export function toMinutes(time: string): number {
  const [hours = '0', minutes = '0'] = time.split(':');
  return Number(hours) * 60 + Number(minutes);
}

/** `09:30:00` → `09:30` */
export function toHm(time: string): string {
  return time.slice(0, 5);
}
