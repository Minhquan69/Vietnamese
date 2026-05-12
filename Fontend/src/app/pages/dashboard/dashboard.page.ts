import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin, of, catchError } from 'rxjs';

import { LearningService } from '../../features/services/learning.service';
import { AccountService } from '../../features/services/account.service';
import { UiButtonComponent } from '../../shared/ui/button/ui-button.component';
import { UiCardComponent } from '../../shared/ui/card/ui-card.component';
import { UiSkeletonComponent } from '../../shared/ui/skeleton/ui-skeleton.component';
import { UiProgressDonutComponent } from '../../shared/ui/progress-donut/ui-progress-donut.component';
import type { DashboardChallengeDto, LearningDashboardDto } from './learning-dashboard.model';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    UiButtonComponent,
    UiCardComponent,
    UiSkeletonComponent,
    UiProgressDonutComponent,
  ],
  templateUrl: './dashboard.page.html',
  styleUrl: './dashboard.page.scss',
})
export class DashboardPageComponent implements OnInit {
  private readonly learning = inject(LearningService);
  private readonly account = inject(AccountService);

  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly dash = signal<LearningDashboardDto | null>(null);
  readonly displayName = signal<string>('');
  readonly ready = signal(false);

  readonly chartMax = computed(() => {
    const d = this.dash();
    if (!d?.activitySeries?.length) {
      return 1;
    }
    return Math.max(1, ...d.activitySeries.map((p) => p.xp));
  });

  ngOnInit(): void {
    this.fetchAll();
  }

  retry(): void {
    this.fetchAll();
  }

  private fetchAll(): void {
    this.loading.set(true);
    this.loadError.set(null);
    forkJoin({
      dash: this.learning.getLearningDashboard(),
      user: this.account.getCurrentUser().pipe(catchError(() => of(null))),
    }).subscribe({
      next: ({ dash, user }) => {
        this.dash.set(dash);
        const raw = user?.name?.trim();
        this.displayName.set(raw ? raw.split(/\s+/)[0]! : '');
        this.loading.set(false);
        this.ready.set(true);
      },
      error: () => {
        this.loadError.set('We could not load your learning stats. Try again in a moment.');
        this.loading.set(false);
      },
    });
  }

  dayLabel(isoDate: string): string {
    const [y, m, d] = isoDate.split('-').map((x) => +x);
    if (!y || !m || !d) {
      return '';
    }
    const dt = new Date(y, m - 1, d);
    return dt.toLocaleDateString(undefined, { weekday: 'short' });
  }

  barHeightPct(xp: number): number {
    const max = this.chartMax();
    return Math.max(6, Math.round((xp / max) * 100));
  }

  challengePct(c: DashboardChallengeDto): number {
    if (c.target <= 0) {
      return 0;
    }
    return Math.min(100, Math.round((c.current / c.target) * 100));
  }

  unlockedCount(d: LearningDashboardDto): number {
    return d.achievements.filter((a) => a.unlocked).length;
  }

  quizParams(unitId: number): Record<string, string | number> {
    return { unitId, refType: 'UNIT' };
  }
}
