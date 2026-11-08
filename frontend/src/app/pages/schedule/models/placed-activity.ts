import { Activity } from '../../../core/modules/schedule/models/activity';

/** An activity with its column in the timeline (`lane` of `laneCount`). */
export interface PlacedActivity {
  activity: Activity;
  lane: number;
  laneCount: number;
}
