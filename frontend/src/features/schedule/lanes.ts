import type { Activity } from '../../core/schedule';
import { toMinutes } from '../../core/schedule';

export interface PlacedActivity {
  activity: Activity;
  /** Hányadik oszlopban jelenjen meg (0-tól). */
  lane: number;
  /** Hány oszlop van összesen az átfedő csoportjában – ebből jön a szélesség. */
  laneCount: number;
}

/**
 * Az átfedő elfoglaltságokat egymás mellé rendezi oszlopokba.
 *
 * Az egymást átfedő elemek egy "csoportot" alkotnak; a csoporton belül minden
 * elem az első olyan oszlopba kerül, amelyik az adott időpontban már szabad.
 * A csoport összes eleme ugyanazt a laneCount-ot kapja, így egyforma szélesek
 * lesznek és kitöltik a sávot.
 *
 * Tiszta függvény: csak a bemenetből számol, ezért könnyen tesztelhető.
 */
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

    // Ha ez már nem ér hozzá a csoport egyetlen eleméhez sem, lezárjuk a csoportot.
    if (group.length > 0 && start >= groupEnd) {
      flushGroup();
    }

    // Az első olyan oszlop, amelyik eddigre felszabadult.
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
