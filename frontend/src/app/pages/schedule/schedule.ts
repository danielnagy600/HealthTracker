import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { todayIso } from '../../core/common/dates';
import { Activity } from '../../core/modules/schedule/models/activity';
import { ScheduleApi } from '../../core/modules/schedule/schedule-api';
import { DateNav } from '../../shared/date-nav';
import { DurationPipe } from '../../shared/duration-pipe';
import { HmPipe } from '../../shared/hm-pipe';
import { ActivityForm } from './activity-form';
import { DayTimeline } from './day-timeline';

@Component({
  selector: 'app-schedule',
  imports: [
    DateNav,
    DayTimeline,
    ActivityForm,
    DurationPipe,
    HmPipe,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './schedule.html',
  styleUrl: './schedule.css',
})
export default class Schedule {
  private readonly schedule = inject(ScheduleApi);

  protected readonly date = signal(todayIso());
  // Reads `date()` inside, so changing the date re-fetches automatically.
  protected readonly day = this.schedule.day(this.date);

  protected readonly editing = signal<Activity | null>(null);
  protected readonly formOpen = signal(false);

  protected readonly longestFreeSlot = computed(() => {
    const slots = this.day.hasValue() ? this.day.value().freeSlots : [];
    return slots.reduce<(typeof slots)[number] | null>(
      (longest, slot) =>
        longest === null || slot.durationMinutes > longest.durationMinutes ? slot : longest,
      null,
    );
  });

  protected open(activity: Activity | null): void {
    this.editing.set(activity);
    this.formOpen.set(true);
  }

  protected close(): void {
    this.formOpen.set(false);
    this.editing.set(null);
  }

  protected onSaved(): void {
    this.close();
    this.day.reload();
  }
}
