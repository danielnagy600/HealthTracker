import { ActivityColor } from './activity-color';

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
