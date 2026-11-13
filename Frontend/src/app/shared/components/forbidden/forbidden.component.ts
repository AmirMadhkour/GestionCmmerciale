import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DEFAULT_REDIRECT_URL } from '../../../core/constants/app.constants';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forbidden',
  imports: [RouterLink],
  templateUrl: './forbidden.component.html'
})
export class ForbiddenComponent {
  private readonly auth = inject(AuthService);

  readonly homeUrl = DEFAULT_REDIRECT_URL;
  readonly roleLabel = this.auth.roleLabel;
}
