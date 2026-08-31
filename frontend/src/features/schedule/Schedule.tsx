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
    <main className="mx-auto my-6 flex max-w-[640px] flex-col gap-[1.1rem] px-4">
      <section className="surface flex items-center gap-2 p-[0.6rem_1rem]">
        <button
          className="btn-link px-[0.6rem] py-[0.1rem] text-[1.4rem] leading-none"
          onClick={() => setDate(addDays(date, -1))}
          aria-label="Previous day"
        >
          ‹
        </button>
        <div className="flex flex-1 flex-col gap-[0.15rem] text-center">
          <strong>{formatLongDate(date)}</strong>
          {date !== todayIso() && (
            <button className="btn-link text-[0.8rem]" onClick={() => setDate(todayIso())}>
              Jump to today
            </button>
          )}
        </div>
        <button
          className="btn-link px-[0.6rem] py-[0.1rem] text-[1.4rem] leading-none"
          onClick={() => setDate(addDays(date, 1))}
          aria-label="Next day"
        >
          ›
        </button>
      </section>

      {day ? (
        <>
          <section className="surface flex flex-wrap items-baseline gap-3 p-[0.9rem_1.25rem] text-[0.9rem]">
            <span>
              <strong className="text-[1.25rem]">{formatDuration(day.busyMinutes)}</strong> busy
            </span>
            <span className="text-muted">
              {formatDuration(day.freeMinutes)} free · {day.activities.length} activities
            </span>
            {longestFreeSlot && longestFreeSlot.durationMinutes > 0 && (
              <span className="text-muted">
                Longest free slot: {toHm(longestFreeSlot.start)}–{toHm(longestFreeSlot.end)}
              </span>
            )}
          </section>

          {day.conflicts.length > 0 && (
            <section className="flex flex-col gap-1 rounded-[14px] bg-amber p-[1rem_1.25rem] text-white">
              <strong>Conflict in your day</strong>
              {day.conflicts.map((conflict) => (
                <span key={`${conflict.firstId}-${conflict.secondId}`} className="text-[0.85rem] opacity-90">
                  “{conflict.firstTitle}” and “{conflict.secondTitle}” overlap{' '}
                  {toHm(conflict.overlapStart)}–{toHm(conflict.overlapEnd)} (
                  {formatDuration(conflict.overlapMinutes)})
                </span>
              ))}
            </section>
          )}

          <section className="surface p-[0.9rem_1rem_0.9rem_3.6rem]">
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
            <section className="surface flex flex-wrap items-center gap-[0.6rem] p-[1.1rem_1.25rem]">
              <button className="btn" onClick={() => setFormOpen(true)}>+ New activity</button>
              <span className="text-muted">Click a block to edit it.</span>
            </section>
          )}
        </>
      ) : (
        <p className="text-muted">Loading…</p>
      )}
    </main>
  );
}
