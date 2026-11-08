import { ReminderStatus } from './reminder-status';

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
