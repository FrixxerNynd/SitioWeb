import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UiNavbarComponent } from '../../components/layout/navbar/navbar';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, UiNavbarComponent],
  template: `
    <ui-navbar-component></ui-navbar-component>
    <router-outlet></router-outlet>
  `
})
export class MainLayout {}