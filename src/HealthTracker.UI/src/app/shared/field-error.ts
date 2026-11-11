import { Component, input } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';

/**
 * Shows the first validation message of a field once it is touched. Inside a mat-form-field
 * use <mat-error> instead; this is for errors outside one (e.g. a server error on the whole form).
 */
@Component({
  selector: 'app-field-error',
  template: `
    @let state = field()();
    @if (state.touched() && state.errors().length > 0) {
      <p class="error-text" role="alert">{{ state.errors()[0]?.message }}</p>
    }
  `,
})
export class FieldError {
  readonly field = input.required<FieldTree<unknown>>();
}
