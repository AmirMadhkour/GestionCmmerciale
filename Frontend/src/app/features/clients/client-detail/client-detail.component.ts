import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Client, clientDisplayName } from '../../../core/models/client.model';
import { OrderStatus, OrderSummary } from '../../../core/models/order.model';
import { ClientService } from '../../../core/services/client.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrderService } from '../../../core/services/order.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { HasRoleDirective } from '../../../shared/directives/has-role.directive';

@Component({
  selector: 'app-client-detail',
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    ConfirmDialogComponent,
    StatusBadgeComponent,
    HasRoleDirective
  ],
  templateUrl: './client-detail.component.html'
})
export class ClientDetailComponent implements OnInit {
  private readonly clientService = inject(ClientService);
  private readonly orderService = inject(OrderService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  readonly client = signal<Client | null>(null);
  readonly orders = signal<OrderSummary[]>([]);
  readonly loading = signal(true);
  readonly confirmOpen = signal(false);
  readonly deleting = signal(false);

  readonly displayName = computed(() => {
    const client = this.client();
    return client ? clientDisplayName(client) : '';
  });

  readonly validatedRevenue = computed(() =>
    this.orders()
      .filter((order) => order.status === OrderStatus.Validated)
      .reduce((total, order) => total + order.totalTTC, 0)
  );

  readonly draftCount = computed(() => this.orders().filter((order) => order.status === OrderStatus.Draft).length);

  ngOnInit(): void {
    forkJoin({
      client: this.clientService.getById(this.id()),
      orders: this.orderService.getByClient(this.id())
    }).subscribe({
      next: ({ client, orders }) => {
        this.client.set(client);
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        void this.router.navigate(['/clients']);
      }
    });
  }

  confirmDelete(): void {
    this.deleting.set(true);
    this.clientService.delete(this.id()).subscribe({
      next: () => {
        this.notifier.success('Client supprimé avec succès.');
        void this.router.navigate(['/clients']);
      },
      error: () => {
        this.deleting.set(false);
        this.confirmOpen.set(false);
      }
    });
  }
}
