import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCmsService } from './admin-cms.service';
import type { AdminCourseRow, PagedResult } from './admin-cms.model';

@Component({
  selector: 'app-admin-cms-courses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-cms-courses.page.html',
  styleUrls: ['./admin-cms-courses.page.scss', './admin-cms.shared.scss'],
})
export class AdminCmsCoursesPageComponent implements OnInit {
  private readonly cms = inject(AdminCmsService);

  filterLevelId: number | null = null;
  filterQ = '';
  activeOnly = false;
  pageSize = 20;
  page = 1;

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly result = signal<PagedResult<AdminCourseRow> | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const levelId = this.filterLevelId != null && this.filterLevelId > 0 ? this.filterLevelId : null;
    this.cms.courses(this.page, this.pageSize, levelId, this.filterQ, this.activeOnly ? true : null).subscribe({
      next: (r) => {
        this.result.set(r);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load courses.');
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
