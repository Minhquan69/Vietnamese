import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AccountService } from '../../services/account.service';
import { BaseService } from '../../services/base.service';
import { UiButtonComponent } from '../../../shared/ui/button/ui-button.component';
import { UiInputComponent } from '../../../shared/ui/input/ui-input.component';
import { UiCardComponent } from '../../../shared/ui/card/ui-card.component';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    UiButtonComponent,
    UiInputComponent,
    UiCardComponent,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent {
  private readonly api = inject(AccountService);
  private readonly base = inject(BaseService);
  private readonly fb = inject(FormBuilder);

  readonly savingProfile = signal(false);
  readonly savingPassword = signal(false);
  readonly uploading = signal(false);
  readonly avatarUrl = signal<string | null>(null);

  readonly profileForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
  });

  readonly passwordForm = this.fb.nonNullable.group({
    oldPassword: ['', Validators.required],
    newPassword: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/),
      ],
    ],
  });

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.api.getCurrentUser().subscribe({
      next: (res) => {
        this.profileForm.patchValue({ name: res.name, email: res.email });
        this.avatarUrl.set(res.avatarUrl ?? null);
      },
      error: (err) => this.base.handleError(err, 'Could not load profile'),
    });
  }

  avatarSrc(): string | null {
    const rel = this.avatarUrl();
    if (!rel) {
      return null;
    }
    if (rel.startsWith('http')) {
      return rel;
    }
    return `${environment.apiOrigin}${rel}`;
  }

  onAvatarSelected(ev: Event): void {
    const input = ev.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }
    this.uploading.set(true);
    this.api.uploadAvatar(file).subscribe({
      next: (res) => {
        this.uploading.set(false);
        this.avatarUrl.set(res.avatarUrl);
        input.value = '';
      },
      error: (err) => {
        this.uploading.set(false);
        this.base.handleError(err, 'Avatar upload failed');
      },
    });
  }

  updateProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    this.savingProfile.set(true);
    const { name, email } = this.profileForm.getRawValue();
    this.api.updateProfile({ name, email }).subscribe({
      next: () => {
        this.savingProfile.set(false);
        alert('Profile updated');
      },
      error: (err) => {
        this.savingProfile.set(false);
        this.base.handleError(err, 'Update failed');
      },
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    this.savingPassword.set(true);
    const { oldPassword, newPassword } = this.passwordForm.getRawValue();
    this.api.changePassword({ oldPassword, newPassword }).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.passwordForm.reset();
        alert('Password changed');
      },
      error: (err) => {
        this.savingPassword.set(false);
        this.base.handleError(err, 'Change password failed');
      },
    });
  }
}
