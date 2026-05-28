import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UiNavbarComponent } from '../components/layout/navbar/navbar';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,UiNavbarComponent],
  templateUrl: './app.html',
})
export class MainLayoutApp {
  protected readonly title = signal('frontend');
}
