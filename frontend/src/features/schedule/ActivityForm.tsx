import { useEffect, useState } from 'react';
import type { SubmitEvent } from 'react';
import type { Activity, ActivityColor, SaveActivityRequest } from '../../core/schedule';
import { ACTIVITY_COLORS, COLOR_LABELS, toHm } from '../../core/schedule';

interface Props {
  /** A szerkesztett elfoglaltság, vagy null, ha újat viszünk fel. */
  editing: Activity | null;
  date: string;
  onSave: (request: SaveActivityRequest) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  onCancel: () => void;
}

/** Új elfoglaltság felvitele és meglévő szerkesztése – ugyanaz az űrlap. */
export function ActivityForm({ editing, date, onSave, onDelete, onCancel }: Props) {
  const [title, setTitle] = useState('');
  const [startTime, setStartTime] = useState('09:00');
  const [endTime, setEndTime] = useState('10:00');
  const [color, setColor] = useState<ActivityColor>('Blue');
  const [note, setNote] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  // Szerkesztésre váltáskor (vagy vissza újra) töltsük fel az űrlapot.
  useEffect(() => {
    setError(null);
    if (editing) {
      setTitle(editing.title);
      setStartTime(toHm(editing.startTime));
      setEndTime(toHm(editing.endTime));
      setColor(editing.color);
      setNote(editing.note ?? '');
    } else {
      setTitle('');
      setStartTime('09:00');
      setEndTime('10:00');
      setColor('Blue');
      setNote('');
    }
  }, [editing]);

  async function submit(event: SubmitEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();

    if (!title.trim()) {
      setError('Title cannot be empty.');
      return;
    }
    if (endTime <= startTime) {
      setError('The end must be after the start.');
      return;
    }

    setSaving(true);
    setError(null);
    try {
      await onSave({
        date,
        startTime: `${startTime}:00`,
        endTime: `${endTime}:00`,
        title: title.trim(),
        color,
        note: note.trim() === '' ? null : note.trim()
      });
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
      <h3>{editing ? 'Edit activity' : 'New activity'}</h3>

      <label>
        Title
        <input
          type="text"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="e.g. Team meeting"
          maxLength={120}
          required
          autoFocus
        />
      </label>

      <div className="time-row">
        <label>
          Start
          <input type="time" value={startTime} onChange={(event) => setStartTime(event.target.value)} required />
        </label>
        <label>
          End
          <input type="time" value={endTime} onChange={(event) => setEndTime(event.target.value)} required />
        </label>
      </div>

      <fieldset className="color-picker">
        <legend>Color</legend>
        {ACTIVITY_COLORS.map((option) => (
          <label key={option} className={`swatch act-${option.toLowerCase()}${color === option ? ' is-selected' : ''}`}>
            <input
              type="radio"
              name="color"
              value={option}
              checked={color === option}
              onChange={() => setColor(option)}
            />
            <span className="sr-only">{COLOR_LABELS[option]}</span>
          </label>
        ))}
      </fieldset>

      <label>
        Note
        <textarea
          value={note}
          onChange={(event) => setNote(event.target.value)}
          placeholder="Any detail that helps you remember…"
          maxLength={500}
          rows={3}
        />
      </label>

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
