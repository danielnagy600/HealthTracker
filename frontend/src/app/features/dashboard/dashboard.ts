import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth';
import { DailySummary, Reminder, WaterService } from '../../core/water';

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, FormsModule],
  template: `
    <header class="topbar">
      <span class="brand">💧 HealthTracker</span>
      <span class="spacer"></span>
      <span class="muted">{{ auth.email() }}</span>
      <button class="link" (click)="logout()">Log out</button>
    </header>

    <main class="dashboard">
      @if (reminder(); as r) {
        <section class="banner" [class]="statusClass(r.status)">
          <strong>{{ r.message }}</strong>
          @if (r.nextReminderAt) {
            <span class="next">Next: {{ r.nextReminderAt | date: 'HH:mm' }} · ~{{ r.nextDoseMl }} ml</span>
          }
        </section>
      }

      @if (summary(); as s) {
        <section class="progress-card">
          <div class="numbers">
            <span class="big">{{ s.consumedMl }} ml</span>
            <span class="muted">/ {{ s.targetMl }} ml · {{ s.remainingMl }} ml left</span>
          </div>
          <div class="bar"><div class="fill" [style.width.%]="percent(s)"></div></div>
          <div class="muted">{{ s.percentComplete }}% of daily goal</div>
        </section>

        <section class="actions">
          <button (click)="add(250)">+250 ml</button>
          <button (click)="add(500)">+500 ml</button>
          <span class="custom">
            <input type="number" min="1" [(ngModel)]="customAmount" />
            <button (click)="add(customAmount)">Add</button>
          </span>
        </section>

        <section class="log">
          <h3>Today</h3>
          @if (s.intakes.length === 0) {
            <p class="muted">No water logged yet. Add your first glass! 🥛</p>
          } @else {
            <ul>
              @for (i of s.intakes; track i.id) {
                <li><span>{{ i.recordedAt | date: 'HH:mm' }}</span><span>{{ i.amountMl }} ml</span></li>
              }
            </ul>
          }
        </section>
      } @else {
        <p class="muted">Loading…</p>
      }
    </main>
  `
})
export class Dashboard implements OnInit {
  private water = inject(WaterService);
  private router = inject(Router);
  auth = inject(AuthService);

  summary = signal<DailySummary | null>(null);
  reminder = signal<Reminder | null>(null);
  customAmount = 250;

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.water.getSummary().subscribe((s) => this.summary.set(s));
    this.water.getReminder().subscribe((r) => this.reminder.set(r));
  }

  add(amountMl: number): void {
    if (!amountMl || amountMl <= 0) {
      return;
    }
    this.water.addIntake(amountMl).subscribe(() => this.refresh());
  }

  percent(s: DailySummary): number {
    return Math.min(100, s.percentComplete);
  }

  statusClass(status: string): string {
    return status === 'Behind' ? 'behind' : status === 'GoalReached' ? 'done' : 'ontrack';
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
