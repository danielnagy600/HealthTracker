import { useCallback, useEffect, useState } from 'react';
import type { DayCalories, FoodEntry, Meal, SaveFoodEntryRequest } from '../../core/calories';
import { MEAL_ICONS, MEAL_LABELS, calories, statusClass } from '../../core/calories';
import { addDays, formatLongDate, todayIso } from '../../core/schedule';
import { formatTime } from '../../core/format';
import { FoodEntryForm } from './FoodEntryForm';
import { GoalEditor } from './GoalEditor';

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
          <section className={`flex flex-col gap-1 rounded-[14px] p-[1rem_1.25rem] text-white ${statusClass(day.status)}`}>
            <strong>{day.message}</strong>
            {day.largestMeal && (
              <span className="text-[0.85rem] opacity-90">
                Biggest meal: {MEAL_LABELS[day.largestMeal]}
              </span>
            )}
          </section>

          <section className="surface p-[1.1rem_1.25rem]">
            <div className="flex items-baseline gap-2">
              <span className="text-[1.7rem] font-bold">{day.consumedKcal} kcal</span>
              <span className="text-muted">
                / {day.targetKcal} kcal ·{' '}
                {day.overKcal > 0 ? `${day.overKcal} kcal over` : `${day.remainingKcal} kcal left`}
              </span>
            </div>
            <div className="mt-[0.7rem] mb-[0.4rem] h-3 overflow-hidden rounded-full bg-track">
              <div
                className={`h-full rounded-full transition-[width] duration-300 ease-in-out ${
                  day.overKcal > 0
                    ? 'bg-[linear-gradient(90deg,#ef7a68,var(--color-red))]'
                    : 'bg-[linear-gradient(90deg,var(--color-blue),var(--color-blue-dark))]'
                }`}
                style={{ width: `${Math.min(100, day.percentOfTarget)}%` }}
              ></div>
            </div>
            <div className="flex items-baseline justify-between gap-2">
              <span className="text-muted">{day.percentOfTarget}% of daily target</span>
              <button className="btn-link" onClick={() => { closeForm(); setGoalOpen(true); }}>
                Change target
              </button>
            </div>
          </section>

          {goalOpen && (
            <GoalEditor current={day.targetKcal} onSave={saveGoal} onCancel={() => setGoalOpen(false)} />
          )}

          {day.meals.map((group) => (
            <section className="surface p-[0.9rem_1.25rem]" key={group.meal}>
              <header className="flex items-baseline justify-between gap-2">
                <h3 className="m-0 text-[1.02rem]">
                  <span aria-hidden="true">{MEAL_ICONS[group.meal]}</span> {MEAL_LABELS[group.meal]}
                </h3>
                <span className="font-bold tabular-nums">{group.kcal} kcal</span>
              </header>

              {group.entries.length === 0 ? (
                <p className="mt-2 mb-0 text-[0.88rem] text-muted">No entries yet.</p>
              ) : (
                <ul className="mt-2 mb-0 list-none p-0 [&>li+li]:border-t [&>li+li]:border-border">
                  {group.entries.map((entry) => (
                    <li key={entry.id}>
                      <button
                        className="flex w-full cursor-pointer items-baseline gap-[0.6rem] rounded-lg border-none bg-transparent py-2 text-left text-[0.94rem] font-normal text-ink hover:bg-white/5"
                        onClick={() => select(entry)}
                      >
                        <span className="flex-1">{entry.name}</span>
                        <span className="text-[0.78rem] text-muted tabular-nums">{formatTime(entry.recordedAt)}</span>
                        <span className="font-semibold tabular-nums">{entry.calories} kcal</span>
                      </button>
                    </li>
                  ))}
                </ul>
              )}

              <button className="btn-link mt-[0.4rem] pl-0 text-[0.85rem]" onClick={() => addTo(group.meal)}>
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
        <p className="text-muted">Loading…</p>
      )}
    </main>
  );
}
