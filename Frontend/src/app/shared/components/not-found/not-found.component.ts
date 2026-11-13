import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DEFAULT_REDIRECT_URL } from '../../../core/constants/app.constants';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  templateUrl: './not-found.component.html'
})
export class NotFoundComponent {
  readonly homeUrl = DEFAULT_REDIRECT_URL;
}
