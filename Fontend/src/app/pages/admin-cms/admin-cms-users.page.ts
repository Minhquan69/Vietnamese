import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCmsService } from './admin-cms.service';
import type { AdminUserRow, PagedResult } from './admin-cms.model';

@Component({
  selector: 'app-admin-cms-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-cms-users.page.html',
  styleUrls: ['./admin-cms-users.page.scss', './admin-cms.shared.scss'],
})
export class AdminCmsUsersPageComponent implements OnInit {
  private readonly cms = inject(AdminCmsService);

  filterEmail = '';
  filterStatus: '' | '0' | '1' = '';
  filterRoleId: number | null = null;
  pageSize = 20;
  page = 1;

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly result = signal<PagedResult<AdminUserRow> | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const status =
      this.filterStatus === '' ? null : (Number(this.filterStatus) as 0 | 1);
    const roleId = this.filterRoleId != null && this.filterRoleId > 0 ? this.filterRoleId : null;
    this.cms.users(this.page, this.pageSize, this.filterEmail, status, roleId).subscribe({
      next: (r) => {
        this.result.set(r);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('You may not have access, or the server is unavailable.');
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
