import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCmsService } from './admin-cms.service';
import type { AdminQuizRow, PagedResult } from './admin-cms.model';

@Component({
  selector: 'app-admin-cms-quizzes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-cms-quizzes.page.html',
  styleUrls: ['./admin-cms-quizzes.page.scss', './admin-cms.shared.scss'],
})
export class AdminCmsQuizzesPageComponent implements OnInit {
  private readonly cms = inject(AdminCmsService);

  filterQ = '';
  filterRefType = '';
  activeOnly = false;
  pageSize = 20;
  page = 1;

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly result = signal<PagedResult<AdminQuizRow> | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const refType = this.filterRefType.trim() || undefined;
    this.cms
      .quizzes(this.page, this.pageSize, this.filterQ, refType, this.activeOnly ? true : null)
      .subscribe({
        next: (r) => {
          this.result.set(r);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load quizzes.');
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
