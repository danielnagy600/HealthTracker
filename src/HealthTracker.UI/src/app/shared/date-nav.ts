import { DatePipe } from '@angular/common';
import { Component, computed, model } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { addDays, fromIsoDate, toIsoDate, todayIso } from '../core/common/dates';

/**
 * Previous / next day switcher with a calendar popup. `model()` makes `date` a two-way
 * binding: the parent writes `<app-date-nav [(date)]="date" />`.
 */
@Component({
  selector: 'app-date-nav',
  imports: [
    DatePipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatTooltipModule,
  ],
  templateUrl: './date-nav.html',
  styleUrl: './date-nav.css',
})
export class DateNav {
  readonly date = model.required<string>();

  protected readonly today = todayIso;
  /** The datepicker works with Date objects, the rest of the app with `yyyy-MM-dd` strings. */
  protected readonly asDate = computed(() => fromIsoDate(this.date()));

  protected shift(days: number): void {
    this.date.update((current) => addDays(current, days));
  }

  protected pick(date: Date | null): void {
    if (date) {
      this.date.set(toIsoDate(date));
    }
  }
}
