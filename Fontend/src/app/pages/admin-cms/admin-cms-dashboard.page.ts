import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminCmsService } from './admin-cms.service';
import type { AdminAnalyticsSeriesPoint, AdminAnalyticsSummary } from './admin-cms.model';

@Component({
  selector: 'app-admin-cms-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-cms-dashboard.page.html',
  styleUrls: ['./admin-cms-dashboard.page.scss', './admin-cms.shared.scss'],
})
export class AdminCmsDashboardPageComponent implements OnInit {
  private readonly cms = inject(AdminCmsService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly summary = signal<AdminAnalyticsSummary | null>(null);

  readonly quizMax = computed(() => {
    const s = this.summary()?.activityLast14Days ?? [];
    const m = Math.max(0, ...s.map((p) => p.quizCompletions));
    return m < 1 ? 1 : m;
  });

  readonly speakMax = computed(() => {
    const s = this.summary()?.activityLast14Days ?? [];
    const m = Math.max(0, ...s.map((p) => p.speakingSessions));
    return m < 1 ? 1 : m;
  });

  barQuizPct(val: number): number {
    return Math.round((100 * val) / this.quizMax());
  }

  barSpeakPct(val: number): number {
    return Math.round((100 * val) / this.speakMax());
  }

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.error.set(null);
    this.cms.analytics().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load analytics.');
        this.loading.set(false);
      },
    });
  }

  shortDate(p: AdminAnalyticsSeriesPoint): string {
    const d = p.date.slice(5);
    return d;
  }
}
