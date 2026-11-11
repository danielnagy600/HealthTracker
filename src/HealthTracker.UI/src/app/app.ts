import { Component, inject } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
})
export class App {
  constructor() {
    // <mat-icon>name</mat-icon> uses the "Material Symbols Outlined" font (linked in index.html).
    inject(MatIconRegistry).setDefaultFontSetClass('material-symbols-outlined');
  }
}
