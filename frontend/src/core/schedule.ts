import { apiFetch } from './api';

export const ACTIVITY_COLORS = ['Blue', 'Green', 'Amber', 'Red', 'Purple', 'Teal'] as const;

export type ActivityColor = (typeof ACTIVITY_COLORS)[number];

export const COLOR_LABELS: Record<ActivityColor, string> = {
  Blue: 'Blue',
  Green: 'Green',
  Amber: 'Amber',
  Red: 'Red',
  Purple: 'Purple',
  Teal: 'Teal'
};

export const ACTIVITY_COLOR_BG: Record<ActivityColor, string> = {
  Blue: 'bg-act-blue',
  Green: 'bg-act-green',
  Amber: 'bg-act-amber',
  Red: 'bg-act-red',
  Purple: 'bg-act-purple',
  Teal: 'bg-act-teal'
};

export interface Activity {
  id: string;
  date: string;
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

export function toMinutes(time: string): number {
  const [hours = '0', minutes = '0'] = time.split(':');
  return Number(hours) * 60 + Number(minutes);
}

export function toHm(time: string): string {
  return time.slice(0, 5);
}

export function formatDuration(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  if (hours === 0) return `${rest}m`;
  if (rest === 0) return `${hours}h`;
  return `${hours}h ${rest}m`;
}

export function toIsoDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

export function fromIsoDate(iso: string): Date {
  const [year, month, day] = iso.split('-').map(Number);
  return new Date(year ?? 1970, (month ?? 1) - 1, day ?? 1, 12);
}

export function formatLongDate(iso: string): string {
  return fromIsoDate(iso).toLocaleDateString('en-GB', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long'
  });
}

export function addDays(iso: string, days: number): string {
  const date = fromIsoDate(iso);
  date.setDate(date.getDate() + days);
  return toIsoDate(date);
}

export function todayIso(): string {
  return toIsoDate(new Date());
}
