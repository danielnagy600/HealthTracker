import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export interface IntakeItem {
  id: string;
  recordedAt: string;
  amountMl: number;
}

export interface DailySummary {
  date: string;
  targetMl: number;
  consumedMl: number;
  remainingMl: number;
  percentComplete: number;
  intakes: IntakeItem[];
}

export type ReminderStatus = 'GoalReached' | 'OnTrack' | 'Behind';

export interface Reminder {
  consumedMl: number;
  targetMl: number;
  remainingMl: number;
  expectedByNowMl: number;
  deficitMl: number;
  status: ReminderStatus;
  nextDoseMl: number;
  nextReminderAt: string | null;
  message: string;
}

export interface Settings {
  dailyTargetMl: number;
  wakeTime: string;
  sleepTime: string;
}

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
