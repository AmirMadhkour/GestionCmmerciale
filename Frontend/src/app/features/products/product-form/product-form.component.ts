import { CurrencyPipe, PercentPipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable, map } from 'rxjs';
import { VAT_RATE } from '../../../core/constants/app.constants';
import { ApiError } from '../../../core/models/api-error.model';
import { CreateProductPayload, Product, UpdateProductPayload } from '../../../core/models/product.model';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductService } from '../../../core/services/product.service';
import { applyServerErrors, isControlInvalid } from '../../../core/utils/form.utils';
import { applyVat, vatAmount } from '../../../core/utils/price.utils';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-product-form',
  imports: [CurrencyPipe, PercentPipe, ReactiveFormsModule, RouterLink, PageHeaderComponent, FieldErrorComponent, LoadingSpinnerComponent],
  templateUrl: './product-form.component.html'
})
export class ProductFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);

  readonly id = input<string>();

  readonly vatRate = VAT_RATE;
  readonly isEdit = computed(() => !!this.id());
  readonly heading = computed(() => (this.isEdit() ? 'Modifier le produit' : 'Nouveau produit'));
  readonly backLink = computed(() => (this.isEdit() ? ['/products', this.id()] : ['/products']));
  readonly isInvalid = isControlInvalid;
  readonly loading = signal(false);
  readonly submitting = signal(false);
  readonly currentStock = signal<number | null>(null);

  readonly form = this.fb.nonNullable.group({
    reference: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(1000)]],
    unitPriceHT: [0, [Validators.required, Validators.min(0)]],
    initialStock: [0, [Validators.required, Validators.min(0)]]
  });

  readonly priceHt = toSignal(this.form.controls.unitPriceHT.valueChanges, {
    initialValue: this.form.controls.unitPriceHT.value
  });

  readonly priceVat = computed(() => vatAmount(Number(this.priceHt()) || 0));
  readonly priceTtc = computed(() => applyVat(Number(this.priceHt()) || 0));

  ngOnInit(): void {
    const id = this.id();
    if (!id) {
      return;
    }

    this.form.controls.initialStock.disable();
    this.loading.set(true);
    this.productService.getById(id).subscribe({
      next: (product) => {
        this.fillForm(product);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        void this.router.navigate(['/products']);
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const id = this.id();
    const value = this.form.getRawValue();
    const base: UpdateProductPayload = {
      reference: value.reference.trim(),
      name: value.name.trim(),
      description: value.description.trim(),
      unitPriceHT: Number(value.unitPriceHT)
    };

    const request$: Observable<string> = id
      ? this.productService.update(id, base).pipe(map(() => id))
      : this.productService
          .create({ ...base, initialStock: Number(value.initialStock) } satisfies CreateProductPayload)
          .pipe(map((product) => product.id));

    this.submitting.set(true);
    request$.subscribe({
      next: (productId) => {
        this.notifier.success(id ? 'Produit modifié avec succès.' : 'Produit créé avec succès.');
        void this.router.navigate(['/products', productId]);
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

  private fillForm(product: Product): void {
    this.currentStock.set(product.stockQuantity);
    this.form.patchValue({
      reference: product.reference,
      name: product.name,
      description: product.description,
      unitPriceHT: product.unitPriceHT,
      initialStock: product.stockQuantity
    });
  }
}
