import { CurrencyPipe, PercentPipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable, forkJoin, map, of } from 'rxjs';
import { VAT_RATE } from '../../../core/constants/app.constants';
import { ApiError } from '../../../core/models/api-error.model';
import { Client, clientDisplayName } from '../../../core/models/client.model';
import { OrderDetail, OrderPayload, OrderStatus } from '../../../core/models/order.model';
import { Product } from '../../../core/models/product.model';
import { ClientService } from '../../../core/services/client.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrderService } from '../../../core/services/order.service';
import { ProductService } from '../../../core/services/product.service';
import { applyServerErrors, isControlInvalid } from '../../../core/utils/form.utils';
import { roundAmount } from '../../../core/utils/price.utils';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

type OrderLineForm = FormGroup<{
  productId: FormControl<string>;
  quantity: FormControl<number>;
}>;

interface LineSummary {
  product: Product | undefined;
  unitPrice: number;
  lineTotal: number;
  insufficientStock: boolean;
}

@Component({
  selector: 'app-order-form',
  imports: [CurrencyPipe, PercentPipe, ReactiveFormsModule, RouterLink, PageHeaderComponent, FieldErrorComponent, LoadingSpinnerComponent],
  templateUrl: './order-form.component.html'
})
export class OrderFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly orderService = inject(OrderService);
  private readonly clientService = inject(ClientService);
  private readonly productService = inject(ProductService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);

  readonly id = input<string>();
  readonly clientId = input<string>();

  readonly vatRate = VAT_RATE;
  readonly displayName = clientDisplayName;
  readonly isInvalid = isControlInvalid;
  readonly isEdit = computed(() => !!this.id());
  readonly backLink = computed(() => (this.isEdit() ? ['/orders', this.id()] : ['/orders']));
  readonly clients = signal<Client[]>([]);
  readonly products = signal<Product[]>([]);
  readonly orderNumber = signal<string | null>(null);
  readonly loading = signal(true);
  readonly submitting = signal(false);

  readonly heading = computed(() => {
    const number = this.orderNumber();
    return this.isEdit() ? `Modifier la commande${number ? ' ' + number : ''}` : 'Nouvelle commande';
  });

  readonly form = this.fb.nonNullable.group({
    clientId: ['', [Validators.required]],
    lines: this.fb.array<OrderLineForm>([], { validators: [Validators.required] })
  });

  private readonly formValue = toSignal(this.form.valueChanges.pipe(map(() => this.form.getRawValue())), {
    initialValue: this.form.getRawValue()
  });

  private readonly productMap = computed(() => new Map(this.products().map((product) => [product.id, product])));

  readonly lineSummaries = computed<LineSummary[]>(() =>
    this.formValue().lines.map((line) => {
      const product = this.productMap().get(line.productId);
      const quantity = Number(line.quantity) || 0;
      const unitPrice = product?.unitPriceHT ?? 0;
      return {
        product,
        unitPrice,
        lineTotal: roundAmount(unitPrice * quantity),
        insufficientStock: !!product && quantity > product.stockQuantity
      };
    })
  );

  readonly totalHt = computed(() => roundAmount(this.lineSummaries().reduce((total, line) => total + line.lineTotal, 0)));
  readonly totalVat = computed(() => roundAmount(this.totalHt() * VAT_RATE));
  readonly totalTtc = computed(() => roundAmount(this.totalHt() + this.totalVat()));
  readonly hasStockWarning = computed(() => this.lineSummaries().some((line) => line.insufficientStock));

  get lines(): FormArray<OrderLineForm> {
    return this.form.controls.lines;
  }

  ngOnInit(): void {
    const id = this.id();
    forkJoin({
      clients: this.clientService.getAll(),
      products: this.productService.getAll(),
      order: id ? this.orderService.getById(id) : of(null)
    }).subscribe({
      next: ({ clients, products, order }) => {
        this.clients.set(clients);
        this.products.set(products);
        if (order) {
          this.loadOrder(order);
        } else {
          this.prepareNewOrder(clients);
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        void this.router.navigate(['/orders']);
      }
    });
  }

  addLine(productId = '', quantity = 1): void {
    this.lines.push(
      this.fb.nonNullable.group({
        productId: [productId, [Validators.required]],
        quantity: [quantity, [Validators.required, Validators.min(1)]]
      })
    );
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
    this.lines.markAsTouched();
  }

  isProductTaken(productId: string, index: number): boolean {
    return this.formValue().lines.some((line, lineIndex) => lineIndex !== index && line.productId === productId);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const id = this.id();
    const payload = this.buildPayload();
    const request$: Observable<string> = id
      ? this.orderService.update(id, payload).pipe(map(() => id))
      : this.orderService.create(payload).pipe(map((order) => order.id));

    this.submitting.set(true);
    request$.subscribe({
      next: (orderId) => {
        this.notifier.success(id ? 'Commande modifiée avec succès.' : 'Commande créée avec succès.');
        void this.router.navigate(['/orders', orderId]);
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

  private loadOrder(order: OrderDetail): void {
    if (order.orderStatus !== OrderStatus.Draft) {
      this.notifier.warning('Seule une commande en brouillon peut être modifiée.');
      void this.router.navigate(['/orders', order.id]);
      return;
    }
    this.orderNumber.set(order.orderNumber);
    this.form.controls.clientId.setValue(order.clientId);
    this.form.controls.clientId.disable();
    order.lines.forEach((line) => this.addLine(line.productId, line.quantity));
  }

  private prepareNewOrder(clients: Client[]): void {
    const preselected = this.clientId();
    if (preselected && clients.some((client) => client.id === preselected)) {
      this.form.controls.clientId.setValue(preselected);
    }
    this.addLine();
  }

  private buildPayload(): OrderPayload {
    const value = this.form.getRawValue();
    return {
      clientId: value.clientId,
      lines: value.lines.map((line) => ({ productId: line.productId, quantity: Number(line.quantity) }))
    };
  }
}
