import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from './api';

// A backend Water moduljának DTO-i TypeScript-oldalon.
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

export interface Reminder {
  consumedMl: number;
  targetMl: number;
  remainingMl: number;
  expectedByNowMl: number;
  deficitMl: number;
  status: 'GoalReached' | 'OnTrack' | 'Behind';
  nextDoseMl: number;
  nextReminderAt: string | null;
  message: string;
}

export interface Settings {
  dailyTargetMl: number;
  wakeTime: string;
  sleepTime: string;
}

/** A Water modul REST-végpontjait hívó kliens. */
@Injectable({ providedIn: 'root' })
export class WaterService {
  private http = inject(HttpClient);
  private base = `${API_BASE}/api/water`;

  getSummary(): Observable<DailySummary> {
    return this.http.get<DailySummary>(`${this.base}/summary`);
  }

  getReminder(): Observable<Reminder> {
    return this.http.get<Reminder>(`${this.base}/reminder`);
  }

  addIntake(amountMl: number): Observable<IntakeItem> {
    return this.http.post<IntakeItem>(`${this.base}/intake`, { amountMl });
  }

  getSettings(): Observable<Settings> {
    return this.http.get<Settings>(`${this.base}/settings`);
  }

  updateSettings(settings: Settings): Observable<Settings> {
    return this.http.put<Settings>(`${this.base}/settings`, settings);
  }
}
