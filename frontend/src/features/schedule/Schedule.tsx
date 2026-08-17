import { useCallback, useEffect, useState } from 'react';
import type { Activity, DaySchedule, SaveActivityRequest } from '../../core/schedule';
import { addDays, formatDuration, formatLongDate, schedule, toHm, todayIso } from '../../core/schedule';
import { ActivityForm } from './ActivityForm';
import { DayTimeline } from './DayTimeline';

/** A napi elfoglaltságok kezelése: dátumváltó, összesítés és vizuális idővonal. */
export function Schedule() {
  const [date, setDate] = useState(todayIso());
  const [day, setDay] = useState<DaySchedule | null>(null);
  const [editing, setEditing] = useState<Activity | null>(null);
  const [formOpen, setFormOpen] = useState(false);

  const refresh = useCallback((): void => {
    schedule
      .getDay(date)
      .then(setDay)
      .catch((error: unknown) => console.error(error));
  }, [date]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  function closeForm(): void {
    setFormOpen(false);
    setEditing(null);
  }

  async function save(request: SaveActivityRequest): Promise<void> {
    if (editing) {
      await schedule.update(editing.id, request);
    } else {
      await schedule.add(request);
    }
    closeForm();
    refresh();
  }

  async function remove(id: string): Promise<void> {
    await schedule.remove(id);
    closeForm();
    refresh();
  }

  function select(activity: Activity): void {
    setEditing(activity);
    setFormOpen(true);
  }

  const longestFreeSlot = day?.freeSlots.reduce<(typeof day.freeSlots)[number] | null>(
    (longest, slot) => (longest === null || slot.durationMinutes > longest.durationMinutes ? slot : longest),
    null
  );

  return (
    <main className="dashboard">
      <section className="date-nav">
        <button className="link" onClick={() => setDate(addDays(date, -1))} aria-label="Previous day">
          ‹
        </button>
        <div className="date-current">
          <strong>{formatLongDate(date)}</strong>
          {date !== todayIso() && (
            <button className="link" onClick={() => setDate(todayIso())}>
              Jump to today
            </button>
          )}
        </div>
        <button className="link" onClick={() => setDate(addDays(date, 1))} aria-label="Next day">
          ›
        </button>
      </section>

      {day ? (
        <>
          <section className="day-summary">
            <span>
              <strong>{formatDuration(day.busyMinutes)}</strong> busy
            </span>
            <span className="muted">
              {formatDuration(day.freeMinutes)} free · {day.activities.length} activities
            </span>
            {longestFreeSlot && longestFreeSlot.durationMinutes > 0 && (
              <span className="muted">
                Longest free slot: {toHm(longestFreeSlot.start)}–{toHm(longestFreeSlot.end)}
              </span>
            )}
          </section>

          {day.conflicts.length > 0 && (
            <section className="banner behind">
              <strong>Conflict in your day</strong>
              {day.conflicts.map((conflict) => (
                <span key={`${conflict.firstId}-${conflict.secondId}`} className="next">
                  “{conflict.firstTitle}” and “{conflict.secondTitle}” overlap{' '}
                  {toHm(conflict.overlapStart)}–{toHm(conflict.overlapEnd)} (
                  {formatDuration(conflict.overlapMinutes)})
                </span>
              ))}
            </section>
          )}

          <section className="timeline-card">
            <DayTimeline day={day} onSelect={select} selectedId={editing?.id ?? null} />
          </section>

          {formOpen ? (
            <ActivityForm
              editing={editing}
              date={date}
              onSave={save}
              onDelete={remove}
              onCancel={closeForm}
            />
          ) : (
            <section className="actions">
              <button onClick={() => setFormOpen(true)}>+ New activity</button>
              <span className="muted">Click a block to edit it.</span>
            </section>
          )}
        </>
      ) : (
        <p className="muted">Loading…</p>
      )}
    </main>
  );
}
