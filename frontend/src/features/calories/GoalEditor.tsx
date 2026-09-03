import { useState } from 'react';
import type { SubmitEvent } from 'react';
import { ApiError } from '../../core/api';

interface Props {
  current: number;
  onSave: (dailyTargetKcal: number) => Promise<void>;
  onCancel: () => void;
}

export function GoalEditor({ current, onSave, onCancel }: Props) {
  const [target, setTarget] = useState(String(current));
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  async function submit(event: SubmitEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();

    const value = Number(target);
    if (!Number.isFinite(value) || value < 500 || value > 10000) {
      setError('The daily target must be between 500 and 10000 kcal.');
      return;
    }

    setSaving(true);
    setError(null);
    try {
      await onSave(Math.round(value));
    } catch (error) {
      console.error(error);
      setError(error instanceof ApiError ? error.message : 'Saving failed.');
      setSaving(false);
    }
  }

  return (
    <form className="surface flex flex-col gap-[0.8rem] p-[1.1rem_1.25rem]" onSubmit={submit}>
      <h3 className="m-0">Daily calorie target</h3>

      <label className="flex flex-col gap-[0.3rem] text-[0.85rem] text-muted">
        Target (kcal)
        <input
          className="field"
          type="number"
          min="500"
          max="10000"
          step="50"
          value={target}
          onChange={(event) => setTarget(event.target.value)}
          required
          autoFocus
        />
      </label>

      {error && <p className="m-0 text-[0.85rem] text-red">{error}</p>}

      <div className="flex items-center gap-[0.6rem]">
        <button className="btn" type="submit" disabled={saving}>
          {saving ? 'Saving…' : 'Save'}
        </button>
        <button className="btn-link" type="button" onClick={onCancel} disabled={saving}>
          Cancel
        </button>
      </div>
    </form>
  );
}
