import { Pipe, PipeTransform } from '@angular/core';
import { toHm } from '../core/common/times';

/** `{{ '09:30:00' | hm }}` → `09:30` */
@Pipe({ name: 'hm' })
export class HmPipe implements PipeTransform {
  transform(time: string): string {
    return toHm(time);
  }
}
