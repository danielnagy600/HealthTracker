import { Activity } from '../../../core/modules/schedule/models/activity';

/** Everything the timeline template needs to draw one activity. */
export interface TimelineBlock {
  activity: Activity;
  top: number;
  height: number;
  left: string;
  width: string;
  showNote: boolean;
  color: string;
  selected: boolean;
  tooltip: string;
}
