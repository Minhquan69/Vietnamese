import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCmsService } from './admin-cms.service';
import type { AdminLessonRow, PagedResult } from './admin-cms.model';

@Component({
  selector: 'app-admin-cms-lessons',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-cms-lessons.page.html',
  styleUrls: ['./admin-cms-lessons.page.scss', './admin-cms.shared.scss'],
})
export class AdminCmsLessonsPageComponent implements OnInit {
  private readonly cms = inject(AdminCmsService);

  filterLevelId: number | null = null;
  filterCourseId: number | null = null;
  filterUnitId: number | null = null;
  filterQ = '';
  activeOnly = false;
  pageSize = 20;
  page = 1;

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly result = signal<PagedResult<AdminLessonRow> | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const levelId = this.filterLevelId != null && this.filterLevelId > 0 ? this.filterLevelId : null;
    const courseId = this.filterCourseId != null && this.filterCourseId > 0 ? this.filterCourseId : null;
    const unitId = this.filterUnitId != null && this.filterUnitId > 0 ? this.filterUnitId : null;
    this.cms
      .lessons(this.page, this.pageSize, {
        levelId,
        courseId,
        unitId,
        q: this.filterQ,
        activeOnly: this.activeOnly ? true : null,
      })
      .subscribe({
        next: (r) => {
          this.result.set(r);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load lessons.');
          this.loading.set(false);
        },
      });
  }

  apply(): void {
    this.page = 1;
    this.load();
  }

  goPage(p: number): void {
    this.page = p;
    this.load();
  }
}
