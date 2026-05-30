import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';  
import { UiBoton } from '../../../../components/shared/boton/boton';

@Component({
  imports: [ReactiveFormsModule, RouterModule, UiBoton],
  templateUrl: './datos-fiscales.html',
})
export class PageDatosFiscales {
  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      rfc: ['', [Validators.required, Validators.pattern(/^[A-ZÑ&]{4}\d{6}[A-Z0-9]{3}$/)]],
      constancia: [null, Validators.required]
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.loginForm.patchValue({ constancia: file });
      this.loginForm.get('constancia')?.updateValueAndValidity();
    }
  }

  siguiente() {
    if (this.loginForm.valid) {
      // Guardar datos si es necesario
      console.log('Formulario válido', this.loginForm.value);
      this.router.navigate(['/pre-registro/datos-personales']);
    } else {
      // Marcar todos los campos como tocados para mostrar errores
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
    }
  }
}