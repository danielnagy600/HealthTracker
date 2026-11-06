import { Component, computed, input, output } from '@angular/core';
import {
  ACTIVITY_COLOR_CSS,
  Activity,
  DaySchedule,
  toHm,
  toMinutes,
} from '../../core/modules/schedule';
import { DurationPipe } from '../../shared/duration-pipe';
import { HmPipe } from '../../shared/hm-pipe';
import { placeActivities } from './lanes';

const HOUR_HEIGHT = 56;

interface Block {
  activity: Activity;
  top: number;
  height: number;
  left: string;
  width: string;
  showNote: boolean;
  color: string;
  selected: boolean;
  tooltip: string;
}

/** The vertical day view. Pure presentation: data comes in, clicks go out. */
@Component({
  selector: 'app-day-timeline',
  imports: [DurationPipe, HmPipe],
  templateUrl: './day-timeline.html',
  styleUrl: './day-timeline.css',
})
export class DayTimeline {
  readonly day = input.required<DaySchedule>();
  readonly selectedId = input<string | null>(null);
  readonly selected = output<Activity>();

  private readonly windowStart = computed(() => toMinutes(this.day().windowStart));
  private readonly windowEnd = computed(() => toMinutes(this.day().windowEnd));

  protected readonly height = computed(
    () => (Math.max(60, this.windowEnd() - this.windowStart()) / 60) * HOUR_HEIGHT,
  );

  protected readonly hourMarks = computed(() => {
    const marks: { label: string; top: number }[] = [];
    for (
      let minute = Math.ceil(this.windowStart() / 60) * 60;
      minute <= this.windowEnd();
      minute += 60
    ) {
      marks.push({
        label: `${String(Math.floor(minute / 60)).padStart(2, '0')}:00`,
        top: this.toPixels(minute),
      });
    }
    return marks;
  });

  protected readonly blocks = computed<Block[]>(() =>
    placeActivities(this.day().activities).map(({ activity, lane, laneCount }) => {
      const height = (activity.durationMinutes / 60) * HOUR_HEIGHT;
      const width = 100 / laneCount;
      return {
        activity,
        top: this.toPixels(toMinutes(activity.startTime)),
        height: Math.max(22, height),
        left: `calc(${lane * width}% + 0.15rem)`,
        width: `calc(${width}% - 0.3rem)`,
        showNote: !!activity.note && height >= 64,
        color: ACTIVITY_COLOR_CSS[activity.color],
        selected: activity.id === this.selectedId(),
        tooltip:
          `${toHm(activity.startTime)}–${toHm(activity.endTime)} · ${activity.title}` +
          (activity.note
            ? `
${activity.note}`
            : ''),
      };
    }),
  );

  private toPixels(minute: number): number {
    return ((minute - this.windowStart()) / 60) * HOUR_HEIGHT;
  }
}
