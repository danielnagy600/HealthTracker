import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { DailySummary } from './models/daily-summary';
import { IntakeItem } from './models/intake-item';
import { Reminder } from './models/reminder';
import { Settings } from './models/settings';

const base = '/api/water';

@Injectable({ providedIn: 'root' })
export class WaterApi {
  private readonly http = inject(HttpClient);

  // Reads return an httpResource (signals). They need an injection context,
  // so call them from a component field initializer.
  today() {
    return httpResource<DailySummary>(() => `${base}/day`);
  }

  reminder() {
    return httpResource<Reminder>(() => `${base}/reminder`);
  }

  // Writes return an Observable: nothing is sent until someone subscribes.
  addIntake(amountMl: number) {
    return this.http.post<IntakeItem>(`${base}/intakes`, { amountMl });
  }

  removeIntake(id: string) {
    return this.http.delete<void>(`${base}/intakes/${id}`);
  }

  getSettings() {
    return this.http.get<Settings>(`${base}/settings`);
  }

  updateSettings(settings: Settings) {
    return this.http.put<Settings>(`${base}/settings`, settings);
  }
}
