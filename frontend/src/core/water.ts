import { apiFetch } from './api';

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

const base = '/api/water';

export const water = {
  getSummary: (): Promise<DailySummary> => apiFetch<DailySummary>(`${base}/summary`),

  getReminder: (): Promise<Reminder> => apiFetch<Reminder>(`${base}/reminder`),

  addIntake: (amountMl: number): Promise<IntakeItem> =>
    apiFetch<IntakeItem>(`${base}/intake`, {
      method: 'POST',
      body: JSON.stringify({ amountMl })
    }),

  getSettings: (): Promise<Settings> => apiFetch<Settings>(`${base}/settings`),

  updateSettings: (settings: Settings): Promise<Settings> =>
    apiFetch<Settings>(`${base}/settings`, {
      method: 'PUT',
      body: JSON.stringify(settings)
    })
};
