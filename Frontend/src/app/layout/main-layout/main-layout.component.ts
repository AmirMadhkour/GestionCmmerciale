import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { APP_NAME } from '../../core/constants/app.constants';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, NavbarComponent],
  templateUrl: './main-layout.component.html'
})
export class MainLayoutComponent {
  readonly appName = APP_NAME;
  readonly year = new Date().getFullYear();
}
