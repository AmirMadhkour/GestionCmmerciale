import { Pipe, PipeTransform } from '@angular/core';
import { ORDER_STATUS_LABELS, OrderStatus } from '../../core/models/order.model';

@Pipe({ name: 'orderStatus' })
export class OrderStatusPipe implements PipeTransform {
  transform(status: OrderStatus | string | null | undefined): string {
    if (!status) {
      return '';
    }
    return ORDER_STATUS_LABELS[status as OrderStatus] ?? status;
  }
}
