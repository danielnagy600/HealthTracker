import { useCallback, useEffect, useState } from 'react';
import type { DayCalories, FoodEntry, Meal, SaveFoodEntryRequest } from '../../core/calories';
import { MEAL_ICONS, MEAL_LABELS, calories, statusClass } from '../../core/calories';
import { addDays, formatLongDate, todayIso } from '../../core/schedule';
import { formatTime } from '../../core/format';
import { FoodEntryForm } from './FoodEntryForm';
import { GoalEditor } from './GoalEditor';

/** A napi kalóriák kezelése: étkezésenkénti bontás, napi keret, dátumváltó. */
export function Calories() {
  const [date, setDate] = useState(todayIso());
  const [day, setDay] = useState<DayCalories | null>(null);
  const [editing, setEditing] = useState<FoodEntry | null>(null);
  const [formMeal, setFormMeal] = useState<Meal>('Breakfast');
  const [formOpen, setFormOpen] = useState(false);
  const [goalOpen, setGoalOpen] = useState(false);

  const refresh = useCallback((): void => {
    calories
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

  async function save(request: SaveFoodEntryRequest): Promise<void> {
    if (editing) {
      await calories.update(editing.id, request);
    } else {
      await calories.add(request);
    }
    closeForm();
    refresh();
  }

  async function remove(id: string): Promise<void> {
    await calories.remove(id);
    closeForm();
    refresh();
  }

  async function saveGoal(target: number): Promise<void> {
    await calories.updateGoal(target);
    setGoalOpen(false);
    refresh();
  }

  function addTo(meal: Meal): void {
    setEditing(null);
    setFormMeal(meal);
    setGoalOpen(false);
    setFormOpen(true);
  }

  function select(entry: FoodEntry): void {
    setEditing(entry);
    setFormMeal(entry.meal);
    setGoalOpen(false);
    setFormOpen(true);
  }

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
          <section className={`banner ${statusClass(day.status)}`}>
            <strong>{day.message}</strong>
            {day.largestMeal && (
              <span className="next">
                Biggest meal: {MEAL_LABELS[day.largestMeal]}
              </span>
            )}
          </section>

          <section className="progress-card">
            <div className="numbers">
              <span className="big">{day.consumedKcal} kcal</span>
              <span className="muted">
                / {day.targetKcal} kcal ·{' '}
                {day.overKcal > 0 ? `${day.overKcal} kcal over` : `${day.remainingKcal} kcal left`}
              </span>
            </div>
            <div className="bar">
              <div
                className={`fill${day.overKcal > 0 ? ' is-over' : ''}`}
                style={{ width: `${Math.min(100, day.percentOfTarget)}%` }}
              ></div>
            </div>
            <div className="goal-row">
              <span className="muted">{day.percentOfTarget}% of daily target</span>
              <button className="link" onClick={() => { closeForm(); setGoalOpen(true); }}>
                Change target
              </button>
            </div>
          </section>

          {goalOpen && (
            <GoalEditor current={day.targetKcal} onSave={saveGoal} onCancel={() => setGoalOpen(false)} />
          )}

          {day.meals.map((group) => (
            <section className="meal-card" key={group.meal}>
              <header className="meal-head">
                <h3>
                  <span aria-hidden="true">{MEAL_ICONS[group.meal]}</span> {MEAL_LABELS[group.meal]}
                </h3>
                <span className="meal-kcal">{group.kcal} kcal</span>
              </header>

              {group.entries.length === 0 ? (
                <p className="muted meal-empty">No entries yet.</p>
              ) : (
                <ul>
                  {group.entries.map((entry) => (
                    <li key={entry.id}>
                      <button className="entry" onClick={() => select(entry)}>
                        <span className="entry-name">{entry.name}</span>
                        <span className="muted entry-time">{formatTime(entry.recordedAt)}</span>
                        <span className="entry-kcal">{entry.calories} kcal</span>
                      </button>
                    </li>
                  ))}
                </ul>
              )}

              <button className="link meal-add" onClick={() => addTo(group.meal)}>
                + Add
              </button>
            </section>
          ))}

          {formOpen && (
            <FoodEntryForm
              editing={editing}
              defaultMeal={formMeal}
              date={date}
              onSave={save}
              onDelete={remove}
              onCancel={closeForm}
            />
          )}
        </>
      ) : (
        <p className="muted">Loading…</p>
      )}
    </main>
  );
}
