import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable, map } from 'rxjs';
import { ApiError } from '../../../core/models/api-error.model';
import { Client, ClientPayload } from '../../../core/models/client.model';
import { ClientService } from '../../../core/services/client.service';
import { NotificationService } from '../../../core/services/notification.service';
import { applyServerErrors, isControlInvalid } from '../../../core/utils/form.utils';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

const PHONE_PATTERN = /^\+?[0-9\s]{8,15}$/;

@Component({
  selector: 'app-client-form',
  imports: [ReactiveFormsModule, RouterLink, PageHeaderComponent, FieldErrorComponent, LoadingSpinnerComponent],
  templateUrl: './client-form.component.html'
})
export class ClientFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly clientService = inject(ClientService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);

  readonly id = input<string>();

  readonly isEdit = computed(() => !!this.id());
  readonly heading = computed(() => (this.isEdit() ? 'Modifier le client' : 'Nouveau client'));
  readonly backLink = computed(() => (this.isEdit() ? ['/clients', this.id()] : ['/clients']));
  readonly isInvalid = isControlInvalid;
  readonly loading = signal(false);
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    lastName: ['', [Validators.maxLength(100)]],
    firstNameOrCompany: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    phone: ['', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
    address: this.fb.nonNullable.array<string>([])
  });

  get addressLines(): FormArray<FormControl<string>> {
    return this.form.controls.address;
  }

  ngOnInit(): void {
    const id = this.id();
    if (!id) {
      this.addAddressLine();
      return;
    }

    this.loading.set(true);
    this.clientService.getById(id).subscribe({
      next: (client) => {
        this.fillForm(client);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        void this.router.navigate(['/clients']);
      }
    });
  }

  addAddressLine(value = ''): void {
    this.addressLines.push(this.fb.nonNullable.control(value, [Validators.maxLength(200)]));
  }

  removeAddressLine(index: number): void {
    this.addressLines.removeAt(index);
    if (this.addressLines.length === 0) {
      this.addAddressLine();
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const id = this.id();
    const payload = this.buildPayload();
    const request$: Observable<string> = id
      ? this.clientService.update(id, payload).pipe(map(() => id))
      : this.clientService.create(payload).pipe(map((client) => client.id));

    this.submitting.set(true);
    request$.subscribe({
      next: (clientId) => {
        this.notifier.success(id ? 'Client modifié avec succès.' : 'Client créé avec succès.');
        void this.router.navigate(['/clients', clientId]);
      },
      error: (error: ApiError) => {
        this.submitting.set(false);
        const unmatched = applyServerErrors(this.form, error);
        if (unmatched.length > 0) {
          this.notifier.error(unmatched.join(' '));
        }
      }
    });
  }

  private fillForm(client: Client): void {
    this.form.patchValue({
      lastName: client.lastName,
      firstNameOrCompany: client.firstNameOrCompany,
      email: client.email,
      phone: client.phone
    });
    this.addressLines.clear();
    client.address.forEach((line) => this.addAddressLine(line));
    if (this.addressLines.length === 0) {
      this.addAddressLine();
    }
  }

  private buildPayload(): ClientPayload {
    const value = this.form.getRawValue();
    return {
      lastName: value.lastName.trim(),
      firstNameOrCompany: value.firstNameOrCompany.trim(),
      email: value.email.trim(),
      phone: value.phone.trim(),
      address: value.address.map((line) => line.trim()).filter((line) => line.length > 0)
    };
  }
}
