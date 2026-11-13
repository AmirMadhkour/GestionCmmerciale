import { Component, inject } from '@angular/core';
import { NotificationService, NotificationType } from '../../../core/services/notification.service';

@Component({
  selector: 'app-notification-toast',
  templateUrl: './notification-toast.component.html'
})
export class NotificationToastComponent {
  private readonly notificationService = inject(NotificationService);

  readonly notifications = this.notificationService.notifications;

  readonly containerClasses: Record<NotificationType, string> = {
    success: 'border-emerald-200 bg-emerald-50 text-emerald-800',
    error: 'border-red-200 bg-red-50 text-red-800',
    warning: 'border-amber-200 bg-amber-50 text-amber-800',
    info: 'border-sky-200 bg-sky-50 text-sky-800'
  };

  readonly iconClasses: Record<NotificationType, string> = {
    success: 'bg-emerald-600',
    error: 'bg-red-600',
    warning: 'bg-amber-500',
    info: 'bg-sky-600'
  };

  readonly icons: Record<NotificationType, string> = {
    success: '✓',
    error: '✕',
    warning: '!',
    info: 'i'
  };

  dismiss(id: number): void {
    this.notificationService.dismiss(id);
  }
}
