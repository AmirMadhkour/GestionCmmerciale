import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrderStatus, OrderSummary } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

type StatusFilter = OrderStatus | 'ALL';

interface StatusFilterOption {
  value: StatusFilter;
  label: string;
}

@Component({
  selector: 'app-order-list',
  imports: [CurrencyPipe, DatePipe, RouterLink, PageHeaderComponent, LoadingSpinnerComponent, StatusBadgeComponent],
  templateUrl: './order-list.component.html'
})
export class OrderListComponent implements OnInit {
  private readonly orderService = inject(OrderService);

  readonly orders = signal<OrderSummary[]>([]);
  readonly loading = signal(true);
  readonly search = signal('');
  readonly statusFilter = signal<StatusFilter>('ALL');

  readonly filterOptions: StatusFilterOption[] = [
    { value: 'ALL', label: 'Toutes' },
    { value: OrderStatus.Draft, label: 'Brouillons' },
    { value: OrderStatus.Validated, label: 'Validées' },
    { value: OrderStatus.Cancelled, label: 'Annulées' }
  ];

  readonly counts = computed(() => {
    const orders = this.orders();
    return {
      ALL: orders.length,
      [OrderStatus.Draft]: orders.filter((order) => order.status === OrderStatus.Draft).length,
      [OrderStatus.Validated]: orders.filter((order) => order.status === OrderStatus.Validated).length,
      [OrderStatus.Cancelled]: orders.filter((order) => order.status === OrderStatus.Cancelled).length
    } as Record<StatusFilter, number>;
  });

  readonly filtered = computed(() => {
    const term = this.search().trim().toLowerCase();
    const status = this.statusFilter();
    return this.orders().filter(
      (order) =>
        (status === 'ALL' || order.status === status) &&
        (!term || order.orderNumber.toLowerCase().includes(term) || order.clientName.toLowerCase().includes(term))
    );
  });

  readonly validatedRevenue = computed(() =>
    this.orders()
      .filter((order) => order.status === OrderStatus.Validated)
      .reduce((total, order) => total + order.totalTTC, 0)
  );

  ngOnInit(): void {
    this.orderService.getAll().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  selectStatus(status: StatusFilter): void {
    this.statusFilter.set(status);
  }
}
