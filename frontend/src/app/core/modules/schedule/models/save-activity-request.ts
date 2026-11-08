import { ActivityColor } from './activity-color';

export interface SaveActivityRequest {
  date: string;
  startTime: string;
  endTime: string;
  title: string;
  color: ActivityColor;
  note: string | null;
}
