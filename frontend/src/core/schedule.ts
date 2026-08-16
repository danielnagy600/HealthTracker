import { apiFetch } from './api';

// A backend Schedule moduljának DTO-i TypeScript-oldalon.

/** A választható színek – a backend ActivityColor enumjával egyezik. */
export const ACTIVITY_COLORS = ['Blue', 'Green', 'Amber', 'Red', 'Purple', 'Teal'] as const;

export type ActivityColor = (typeof ACTIVITY_COLORS)[number];

/** Magyar címkék a palettához. */
export const COLOR_LABELS: Record<ActivityColor, string> = {
  Blue: 'Kék',
  Green: 'Zöld',
  Amber: 'Borostyán',
  Red: 'Piros',
  Purple: 'Lila',
  Teal: 'Türkiz'
};

export interface Activity {
  id: string;
  /** "2026-08-16" */
  date: string;
  /** "09:00:00" */
  startTime: string;
  endTime: string;
  title: string;
  color: ActivityColor;
  note: string | null;
  durationMinutes: number;
}

export interface TimeSlot {
  start: string;
  end: string;
  durationMinutes: number;
}

export interface Conflict {
  firstId: string;
  secondId: string;
  firstTitle: string;
  secondTitle: string;
  overlapStart: string;
  overlapEnd: string;
  overlapMinutes: number;
}

export interface DaySchedule {
  date: string;
  windowStart: string;
  windowEnd: string;
  busyMinutes: number;
  freeMinutes: number;
  activities: Activity[];
  freeSlots: TimeSlot[];
  conflicts: Conflict[];
}

export interface SaveActivityRequest {
  date: string;
  startTime: string;
  endTime: string;
  title: string;
  color: ActivityColor;
  note: string | null;
}

/** A Schedule modul REST-végpontjait hívó kliens. */
export const schedule = {
  getDay: (date: string) => apiFetch<DaySchedule>(`/api/schedule/day?date=${date}`),

  add: (activity: SaveActivityRequest) =>
    apiFetch<Activity>('/api/schedule/activities', {
      method: 'POST',
      body: JSON.stringify(activity)
    }),

  update: (id: string, activity: SaveActivityRequest) =>
    apiFetch<Activity>(`/api/schedule/activities/${id}`, {
      method: 'PUT',
      body: JSON.stringify(activity)
    }),

  remove: (id: string) => apiFetch<void>(`/api/schedule/activities/${id}`, { method: 'DELETE' })
};

// ---- Idő- és dátumsegédek ----

/** "09:30:00" → 570 (éjfél óta eltelt percek). */
export function toMinutes(time: string): number {
  const [hours = '0', minutes = '0'] = time.split(':');
  return Number(hours) * 60 + Number(minutes);
}

/** "09:30:00" → "09:30" (az input[type=time] is ezt várja). */
export function toHm(time: string): string {
  return time.slice(0, 5);
}

/** 90 → "1 ó 30 p" */
export function formatDuration(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  if (hours === 0) return `${rest} p`;
  if (rest === 0) return `${hours} ó`;
  return `${hours} ó ${rest} p`;
}

/** Date → "2026-08-16" (helyi idő szerint, nem UTC-ben). */
export function toIsoDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

/** "2026-08-16" → helyi Date (dél, hogy a nyári időszámítás se tolja el a napot). */
export function fromIsoDate(iso: string): Date {
  const [year, month, day] = iso.split('-').map(Number);
  return new Date(year ?? 1970, (month ?? 1) - 1, day ?? 1, 12);
}

/** "2026-08-16" → "2026. augusztus 16., vasárnap" */
export function formatLongDate(iso: string): string {
  return fromIsoDate(iso).toLocaleDateString('hu-HU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long'
  });
}

/** Napok hozzáadása egy ISO dátumhoz. */
export function addDays(iso: string, days: number): string {
  const date = fromIsoDate(iso);
  date.setDate(date.getDate() + days);
  return toIsoDate(date);
}

export function todayIso(): string {
  return toIsoDate(new Date());
}
