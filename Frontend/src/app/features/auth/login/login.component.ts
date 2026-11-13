import { Component, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { APP_NAME, DEFAULT_REDIRECT_URL } from '../../../core/constants/app.constants';
import { ApiError } from '../../../core/models/api-error.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { applyServerErrors, isControlInvalid } from '../../../core/utils/form.utils';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, FieldErrorComponent],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifier = inject(NotificationService);

  readonly returnUrl = input<string>();

  readonly appName = APP_NAME;
  readonly isInvalid = isControlInvalid;
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  togglePassword(): void {
    this.showPassword.update((visible) => !visible);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: (user) => {
        this.notifier.success(`Bienvenue ${user.fullName || user.email} !`);
        void this.router.navigateByUrl(this.safeReturnUrl());
      },
      error: (error: ApiError) => {
        this.submitting.set(false);
        const unmatched = applyServerErrors(this.form, error);
        if (error.status === 401) {
          this.errorMessage.set(error.detail ?? 'Email ou mot de passe incorrect.');
        } else if (unmatched.length > 0) {
          this.errorMessage.set(unmatched.join(' '));
        } else if (!error.errors) {
          this.errorMessage.set(error.status === 0 ? 'Le serveur est injoignable.' : (error.detail ?? error.title));
        }
      }
    });
  }

  private safeReturnUrl(): string {
    const url = this.returnUrl();
    return url && url.startsWith('/') && !url.startsWith('//') && !url.startsWith('/login') ? url : DEFAULT_REDIRECT_URL;
  }
}
