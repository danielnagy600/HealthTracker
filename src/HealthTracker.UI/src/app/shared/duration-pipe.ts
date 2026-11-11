import { Pipe, PipeTransform } from '@angular/core';

/** `{{ 95 | duration }}` → `1h 35m` */
@Pipe({ name: 'duration' })
export class DurationPipe implements PipeTransform {
  transform(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const rest = minutes % 60;
    if (hours === 0) return `${rest}m`;
    if (rest === 0) return `${hours}h`;
    return `${hours}h ${rest}m`;
  }
}
