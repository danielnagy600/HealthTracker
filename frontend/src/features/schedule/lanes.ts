import type { Activity } from '../../core/schedule';
import { toMinutes } from '../../core/schedule';

export interface PlacedActivity {
  activity: Activity;
  lane: number;
  laneCount: number;
}

export function placeActivities(activities: Activity[]): PlacedActivity[] {
  const sorted = [...activities].sort(
    (a, b) => toMinutes(a.startTime) - toMinutes(b.startTime) || toMinutes(a.endTime) - toMinutes(b.endTime)
  );

  const placed: PlacedActivity[] = [];
  let group: { activity: Activity; lane: number }[] = [];
  let laneEnds: number[] = [];
  let groupEnd = -1;

  function flushGroup(): void {
    for (const item of group) {
      placed.push({ ...item, laneCount: laneEnds.length });
    }
    group = [];
    laneEnds = [];
    groupEnd = -1;
  }

  for (const activity of sorted) {
    const start = toMinutes(activity.startTime);
    const end = toMinutes(activity.endTime);

    if (group.length > 0 && start >= groupEnd) {
      flushGroup();
    }

    let lane = laneEnds.findIndex((laneEnd) => laneEnd <= start);
    if (lane === -1) {
      lane = laneEnds.length;
    }

    laneEnds[lane] = end;
    group.push({ activity, lane });
    groupEnd = Math.max(groupEnd, end);
  }

  flushGroup();
  return placed;
}
