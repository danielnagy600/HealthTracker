import { Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import { FormField, FormRoot, form, maxLength, validate } from '@angular/forms/signals';
import { TextFieldModule } from '@angular/cdk/text-field';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';
import { errorMessage } from '../../core/http/error-message';
import {
  ACTIVITY_COLOR_CSS,
  ACTIVITY_COLORS,
} from '../../core/modules/schedule/models/activity-color';
import { Activity } from '../../core/modules/schedule/models/activity';
import { ScheduleApi } from '../../core/modules/schedule/schedule-api';
import { toHm } from '../../core/common/times';
import { ActivityFormModel } from './models/activity-form-model';
import { Autofocus } from '../../shared/autofocus';
import { FieldError } from '../../shared/field-error';

/** Add / edit / delete one activity. Talks to the API itself and reports back via outputs. */
@Component({
  selector: 'app-activity-form',
  imports: [
    FormField,
    FormRoot,
    FieldError,
    Autofocus,
    TextFieldModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './activity-form.html',
  styleUrl: './activity-form.css',
})
export class ActivityForm {
  private readonly schedule = inject(ScheduleApi);

  readonly editing = input<Activity | null>(null);
  readonly date = input.required<string>();

  readonly saved = output();
  readonly cancelled = output();

  protected readonly colors = ACTIVITY_COLORS;
  protected readonly colorCss = ACTIVITY_COLOR_CSS;

  // linkedSignal: a writable signal that resets itself whenever `editing` changes.
  private readonly model = linkedSignal<ActivityFormModel>(() => {
    const activity = this.editing();
    return activity
      ? {
          title: activity.title,
          startTime: toHm(activity.startTime),
          endTime: toHm(activity.endTime),
          color: activity.color,
          note: activity.note ?? '',
        }
      : { title: '', startTime: '09:00', endTime: '10:00', color: 'Blue', note: '' };
  });

  protected readonly activityForm = form(
    this.model,
    (path) => {
      validate(path.title, ({ value }) =>
        value().trim() ? undefined : { kind: 'required', message: 'Title cannot be empty.' },
      );
      maxLength(path.title, 120);
      maxLength(path.note, 500);
      validate(path.endTime, ({ value, valueOf }) =>
        value() > valueOf(path.startTime)
          ? undefined
          : { kind: 'order', message: 'The end must be after the start.' },
      );
    },
    {
      submission: {
        action: async (fields) => {
          const value = fields().value();
          const request = {
            date: this.date(),
            startTime: `${value.startTime}:00`,
            endTime: `${value.endTime}:00`,
            title: value.title.trim(),
            color: value.color,
            note: value.note.trim() === '' ? null : value.note.trim(),
          };
          const editing = this.editing();
          try {
            await firstValueFrom(
              editing ? this.schedule.update(editing.id, request) : this.schedule.add(request),
            );
          } catch (error) {
            console.error(error);
            // A returned error is attached to the form (shown by <app-field-error [field]="activityForm" />).
            return {
              kind: 'server',
              message: errorMessage(error, 'Saving failed. Check the values and try again.'),
            };
          }
          this.saved.emit();
          return undefined;
        },
      },
    },
  );

  private readonly deleting = signal(false);
  protected readonly deleteError = signal<string | null>(null);
  protected readonly busy = computed(() => this.activityForm().submitting() || this.deleting());

  protected async remove(): Promise<void> {
    const editing = this.editing();
    if (!editing) return;

    this.deleting.set(true);
    this.deleteError.set(null);
    try {
      await firstValueFrom(this.schedule.remove(editing.id));
      this.saved.emit();
    } catch (error) {
      console.error(error);
      this.deleteError.set(errorMessage(error, 'Deleting failed.'));
    } finally {
      this.deleting.set(false);
    }
  }
}
