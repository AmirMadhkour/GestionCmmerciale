import { Component, computed, input, output } from '@angular/core';

export type ConfirmDialogVariant = 'danger' | 'primary' | 'warning';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  host: {
    '(document:keydown.escape)': 'onEscape()'
  }
})
export class ConfirmDialogComponent {
  readonly open = input(false);
  readonly heading = input('Confirmation');
  readonly message = input('');
  readonly confirmLabel = input('Confirmer');
  readonly cancelLabel = input('Annuler');
  readonly variant = input<ConfirmDialogVariant>('danger');
  readonly busy = input(false);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  readonly confirmButtonClass = computed(() => {
    switch (this.variant()) {
      case 'primary':
        return 'btn-primary';
      case 'warning':
        return 'btn-warning';
      default:
        return 'btn-danger';
    }
  });

  readonly iconClass = computed(() => {
    switch (this.variant()) {
      case 'primary':
        return 'bg-primary-100 text-primary-700';
      case 'warning':
        return 'bg-amber-100 text-amber-700';
      default:
        return 'bg-red-100 text-red-700';
    }
  });

  confirm(): void {
    if (!this.busy()) {
      this.confirmed.emit();
    }
  }

  cancel(): void {
    if (!this.busy()) {
      this.cancelled.emit();
    }
  }

  onEscape(): void {
    if (this.open()) {
      this.cancel();
    }
  }
}
