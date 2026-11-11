import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Activity } from './models/activity';
import { DaySchedule } from './models/day-schedule';
import { SaveActivityRequest } from './models/save-activity-request';

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
