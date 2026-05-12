import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { distinctUntilChanged, filter, map } from 'rxjs';
import { LearningPathService } from '../../features/services/learning-path.service';
import type { LessonPlayerDto } from '../../features/models/learning-path.model';
import { UiButtonComponent } from '../../shared/ui/button/ui-button.component';
import { VideoLearningSessionComponent } from './video-learning-session.component';

function extractYoutubeId(url: string | null | undefined): string | null {
  if (!url?.trim()) {
    return null;
  }
  const u = url.trim();
  const watch = u.match(/[?&]v=([a-zA-Z0-9_-]{11})/);
  if (watch?.[1]) {
    return watch[1];
  }
  const short = u.match(/youtu\.be\/([a-zA-Z0-9_-]{11})/);
  if (short?.[1]) {
    return short[1];
  }
  const embed = u.match(/youtube\.com\/embed\/([a-zA-Z0-9_-]{11})/);
  if (embed?.[1]) {
    return embed[1];
  }
  return null;
}

function youtubeEmbedUrl(url: string): string {
  const watch = url.match(/[?&]v=([a-zA-Z0-9_-]{11})/);
  if (watch?.[1]) {
    return `https://www.youtube.com/embed/${watch[1]}`;
  }
  const short = url.match(/youtu\.be\/([a-zA-Z0-9_-]{11})/);
  if (short?.[1]) {
    return `https://www.youtube.com/embed/${short[1]}`;
  }
  return url;
}

@Component({
  selector: 'app-lesson-player',
  standalone: true,
  imports: [CommonModule, RouterLink, UiButtonComponent, VideoLearningSessionComponent],
  templateUrl: './lesson-player.component.html',
  styleUrl: './lesson-player.component.scss',
})
export class LessonPlayerComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly learningPath = inject(LearningPathService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly lesson = signal<LessonPlayerDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly completing = signal(false);

  readonly bullets = computed(() => {
    const raw = this.lesson()?.contentJson;
    if (!raw?.trim()) {
      return [] as string[];
    }
    try {
      const o = JSON.parse(raw) as { bullets?: string[] };
      return Array.isArray(o.bullets) ? o.bullets.filter(Boolean) : [];
    } catch {
      return [] as string[];
    }
  });

  readonly videoEmbedUrl = computed((): SafeResourceUrl | null => {
    const L = this.lesson();
    const url = L?.videoUrl;
    if (!url?.trim()) {
      return null;
    }
    const embed = youtubeEmbedUrl(url.trim());
    return this.sanitizer.bypassSecurityTrustResourceUrl(embed);
  });

  readonly youtubeId = computed(() => extractYoutubeId(this.lesson()?.videoUrl));

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        map((pm) => +(pm.get('lessonId') ?? '0')),
        filter((id) => id > 0),
        distinctUntilChanged(),
      )
      .subscribe((id) => this.fetchLesson(id));
  }

  private fetchLesson(lessonId: number): void {
    this.loading.set(true);
    this.error.set(null);
    this.learningPath.getLesson(lessonId).subscribe({
      next: (data) => {
        this.lesson.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load lesson.');
        this.loading.set(false);
      },
    });
  }

  complete(): void {
    const L = this.lesson();
    if (!L || L.completed) {
      return;
    }

    this.completing.set(true);
    this.learningPath.completeLesson(L.lessonId).subscribe({
      next: (res) => {
        this.completing.set(false);
        this.lesson.update((cur) => (cur ? { ...cur, completed: true } : cur));
        if (res.nextLessonId) {
          void this.router.navigate([
            '/learn/course',
            L.courseId,
            'unit',
            L.unitId,
            'lesson',
            res.nextLessonId,
          ]);
        }
      },
      error: () => {
        this.completing.set(false);
      },
    });
  }

  goLesson(id: number | null): void {
    const L = this.lesson();
    if (!L || !id) {
      return;
    }
    void this.router.navigate([
      '/learn/course',
      L.courseId,
      'unit',
      L.unitId,
      'lesson',
      id,
    ]);
  }

  typeLabel(type: string): string {
    return type.charAt(0).toUpperCase() + type.slice(1);
  }
}
