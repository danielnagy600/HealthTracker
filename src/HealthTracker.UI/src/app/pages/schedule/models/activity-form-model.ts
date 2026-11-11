import { ActivityColor } from '../../../core/modules/schedule/models/activity-color';

/** What the activity form edits (times as `HH:mm`, note never null). */
export interface ActivityFormModel {
  title: string;
  startTime: string;
  endTime: string;
  color: ActivityColor;
  note: string;
}
