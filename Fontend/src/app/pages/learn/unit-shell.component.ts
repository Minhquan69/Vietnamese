import {
  Component,
  OnDestroy,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ActivatedRoute,
  NavigationEnd,
  Router,
  RouterLink,
  RouterOutlet,
} from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { LearningPathService } from '../../features/services/learning-path.service';
import type { UnitOutlineDto } from '../../features/models/learning-path.model';

@Component({
  selector: 'app-unit-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterOutlet],
  templateUrl: './unit-shell.component.html',
  styleUrl: './unit-shell.component.scss',
})
export class UnitShellComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly learningPath = inject(LearningPathService);

  readonly outline = signal<UnitOutlineDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  private sub?: Subscription;

  ngOnInit(): void {
    const unitId = +(this.route.snapshot.paramMap.get('unitId') ?? '0');
    if (!unitId) {
      this.error.set('Invalid unit.');
      this.loading.set(false);
      return;
    }

    this.loadOutline(unitId);

    this.sub = this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe(() => this.maybeRedirectToLesson(unitId));
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private loadOutline(unitId: number): void {
    this.learningPath.getUnitOutline(unitId).subscribe({
      next: (data) => {
        this.outline.set(data);
        this.loading.set(false);
        this.maybeRedirectToLesson(unitId);
      },
      error: () => {
        this.error.set('Could not load unit.');
        this.loading.set(false);
      },
    });
  }

  private maybeRedirectToLesson(unitId: number): void {
    const data = this.outline();
    if (!data?.lessons?.length) {
      return;
    }

    const child = this.route.firstChild;
    const hasLesson = !!child?.snapshot.params['lessonId'];
    if (hasLesson) {
      return;
    }

    const sorted = [...data.lessons].sort((a, b) => a.orderIndex - b.orderIndex);
    const target = data.continueLessonId ?? sorted[0]?.lessonId;
    if (!target) {
      return;
    }

    void this.router.navigate(['lesson', target], { relativeTo: this.route });
  }

  lessonIcon(type: string): string {
    const t = type.toLowerCase();
    const map: Record<string, string> = {
      vocabulary: '📚',
      grammar: '📝',
      listening: '🎧',
      speaking: '🎤',
      reading: '📖',
      video: '▶',
    };
    return map[t] ?? '◆';
  }
}
