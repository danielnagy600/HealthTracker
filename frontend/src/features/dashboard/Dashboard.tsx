import { useCallback, useEffect, useState } from 'react';
import { formatTime } from '../../core/format';
import { water } from '../../core/water';
import type { DailySummary, Reminder } from '../../core/water';

function statusClass(status: Reminder['status']): string {
  return status === 'Behind' ? 'bg-amber' : status === 'GoalReached' ? 'bg-green' : 'bg-blue';
}

export function Dashboard() {
  const [summary, setSummary] = useState<DailySummary | null>(null);
  const [reminder, setReminder] = useState<Reminder | null>(null);
  const [customAmount, setCustomAmount] = useState('250');

  const refresh = useCallback((): void => {
    water.getSummary().then(setSummary).catch(console.error);
    water.getReminder().then(setReminder).catch(console.error);
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  async function add(amountMl: number): Promise<void> {
    if (!amountMl || amountMl <= 0) {
      return;
    }
    try {
      await water.addIntake(amountMl);
      refresh();
    } catch (error) {
      console.error(error);
    }
  }

  return (
    <main className="mx-auto my-6 flex max-w-[640px] flex-col gap-[1.1rem] px-4">
        {reminder && (
          <section className={`flex flex-col gap-1 rounded-[14px] p-[1rem_1.25rem] text-white ${statusClass(reminder.status)}`}>
            <strong>{reminder.message}</strong>
            {reminder.nextReminderAt && (
              <span className="text-[0.85rem] opacity-90">
                Next: {formatTime(reminder.nextReminderAt)} · ~{reminder.nextDoseMl} ml
              </span>
            )}
          </section>
        )}

        {summary ? (
          <>
            <section className="surface p-[1.1rem_1.25rem]">
              <div className="flex items-baseline gap-2">
                <span className="text-[1.7rem] font-bold">{summary.consumedMl} ml</span>
                <span className="text-muted">
                  / {summary.targetMl} ml · {summary.remainingMl} ml left
                </span>
              </div>
              <div className="mt-[0.7rem] mb-[0.4rem] h-3 overflow-hidden rounded-full bg-track">
                <div
                  className="h-full rounded-full bg-[linear-gradient(90deg,var(--color-blue),var(--color-blue-dark))] transition-[width] duration-300 ease-in-out"
                  style={{ width: `${Math.min(100, summary.percentComplete)}%` }}
                ></div>
              </div>
              <div className="text-muted">{summary.percentComplete}% of daily goal</div>
            </section>

            <section className="surface flex flex-wrap items-center gap-[0.6rem] p-[1.1rem_1.25rem]">
              <button className="btn" onClick={() => void add(250)}>+250 ml</button>
              <button className="btn" onClick={() => void add(500)}>+500 ml</button>
              <span className="ml-auto flex gap-[0.4rem]">
                <input
                  className="field w-[90px] p-[0.55rem]"
                  type="number"
                  min="1"
                  value={customAmount}
                  onChange={(event) => setCustomAmount(event.target.value)}
                />
                <button className="btn" onClick={() => void add(Number(customAmount))}>Add</button>
              </span>
            </section>

            <section className="surface p-[1.1rem_1.25rem]">
              <h3 className="m-0 mb-[0.6rem]">Today</h3>
              {summary.intakes.length === 0 ? (
                <p className="text-muted">No water logged yet. Add your first glass! 🥛</p>
              ) : (
                <ul className="m-0 list-none p-0">
                  {summary.intakes.map((intake) => (
                    <li key={intake.id} className="flex justify-between border-b border-border py-2 last:border-b-0">
                      <span>{formatTime(intake.recordedAt)}</span>
                      <span>{intake.amountMl} ml</span>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </>
        ) : (
          <p className="text-muted">Loading…</p>
        )}
    </main>
  );
}
