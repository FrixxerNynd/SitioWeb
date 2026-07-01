import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { UiBoton } from '../../../../components/shared/boton/boton';
import { PreRegistroService } from '../../pre-registro.service';

@Component({
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './datos-fiscales.html',
})
export class PageDatosFiscales {
  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private preRegistroService: PreRegistroService
  ) {
    // Limpiar cualquier estado previo al iniciar el wizard
    this.preRegistroService.limpiar();

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
      this.preRegistroService.guardarArchivoConstancia(file);
    } else {
      this.loginForm.patchValue({ constancia: null });
      this.loginForm.get('constancia')?.updateValueAndValidity();
    }
  }

  siguiente() {
    if (this.loginForm.valid) {
      // Guardar RFC en el estado del wizard (constancia se sube por endpoint separado en el futuro)
      this.preRegistroService.guardarPaso({ RFC: this.loginForm.value.rfc });
      this.router.navigate(['/pre-registro/datos-personales']);
    } else {
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
    }
  }
}