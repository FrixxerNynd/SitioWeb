import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { MainLayoutApp } from './app/main-layout/app';

bootstrapApplication(MainLayoutApp, appConfig)
  .catch((err) => console.error(err));
