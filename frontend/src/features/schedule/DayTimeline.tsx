import type { Activity, DaySchedule } from '../../core/schedule';
import { formatDuration, toHm, toMinutes } from '../../core/schedule';
import { placeActivities } from './lanes';

/** Egy óra magassága képpontban – ebből számoljuk a blokkok helyét és méretét. */
const HOUR_HEIGHT = 56;

interface Props {
  day: DaySchedule;
  onSelect: (activity: Activity) => void;
  selectedId: string | null;
}

/**
 * A nap vizuális idővonala: a blokkok helye és magassága az időpontokból jön,
 * az egymást átfedő elfoglaltságok pedig egymás mellé kerülnek.
 */
export function DayTimeline({ day, onSelect, selectedId }: Props) {
  const windowStart = toMinutes(day.windowStart);
  const windowEnd = toMinutes(day.windowEnd);
  const totalMinutes = Math.max(60, windowEnd - windowStart);

  // Óravonalak minden egész órára az ablakon belül.
  const hourMarks: number[] = [];
  for (let minute = Math.ceil(windowStart / 60) * 60; minute <= windowEnd; minute += 60) {
    hourMarks.push(minute);
  }

  const placed = placeActivities(day.activities);
  const height = (totalMinutes / 60) * HOUR_HEIGHT;

  return (
    <div className="timeline" style={{ height: `${height}px` }}>
      {hourMarks.map((minute) => (
        <div
          key={minute}
          className="hour-line"
          style={{ top: `${((minute - windowStart) / 60) * HOUR_HEIGHT}px` }}
        >
          <span className="hour-label">{String(Math.floor(minute / 60)).padStart(2, '0')}:00</span>
        </div>
      ))}

      {day.activities.length === 0 && (
        <p className="timeline-empty muted">
          This day is empty. Add your first activity! 🗓️
        </p>
      )}

      {placed.map(({ activity, lane, laneCount }) => {
        const start = toMinutes(activity.startTime);
        const top = ((start - windowStart) / 60) * HOUR_HEIGHT;
        const blockHeight = (activity.durationMinutes / 60) * HOUR_HEIGHT;
        const width = 100 / laneCount;

        return (
          <button
            key={activity.id}
            type="button"
            className={`activity act-${activity.color.toLowerCase()}${
              activity.id === selectedId ? ' is-selected' : ''
            }`}
            style={{
              top: `${top}px`,
              height: `${Math.max(22, blockHeight)}px`,
              left: `calc(${lane * width}% + 0.15rem)`,
              width: `calc(${width}% - 0.3rem)`
            }}
            onClick={() => onSelect(activity)}
            title={`${toHm(activity.startTime)}–${toHm(activity.endTime)} · ${activity.title}${
              activity.note ? `\n${activity.note}` : ''
            }`}
          >
            <span className="activity-title">{activity.title}</span>
            <span className="activity-time">
              {toHm(activity.startTime)}–{toHm(activity.endTime)} · {formatDuration(activity.durationMinutes)}
            </span>
            {activity.note && blockHeight >= 64 && <span className="activity-note">{activity.note}</span>}
          </button>
        );
      })}
    </div>
  );
}
