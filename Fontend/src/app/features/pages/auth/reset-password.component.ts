import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthSessionService } from '../../../core/auth/auth-session.service';
import { AuthPageComponent } from '../../../core/auth/auth-page.component';
import { UiButtonComponent } from '../../../shared/ui/button/ui-button.component';
import { UiInputComponent } from '../../../shared/ui/input/ui-input.component';

function matchNewPasswords(c: AbstractControl): ValidationErrors | null {
  const p = c.get('newPassword')?.value as string | undefined;
  const cp = c.get('confirmPassword')?.value as string | undefined;
  if (!p || !cp) {
    return null;
  }
  return p === cp ? null : { mismatch: true };
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AuthPageComponent,
    UiButtonComponent,
    UiInputComponent,
  ],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthSessionService);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly done = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      token: ['', [Validators.required, Validators.minLength(20)]],
      newPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/),
        ],
      ],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: [matchNewPasswords] },
  );

  constructor() {
    this.route.queryParamMap.subscribe((q) => {
      const email = q.get('email') ?? '';
      const token = q.get('token') ?? '';
      this.form.patchValue({ email, token });
    });
  }

  submit(): void {
    this.error.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, token, newPassword } = this.form.getRawValue();
    this.loading.set(true);
    this.auth.resetPassword({ email, token, newPassword }).subscribe({
      next: () => {
        this.loading.set(false);
        this.done.set(true);
      },
      error: (err: { error?: { message?: string } }) => {
        this.loading.set(false);
        this.error.set(err?.error?.message || 'Reset failed');
      },
    });
  }

  confirmError(): string {
    const c = this.form.controls.confirmPassword;
    if (!c.touched) {
      return '';
    }
    if (this.form.hasError('mismatch')) {
      return 'Passwords do not match';
    }
    if (c.hasError('required')) {
      return 'Confirm your password';
    }
    return '';
  }
}
