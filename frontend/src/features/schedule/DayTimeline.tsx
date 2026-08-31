import type { Activity, DaySchedule } from '../../core/schedule';
import { ACTIVITY_COLOR_BG, formatDuration, toHm, toMinutes } from '../../core/schedule';
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
    <div className="relative" style={{ height: `${height}px` }}>
      {hourMarks.map((minute) => (
        <div
          key={minute}
          className="absolute inset-x-0 h-px bg-border"
          style={{ top: `${((minute - windowStart) / 60) * HOUR_HEIGHT}px` }}
        >
          <span className="absolute -top-[0.55rem] -left-[3.3rem] text-[0.72rem] text-muted tabular-nums">
            {String(Math.floor(minute / 60)).padStart(2, '0')}:00
          </span>
        </div>
      ))}

      {day.activities.length === 0 && (
        <p className="absolute inset-0 m-0 flex items-center justify-center text-center text-muted">
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
            className={`absolute flex cursor-pointer flex-col gap-[0.1rem] overflow-hidden rounded-lg border-none p-[0.3rem_0.5rem] text-left text-[0.8rem] text-white transition-[filter,box-shadow] duration-150 ease-in-out hover:brightness-[1.07] ${ACTIVITY_COLOR_BG[activity.color]}${
              activity.id === selectedId ? ' shadow-[0_0_0_3px_rgba(28,36,48,0.35)]' : ''
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
            <span className="overflow-hidden text-ellipsis whitespace-nowrap font-bold">{activity.title}</span>
            <span className="text-[0.72rem] opacity-90 tabular-nums">
              {toHm(activity.startTime)}–{toHm(activity.endTime)} · {formatDuration(activity.durationMinutes)}
            </span>
            {activity.note && blockHeight >= 64 && (
              <span className="overflow-hidden text-[0.72rem] opacity-85">{activity.note}</span>
            )}
          </button>
        );
      })}
    </div>
  );
}
