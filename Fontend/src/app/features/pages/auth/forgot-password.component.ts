import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthSessionService } from '../../../core/auth/auth-session.service';
import { AuthPageComponent } from '../../../core/auth/auth-page.component';
import { UiButtonComponent } from '../../../shared/ui/button/ui-button.component';
import { UiInputComponent } from '../../../shared/ui/input/ui-input.component';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AuthPageComponent,
    UiButtonComponent,
    UiInputComponent,
  ],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthSessionService);

  readonly loading = signal(false);
  readonly done = signal(false);
  readonly devToken = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.auth.forgotPassword(this.form.controls.email.value).subscribe({
      next: (data: unknown) => {
        this.loading.set(false);
        this.done.set(true);
        const d = data as { devResetToken?: string } | null;
        this.devToken.set(d?.devResetToken ?? null);
      },
      error: () => {
        this.loading.set(false);
        this.done.set(true);
      },
    });
  }
}
