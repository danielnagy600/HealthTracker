import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export const ACTIVITY_COLORS = ['Blue', 'Green', 'Amber', 'Red', 'Purple', 'Teal'] as const;

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

const base = '/api/schedule';

@Injectable({ providedIn: 'root' })
export class ScheduleApi {
  private readonly http = inject(HttpClient);

  /** Re-fetches by itself whenever the `date` signal changes. */
  day(date: () => string) {
    return httpResource<DaySchedule>(() => `${base}/day?date=${date()}`);
  }

  add(activity: SaveActivityRequest) {
    return this.http.post<Activity>(`${base}/activities`, activity);
  }

  update(id: string, activity: SaveActivityRequest) {
    return this.http.put<Activity>(`${base}/activities/${id}`, activity);
  }

  remove(id: string) {
    return this.http.delete<void>(`${base}/activities/${id}`);
  }
}

export function toMinutes(time: string): number {
  const [hours = '0', minutes = '0'] = time.split(':');
  return Number(hours) * 60 + Number(minutes);
}

/** `09:30:00` → `09:30` */
export function toHm(time: string): string {
  return time.slice(0, 5);
}
