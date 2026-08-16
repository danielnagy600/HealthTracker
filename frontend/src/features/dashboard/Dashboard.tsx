import { useCallback, useEffect, useState } from 'react';
import { formatTime } from '../../core/format';
import { water } from '../../core/water';
import type { DailySummary, Reminder } from '../../core/water';

function statusClass(status: Reminder['status']): string {
  return status === 'Behind' ? 'behind' : status === 'GoalReached' ? 'done' : 'ontrack';
}

export function Dashboard() {
  const [summary, setSummary] = useState<DailySummary | null>(null);
  const [reminder, setReminder] = useState<Reminder | null>(null);
  // Stringként tároljuk, hogy az input üresre törlése is kezelhető legyen.
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
    <main className="dashboard">
        {reminder && (
          <section className={`banner ${statusClass(reminder.status)}`}>
            <strong>{reminder.message}</strong>
            {reminder.nextReminderAt && (
              <span className="next">
                Next: {formatTime(reminder.nextReminderAt)} · ~{reminder.nextDoseMl} ml
              </span>
            )}
          </section>
        )}

        {summary ? (
          <>
            <section className="progress-card">
              <div className="numbers">
                <span className="big">{summary.consumedMl} ml</span>
                <span className="muted">
                  / {summary.targetMl} ml · {summary.remainingMl} ml left
                </span>
              </div>
              <div className="bar">
                <div
                  className="fill"
                  style={{ width: `${Math.min(100, summary.percentComplete)}%` }}
                ></div>
              </div>
              <div className="muted">{summary.percentComplete}% of daily goal</div>
            </section>

            <section className="actions">
              <button onClick={() => void add(250)}>+250 ml</button>
              <button onClick={() => void add(500)}>+500 ml</button>
              <span className="custom">
                <input
                  type="number"
                  min="1"
                  value={customAmount}
                  onChange={(event) => setCustomAmount(event.target.value)}
                />
                <button onClick={() => void add(Number(customAmount))}>Add</button>
              </span>
            </section>

            <section className="log">
              <h3>Today</h3>
              {summary.intakes.length === 0 ? (
                <p className="muted">No water logged yet. Add your first glass! 🥛</p>
              ) : (
                <ul>
                  {summary.intakes.map((intake) => (
                    <li key={intake.id}>
                      <span>{formatTime(intake.recordedAt)}</span>
                      <span>{intake.amountMl} ml</span>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </>
        ) : (
          <p className="muted">Loading…</p>
        )}
    </main>
  );
}
