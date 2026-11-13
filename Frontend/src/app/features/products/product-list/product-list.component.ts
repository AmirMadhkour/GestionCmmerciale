import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LOW_STOCK_THRESHOLD } from '../../../core/constants/app.constants';
import { Product } from '../../../core/models/product.model';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductService } from '../../../core/services/product.service';
import { applyVat } from '../../../core/utils/price.utils';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { HasRoleDirective } from '../../../shared/directives/has-role.directive';
import { stockBadgeClass, stockLabel } from '../stock-level';

@Component({
  selector: 'app-product-list',
  imports: [CurrencyPipe, RouterLink, PageHeaderComponent, LoadingSpinnerComponent, ConfirmDialogComponent, HasRoleDirective],
  templateUrl: './product-list.component.html'
})
export class ProductListComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly notifier = inject(NotificationService);

  readonly priceTtc = applyVat;
  readonly stockBadgeClass = stockBadgeClass;
  readonly stockLabel = stockLabel;
  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly search = signal('');
  readonly pendingDelete = signal<Product | null>(null);
  readonly deleting = signal(false);

  readonly filtered = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) {
      return this.products();
    }
    return this.products().filter((product) =>
      [product.reference, product.name, product.description].some((value) => value.toLowerCase().includes(term))
    );
  });

  readonly lowStockCount = computed(
    () => this.products().filter((product) => product.stockQuantity <= LOW_STOCK_THRESHOLD).length
  );

  readonly deleteMessage = computed(() => {
    const product = this.pendingDelete();
    return product ? `Voulez-vous vraiment supprimer le produit « ${product.name} » (${product.reference}) ?` : '';
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.productService.getAll().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  askDelete(product: Product): void {
    this.pendingDelete.set(product);
  }

  cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  confirmDelete(): void {
    const product = this.pendingDelete();
    if (!product) {
      return;
    }
    this.deleting.set(true);
    this.productService.delete(product.id).subscribe({
      next: () => {
        this.products.update((products) => products.filter((item) => item.id !== product.id));
        this.notifier.success('Produit supprimé avec succès.');
        this.deleting.set(false);
        this.pendingDelete.set(null);
      },
      error: () => {
        this.deleting.set(false);
        this.pendingDelete.set(null);
      }
    });
  }
}
