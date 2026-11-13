import { Component, computed, input } from '@angular/core';
import { OrderStatus } from '../../../core/models/order.model';
import { OrderStatusPipe } from '../../pipes/order-status.pipe';

const BADGE_CLASSES: Record<OrderStatus, string> = {
  [OrderStatus.Draft]: 'bg-amber-50 text-amber-700 ring-amber-200',
  [OrderStatus.Validated]: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  [OrderStatus.Cancelled]: 'bg-red-50 text-red-700 ring-red-200'
};

const DOT_CLASSES: Record<OrderStatus, string> = {
  [OrderStatus.Draft]: 'bg-amber-500',
  [OrderStatus.Validated]: 'bg-emerald-500',
  [OrderStatus.Cancelled]: 'bg-red-500'
};

@Component({
  selector: 'app-status-badge',
  imports: [OrderStatusPipe],
  templateUrl: './status-badge.component.html'
})
export class StatusBadgeComponent {
  readonly status = input.required<OrderStatus>();

  readonly badgeClass = computed(() => BADGE_CLASSES[this.status()] ?? 'bg-slate-100 text-slate-700 ring-slate-200');
  readonly dotClass = computed(() => DOT_CLASSES[this.status()] ?? 'bg-slate-400');
}
