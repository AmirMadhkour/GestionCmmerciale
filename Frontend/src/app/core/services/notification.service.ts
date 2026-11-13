import { Injectable, signal } from '@angular/core';
import { NOTIFICATION_DURATION_MS } from '../constants/app.constants';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface AppNotification {
  id: number;
  type: NotificationType;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 0;
  private readonly items = signal<AppNotification[]>([]);

  readonly notifications = this.items.asReadonly();

  success(message: string): void {
    this.show('success', message);
  }

  error(message: string): void {
    this.show('error', message);
  }

  warning(message: string): void {
    this.show('warning', message);
  }

  info(message: string): void {
    this.show('info', message);
  }

  dismiss(id: number): void {
    this.items.update((items) => items.filter((item) => item.id !== id));
  }

  private show(type: NotificationType, message: string): void {
    if (this.items().some((item) => item.type === type && item.message === message)) {
      return;
    }
    const id = ++this.nextId;
    this.items.update((items) => [...items, { id, type, message }]);
    setTimeout(() => this.dismiss(id), NOTIFICATION_DURATION_MS);
  }
}
