import { Directive, ElementRef, afterNextRender, inject } from '@angular/core';

/**
 * Focuses the element once it is rendered. (The plain `autofocus` attribute only
 * works on page load, not for elements that appear later, e.g. an opened form.)
 */
@Directive({ selector: '[appAutofocus]' })
export class Autofocus {
  constructor() {
    const element = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    afterNextRender(() => element.focus());
  }
}
