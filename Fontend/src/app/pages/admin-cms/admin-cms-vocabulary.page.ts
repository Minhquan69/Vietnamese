import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCmsService } from './admin-cms.service';
import type { AdminVocabularyRow, PagedResult } from './admin-cms.model';

@Component({
  selector: 'app-admin-cms-vocabulary',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-cms-vocabulary.page.html',
  styleUrls: ['./admin-cms-vocabulary.page.scss', './admin-cms.shared.scss'],
})
export class AdminCmsVocabularyPageComponent implements OnInit {
  private readonly cms = inject(AdminCmsService);

  filterQ = '';
  activeOnly = false;
  pageSize = 20;
  page = 1;

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly result = signal<PagedResult<AdminVocabularyRow> | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.cms.vocabulary(this.page, this.pageSize, this.filterQ, this.activeOnly ? true : null).subscribe({
      next: (r) => {
        this.result.set(r);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load vocabulary.');
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
