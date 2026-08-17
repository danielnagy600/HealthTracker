import { useEffect, useState } from 'react';
import type { SubmitEvent } from 'react';
import type { FoodEntry, Meal, SaveFoodEntryRequest } from '../../core/calories';
import { MEAL_ICONS, MEAL_LABELS, MEALS } from '../../core/calories';

interface Props {
  /** A szerkesztett bejegyzés, vagy null, ha újat viszünk fel. */
  editing: FoodEntry | null;
  /** Új bejegyzésnél ez az előre kiválasztott étkezés. */
  defaultMeal: Meal;
  date: string;
  onSave: (request: SaveFoodEntryRequest) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  onCancel: () => void;
}

/** Új bejegyzés felvitele és meglévő szerkesztése – ugyanaz az űrlap. */
export function FoodEntryForm({ editing, defaultMeal, date, onSave, onDelete, onCancel }: Props) {
  const [name, setName] = useState('');
  const [kcal, setKcal] = useState('');
  const [meal, setMeal] = useState<Meal>(defaultMeal);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setError(null);
    if (editing) {
      setName(editing.name);
      setKcal(String(editing.calories));
      setMeal(editing.meal);
    } else {
      setName('');
      setKcal('');
      setMeal(defaultMeal);
    }
  }, [editing, defaultMeal]);

  async function submit(event: SubmitEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();

    const calories = Number(kcal);
    if (!name.trim()) {
      setError('The food name cannot be empty.');
      return;
    }
    if (!Number.isFinite(calories) || calories <= 0) {
      setError('Calories must be a positive number.');
      return;
    }

    setSaving(true);
    setError(null);
    try {
      await onSave({ date, meal, name: name.trim(), calories: Math.round(calories) });
    } catch {
      setError('Saving failed. Check the values and try again.');
    } finally {
      setSaving(false);
    }
  }

  async function remove(): Promise<void> {
    if (!editing) return;
    setSaving(true);
    try {
      await onDelete(editing.id);
    } catch {
      setError('Deleting failed.');
      setSaving(false);
    }
  }

  return (
    <form className="activity-form" onSubmit={submit}>
      <h3>{editing ? 'Edit entry' : 'New entry'}</h3>

      <label>
        What did you eat?
        <input
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
          placeholder="e.g. Porridge with raspberries"
          maxLength={120}
          required
          autoFocus
        />
      </label>

      <label>
        Calories (kcal)
        <input
          type="number"
          min="1"
          max="10000"
          value={kcal}
          onChange={(event) => setKcal(event.target.value)}
          placeholder="320"
          required
        />
      </label>

      <fieldset className="meal-picker">
        <legend>Meal</legend>
        <div className="meal-options">
          {MEALS.map((option) => (
            <label key={option} className={`meal-option${meal === option ? ' is-selected' : ''}`}>
              <input
                type="radio"
                name="meal"
                value={option}
                checked={meal === option}
                onChange={() => setMeal(option)}
              />
              <span aria-hidden="true">{MEAL_ICONS[option]}</span>
              {MEAL_LABELS[option]}
            </label>
          ))}
        </div>
      </fieldset>

      {error && <p className="error">{error}</p>}

      <div className="form-actions">
        <button type="submit" disabled={saving}>
          {saving ? 'Saving…' : editing ? 'Save' : 'Add'}
        </button>
        <button type="button" className="link" onClick={onCancel} disabled={saving}>
          Cancel
        </button>
        {editing && (
          <button type="button" className="danger" onClick={() => void remove()} disabled={saving}>
            Delete
          </button>
        )}
      </div>
    </form>
  );
}
