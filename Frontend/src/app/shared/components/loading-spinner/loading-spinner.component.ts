import { Component, computed, input } from '@angular/core';

export type SpinnerSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-loading-spinner',
  templateUrl: './loading-spinner.component.html'
})
export class LoadingSpinnerComponent {
  readonly label = input('Chargement...');
  readonly size = input<SpinnerSize>('md');

  readonly sizeClass = computed(() => {
    switch (this.size()) {
      case 'sm':
        return 'h-4 w-4 border-2';
      case 'lg':
        return 'h-12 w-12 border-4';
      default:
        return 'h-8 w-8 border-[3px]';
    }
  });
}
