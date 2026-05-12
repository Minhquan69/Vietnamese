import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { GamificationService } from '../../features/services/gamification.service';
import type {
  GamificationLeaderboardRow,
  GamificationState,
} from '../../features/models/gamification.model';

@Component({
  selector: 'app-gamification-arena-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './gamification-arena.page.html',
  styleUrl: './gamification-arena.page.scss',
})
export class GamificationArenaPageComponent implements OnInit {
  private readonly gm = inject(GamificationService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly state = signal<GamificationState | null>(null);
  readonly board = signal<GamificationLeaderboardRow[]>([]);
  readonly xpPulse = signal(false);

  readonly xpPct = computed(() => {
    const p = this.state()?.profile;
    if (!p || p.xpRequiredForNextLevel <= 0) {
      return 0;
    }
    return Math.min(100, Math.round((100 * p.xpIntoCurrentLevel) / p.xpRequiredForNextLevel));
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      me: this.gm.me(),
      lb: this.gm.leaderboard(40),
    }).subscribe({
      next: ({ me, lb }) => {
        this.state.set(me);
        this.board.set(lb);
        this.loading.set(false);
        this.xpPulse.set(true);
        setTimeout(() => this.xpPulse.set(false), 1200);
      },
      error: () => {
        this.error.set('Could not load guild data. Try again shortly.');
        this.loading.set(false);
      },
    });
  }

  iconFromKey(key: string): string {
    const k = (key || '').toLowerCase();
    const map: Record<string, string> = {
      target: '🎯',
      star: '⭐',
      flame: '🔥',
      brick: '🧱',
      book: '📚',
      mic: '🎙️',
      bolt: '⚡',
    };
    return map[k] ?? '✨';
  }

  tierLabel(tier: number): string {
    if (tier >= 3) {
      return 'Gold';
    }

    if (tier === 2) {
      return 'Silver';
    }

    return 'Bronze';
  }
}
