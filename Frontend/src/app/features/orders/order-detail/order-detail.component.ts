import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { VAT_RATE } from '../../../core/constants/app.constants';
import { ActionMessage, OrderDetail, OrderStatus } from '../../../core/models/order.model';
import { NotificationService } from '../../../core/services/notification.service';
import { OrderService } from '../../../core/services/order.service';
import { roundAmount } from '../../../core/utils/price.utils';
import { ConfirmDialogComponent, ConfirmDialogVariant } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { HasRoleDirective } from '../../../shared/directives/has-role.directive';

type OrderAction = 'validate' | 'cancel' | 'delete';

interface DialogConfig {
  heading: string;
  message: string;
  confirmLabel: string;
  variant: ConfirmDialogVariant;
}

@Component({
  selector: 'app-order-detail',
  imports: [
    CurrencyPipe,
    DatePipe,
    PercentPipe,
    RouterLink,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    ConfirmDialogComponent,
    StatusBadgeComponent,
    HasRoleDirective
  ],
  templateUrl: './order-detail.component.html'
})
export class OrderDetailComponent implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  readonly vatRate = VAT_RATE;
  readonly order = signal<OrderDetail | null>(null);
  readonly loading = signal(true);
  readonly pendingAction = signal<OrderAction | null>(null);
  readonly busy = signal(false);

  readonly status = computed(() => this.order()?.orderStatus ?? null);
  readonly isDraft = computed(() => this.status() === OrderStatus.Draft);
  readonly isCancelled = computed(() => this.status() === OrderStatus.Cancelled);
  readonly canCancel = computed(() => this.status() !== null && this.status() !== OrderStatus.Cancelled);
  readonly canDelete = computed(() => this.status() !== null && this.status() !== OrderStatus.Validated);
  readonly totalVat = computed(() => {
    const order = this.order();
    return order ? roundAmount(order.totalTTC - order.totalHt) : 0;
  });
  readonly itemCount = computed(() => this.order()?.lines.reduce((total, line) => total + line.quantity, 0) ?? 0);

  readonly dialog = computed<DialogConfig | null>(() => {
    const order = this.order();
    switch (this.pendingAction()) {
      case 'validate':
        return {
          heading: 'Valider la commande',
          message: 'La validation décrémente le stock des produits commandés. La commande ne pourra plus être modifiée.',
          confirmLabel: 'Valider',
          variant: 'primary'
        };
      case 'cancel':
        return {
          heading: 'Annuler la commande',
          message:
            order?.orderStatus === OrderStatus.Validated
              ? 'La commande sera annulée et les quantités seront restituées au stock.'
              : 'La commande en brouillon sera annulée.',
          confirmLabel: 'Annuler la commande',
          variant: 'warning'
        };
      case 'delete':
        return {
          heading: 'Supprimer la commande',
          message: `La commande ${order?.orderNumber ?? ''} sera définitivement supprimée.`,
          confirmLabel: 'Supprimer',
          variant: 'danger'
        };
      default:
        return null;
    }
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.orderService.getById(this.id()).subscribe({
      next: (order) => {
        this.order.set(order);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        void this.router.navigate(['/orders']);
      }
    });
  }

  ask(action: OrderAction): void {
    this.pendingAction.set(action);
  }

  closeDialog(): void {
    this.pendingAction.set(null);
  }

  confirmAction(): void {
    const action = this.pendingAction();
    if (!action) {
      return;
    }

    this.busy.set(true);

    if (action === 'delete') {
      this.orderService.delete(this.id()).subscribe({
        next: () => {
          this.notifier.success('Commande supprimée avec succès.');
          void this.router.navigate(['/orders']);
        },
        error: () => this.finishAction()
      });
      return;
    }

    const request$: Observable<ActionMessage> =
      action === 'validate' ? this.orderService.validate(this.id()) : this.orderService.cancel(this.id());

    request$.subscribe({
      next: (response) => {
        this.notifier.success(response.message);
        this.finishAction();
        this.load();
      },
      error: () => this.finishAction()
    });
  }

  private finishAction(): void {
    this.busy.set(false);
    this.pendingAction.set(null);
  }
}
