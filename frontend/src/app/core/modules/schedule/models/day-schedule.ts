import { Activity } from './activity';
import { Conflict } from './conflict';
import { TimeSlot } from './time-slot';

export interface DaySchedule {
  date: string;
  windowStart: string;
  windowEnd: string;
  busyMinutes: number;
  freeMinutes: number;
  activities: Activity[];
  freeSlots: TimeSlot[];
  conflicts: Conflict[];
}
