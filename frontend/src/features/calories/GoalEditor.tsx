import { useState } from 'react';
import type { SubmitEvent } from 'react';

interface Props {
  current: number;
  onSave: (dailyTargetKcal: number) => Promise<void>;
  onCancel: () => void;
}

/** A napi kalóriakeret módosítása. */
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
    } catch {
      setError('Saving failed.');
      setSaving(false);
    }
  }

  return (
    <form className="activity-form" onSubmit={submit}>
      <h3>Daily calorie target</h3>

      <label>
        Target (kcal)
        <input
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

      {error && <p className="error">{error}</p>}

      <div className="form-actions">
        <button type="submit" disabled={saving}>
          {saving ? 'Saving…' : 'Save'}
        </button>
        <button type="button" className="link" onClick={onCancel} disabled={saving}>
          Cancel
        </button>
      </div>
    </form>
  );
}
