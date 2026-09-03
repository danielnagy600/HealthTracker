import { useEffect, useState } from 'react';
import type { SubmitEvent } from 'react';
import { ApiError } from '../../core/api';
import type { Activity, ActivityColor, SaveActivityRequest } from '../../core/schedule';
import { ACTIVITY_COLOR_BG, ACTIVITY_COLORS, COLOR_LABELS, toHm } from '../../core/schedule';

interface Props {
  editing: Activity | null;
  date: string;
  onSave: (request: SaveActivityRequest) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  onCancel: () => void;
}

export function ActivityForm({ editing, date, onSave, onDelete, onCancel }: Props) {
  const [title, setTitle] = useState('');
  const [startTime, setStartTime] = useState('09:00');
  const [endTime, setEndTime] = useState('10:00');
  const [color, setColor] = useState<ActivityColor>('Blue');
  const [note, setNote] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

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
    } catch (error) {
      console.error(error);
      setError(error instanceof ApiError ? error.message : 'Saving failed. Check the values and try again.');
    } finally {
      setSaving(false);
    }
  }

  async function remove(): Promise<void> {
    if (!editing) return;
    setSaving(true);
    try {
      await onDelete(editing.id);
    } catch (error) {
      console.error(error);
      setError(error instanceof ApiError ? error.message : 'Deleting failed.');
      setSaving(false);
    }
  }

  return (
    <form className="surface flex flex-col gap-[0.8rem] p-[1.1rem_1.25rem]" onSubmit={submit}>
      <h3 className="m-0">{editing ? 'Edit activity' : 'New activity'}</h3>

      <label className="flex flex-col gap-[0.3rem] text-[0.85rem] text-muted">
        Title
        <input
          className="field"
          type="text"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="e.g. Team meeting"
          maxLength={120}
          required
          autoFocus
        />
      </label>

      <div className="flex gap-[0.8rem]">
        <label className="flex flex-1 flex-col gap-[0.3rem] text-[0.85rem] text-muted">
          Start
          <input
            className="field"
            type="time"
            value={startTime}
            onChange={(event) => setStartTime(event.target.value)}
            required
          />
        </label>
        <label className="flex flex-1 flex-col gap-[0.3rem] text-[0.85rem] text-muted">
          End
          <input
            className="field"
            type="time"
            value={endTime}
            onChange={(event) => setEndTime(event.target.value)}
            required
          />
        </label>
      </div>

      <fieldset className="m-0 flex items-center gap-2 border-none p-0">
        <legend className="mb-[0.35rem] p-0 text-[0.85rem] text-muted">Color</legend>
        {ACTIVITY_COLORS.map((option) => (
          <label
            key={option}
            className={`h-[30px] w-[30px] cursor-pointer rounded-full border-2 border-transparent [box-shadow:0_0_0_1px_var(--color-border)] ${ACTIVITY_COLOR_BG[option]}${
              color === option ? ' border-card [box-shadow:0_0_0_3px_var(--color-ink)]' : ''
            }`}
          >
            <input
              type="radio"
              name="color"
              value={option}
              checked={color === option}
              onChange={() => setColor(option)}
              className="sr-only"
            />
            <span className="sr-only">{COLOR_LABELS[option]}</span>
          </label>
        ))}
      </fieldset>

      <label className="flex flex-col gap-[0.3rem] text-[0.85rem] text-muted">
        Note
        <textarea
          className="field resize-y"
          value={note}
          onChange={(event) => setNote(event.target.value)}
          placeholder="Any detail that helps you remember…"
          maxLength={500}
          rows={3}
        />
      </label>

      {error && <p className="m-0 text-[0.85rem] text-red">{error}</p>}

      <div className="flex items-center gap-[0.6rem]">
        <button className="btn" type="submit" disabled={saving}>
          {saving ? 'Saving…' : editing ? 'Save' : 'Add'}
        </button>
        <button className="btn-link" type="button" onClick={onCancel} disabled={saving}>
          Cancel
        </button>
        {editing && (
          <button className="btn btn-danger" type="button" onClick={() => void remove()} disabled={saving}>
            Delete
          </button>
        )}
      </div>
    </form>
  );
}
