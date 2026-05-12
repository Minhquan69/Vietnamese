import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LearningPathService } from '../../features/services/learning-path.service';
import type { CourseCatalogItemDto } from '../../features/models/learning-path.model';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './course-list.component.html',
  styleUrl: './course-list.component.scss',
})
export class CourseListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly learningPath = inject(LearningPathService);

  readonly courses = signal<CourseCatalogItemDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly levelId = signal<number>(0);

  ngOnInit(): void {
    const id = +(this.route.snapshot.paramMap.get('levelId') ?? '0');
    this.levelId.set(id);
    if (!id) {
      this.error.set('Invalid level.');
      this.loading.set(false);
      return;
    }

    this.learningPath.getCatalog(id).subscribe({
      next: (list) => {
        this.courses.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load courses.');
        this.loading.set(false);
      },
    });
  }

}
