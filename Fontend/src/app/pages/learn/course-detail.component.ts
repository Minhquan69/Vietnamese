import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LearningPathService } from '../../features/services/learning-path.service';
import type { CourseLearnDetailDto } from '../../features/models/learning-path.model';
import { UiButtonComponent } from '../../shared/ui/button/ui-button.component';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, UiButtonComponent],
  templateUrl: './course-detail.component.html',
  styleUrl: './course-detail.component.scss',
})
export class CourseDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly learningPath = inject(LearningPathService);

  readonly detail = signal<CourseLearnDetailDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly continueLink = computed(() => {
    const d = this.detail();
    if (!d?.continueLessonId || !d.continueUnitId) {
      return null;
    }
    return ['/learn/course', d.courseId, 'unit', d.continueUnitId, 'lesson', d.continueLessonId];
  });

  ngOnInit(): void {
    const courseId = +(this.route.snapshot.paramMap.get('courseId') ?? '0');
    if (!courseId) {
      this.error.set('Invalid course.');
      this.loading.set(false);
      return;
    }

    this.learningPath.getCourseDetail(courseId).subscribe({
      next: (data) => {
        this.detail.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Sign in to view course detail and lesson progress.');
        this.loading.set(false);
      },
    });
  }
}
