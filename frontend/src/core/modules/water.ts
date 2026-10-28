import { apiFetch } from '../common/api';

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
  getDay: (date?: string): Promise<DailySummary> =>
    apiFetch<DailySummary>(date ? `${base}/day?date=${date}` : `${base}/day`),

  getReminder: (): Promise<Reminder> => apiFetch<Reminder>(`${base}/reminder`),

  addIntake: (amountMl: number): Promise<IntakeItem> =>
    apiFetch<IntakeItem>(`${base}/intakes`, {
      method: 'POST',
      body: JSON.stringify({ amountMl })
    }),

  removeIntake: (id: string): Promise<void> =>
    apiFetch<void>(`${base}/intakes/${id}`, { method: 'DELETE' }),

  getSettings: (): Promise<Settings> => apiFetch<Settings>(`${base}/settings`),

  updateSettings: (settings: Settings): Promise<Settings> =>
    apiFetch<Settings>(`${base}/settings`, {
      method: 'PUT',
      body: JSON.stringify(settings)
    })
};
