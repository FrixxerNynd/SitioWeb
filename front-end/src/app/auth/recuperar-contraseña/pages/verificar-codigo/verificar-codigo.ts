import { Component } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UiBoton } from '../../../../components/shared/boton/boton';

@Component({
  imports: [ReactiveFormsModule, RouterModule, CommonModule,UiBoton],
  templateUrl: './verificar-codigo.html',
})
export class PageVerificarCodigo {

}
