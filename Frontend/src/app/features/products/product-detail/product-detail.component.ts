import { CurrencyPipe, PercentPipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { VAT_RATE } from '../../../core/constants/app.constants';
import { Product } from '../../../core/models/product.model';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductService } from '../../../core/services/product.service';
import { applyVat, vatAmount } from '../../../core/utils/price.utils';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { HasRoleDirective } from '../../../shared/directives/has-role.directive';
import { stockBadgeClass, stockLabel } from '../stock-level';

@Component({
  selector: 'app-product-detail',
  imports: [CurrencyPipe, PercentPipe, RouterLink, PageHeaderComponent, LoadingSpinnerComponent, ConfirmDialogComponent, HasRoleDirective],
  templateUrl: './product-detail.component.html'
})
export class ProductDetailComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  readonly vatRate = VAT_RATE;
  readonly product = signal<Product | null>(null);
  readonly loading = signal(true);
  readonly confirmOpen = signal(false);
  readonly deleting = signal(false);

  readonly priceVat = computed(() => vatAmount(this.product()?.unitPriceHT ?? 0));
  readonly priceTtc = computed(() => applyVat(this.product()?.unitPriceHT ?? 0));
  readonly stockClass = computed(() => stockBadgeClass(this.product()?.stockQuantity ?? 0));
  readonly stockText = computed(() => stockLabel(this.product()?.stockQuantity ?? 0));

  ngOnInit(): void {
    this.productService.getById(this.id()).subscribe({
      next: (product) => {
        this.product.set(product);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        void this.router.navigate(['/products']);
      }
    });
  }

  confirmDelete(): void {
    this.deleting.set(true);
    this.productService.delete(this.id()).subscribe({
      next: () => {
        this.notifier.success('Produit supprimé avec succès.');
        void this.router.navigate(['/products']);
      },
      error: () => {
        this.deleting.set(false);
        this.confirmOpen.set(false);
      }
    });
  }
}
