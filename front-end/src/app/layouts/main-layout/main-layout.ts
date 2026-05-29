import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UiNavbarComponent } from '../../components/layout/navbar/navbar';
import { UiFooterComponent } from '../../components/shared/footer/footer';


@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, UiNavbarComponent,UiFooterComponent],
  template: `
    <ui-navbar-component></ui-navbar-component>
    <router-outlet></router-outlet>
    <ui-footer></ui-footer>

  `
})
export class MainLayout {}