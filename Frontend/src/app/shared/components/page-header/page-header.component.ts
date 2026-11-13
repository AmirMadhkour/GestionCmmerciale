import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-page-header',
  imports: [RouterLink],
  templateUrl: './page-header.component.html'
})
export class PageHeaderComponent {
  readonly heading = input.required<string>();
  readonly subtitle = input<string>();
  readonly backLink = input<string | unknown[]>();
  readonly backLabel = input('Retour');
}
