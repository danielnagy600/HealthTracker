import { IntakeItem } from './intake-item';

export interface DailySummary {
  date: string;
  targetMl: number;
  consumedMl: number;
  remainingMl: number;
  percentComplete: number;
  intakes: IntakeItem[];
}
