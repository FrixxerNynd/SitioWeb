import { Component } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';  
import { UiBoton } from '../../../../components/shared/boton/boton';

@Component({
  imports: [ReactiveFormsModule, RouterModule,UiBoton],
  templateUrl: './datos-personales.html',
})

export class PageDatosPersonales {

}
