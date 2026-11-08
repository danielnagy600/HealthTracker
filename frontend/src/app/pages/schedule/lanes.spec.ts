import { Activity } from '../../core/modules/schedule/models/activity';
import { placeActivities } from './lanes';

function activity(id: string, startTime: string, endTime: string): Activity {
  return {
    id,
    date: '2026-01-01',
    startTime,
    endTime,
    title: id,
    color: 'Blue',
    note: null,
    durationMinutes: 0,
  };
}

describe('placeActivities', () => {
  it('keeps separate activities in a single lane', () => {
    const placed = placeActivities([
      activity('a', '09:00', '10:00'),
      activity('b', '10:00', '11:00'),
    ]);
    expect(placed.map((p) => [p.activity.id, p.lane, p.laneCount])).toEqual([
      ['a', 0, 1],
      ['b', 0, 1],
    ]);
  });

  it('puts overlapping activities side by side', () => {
    const placed = placeActivities([
      activity('a', '09:00', '11:00'),
      activity('b', '10:00', '12:00'),
      activity('c', '11:00', '12:00'),
    ]);
    expect(placed.map((p) => [p.activity.id, p.lane, p.laneCount])).toEqual([
      ['a', 0, 2],
      ['b', 1, 2],
      ['c', 0, 2],
    ]);
  });
});
