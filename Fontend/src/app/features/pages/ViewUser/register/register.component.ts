import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthSessionService } from '../../../../core/auth/auth-session.service';
import { AuthPageComponent } from '../../../../core/auth/auth-page.component';
import { UiButtonComponent } from '../../../../shared/ui/button/ui-button.component';
import { UiInputComponent } from '../../../../shared/ui/input/ui-input.component';

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value as string | undefined;
  const confirm = group.get('confirmPassword')?.value as string | undefined;
  if (!password || !confirm) {
    return null;
  }
  return password === confirm ? null : { mismatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AuthPageComponent,
    UiButtonComponent,
    UiInputComponent,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthSessionService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
      email: ['', [Validators.required, Validators.email]],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/),
        ],
      ],
      confirmPassword: ['', [Validators.required]],
      agree: [false, [Validators.requiredTrue]],
    },
    { validators: [passwordsMatch] },
  );

  submit(): void {
    this.error.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { name, email, password } = this.form.getRawValue();
    this.loading.set(true);
    this.auth.register({ name, email, password }).subscribe({
      next: () => {
        this.loading.set(false);
        void this.router.navigateByUrl('/dashboard').then(() => {
          window.location.reload();
        });
      },
      error: (err: { error?: { message?: string; errors?: string[] } }) => {
        this.loading.set(false);
        const msg =
          err?.error?.message ||
          err?.error?.errors?.[0] ||
          (typeof err?.error === 'string' ? err.error : null) ||
          'Registration failed';
        this.error.set(msg);
      },
    });
  }

  confirmPasswordError(): string {
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
