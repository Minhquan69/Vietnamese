import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../services/account.service';
import { BaseService } from '../../services/base.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css'],
})
export class ProfileComponent {
  name: string = '';
  email: string = '';
  role: string = '';

  newName: string = '';
  newEmail: string = '';

  oldPassword: string = '';
  newPassword: string = '';

  constructor(private api: AccountService,
    private baseService: BaseService,
  ) {}

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    this.api.getCurrentUser().subscribe((res: any) => {
      this.name = res.name;
      this.email = res.email;
      this.role = res.role;

      this.newName = this.name;
      this.newEmail = this.email;
    });
  }

  updateProfile() {
    const data = {
      name: this.newName,
      email: this.newEmail,
    };
    
    this.api.updateProfile(data).subscribe({
      next: () => {
        alert('Profile updated');
        this.name = this.newName;
        this.email = this.newEmail;
      },
      error: (err) => this.baseService.handleError(err, 'Update failed'),
    });
  }

  changePassword() {
    const data = {
      oldPassword: this.oldPassword,
      newPassword: this.newPassword,
    };

    this.api.changePassword(data).subscribe({
      next: () => {
        alert('Password changed');
        this.oldPassword = '';
        this.newPassword = '';
      },
      error: (err) =>
        this.baseService.handleError(err, 'Change password failed'),
    });
  }
}
