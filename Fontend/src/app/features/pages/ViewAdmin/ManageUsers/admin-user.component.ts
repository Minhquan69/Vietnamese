import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../../services/account.service';

@Component({
  selector: 'app-admin-user',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './admin-user.component.html',
  styleUrls: ['./admin-user.component.css'],
})
export class AdminUserComponent {
  users: any[] = [];

  email: string = '';
  status: number | null = null;
  roleId: number | null = null;

  page = 1;
  pageSize = 10;
  total = 0;
  totalPage = 1;

  constructor(private api: AccountService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.api
      .getUsers(
        this.email,
        this.status!,
        this.roleId!,
        this.page,
        this.pageSize,
      )
      .subscribe((res: any) => {
        
        this.users = res.data;
        this.total = res.total;
        this.totalPage = Math.ceil(this.total / this.pageSize);
      });
  }

  saveUser(user: any) {
    const id = user.id || user.userId; // ← lấy đúng id

    this.api.updateUserStatus(id, user.status).subscribe(() => {
      this.api.updateUserRole(id, user.roleId).subscribe(() => {
        alert('Updated user');
        this.loadUsers();
      });
    });
  }

  nextPage() {
    if (this.page < this.totalPage) {
      this.page++;
      this.loadUsers();
    }
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.loadUsers();
    }
  }

  search() {
    this.page = 1;
    this.loadUsers();
  }
}
