import { Component, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RecaptchaService } from '../../../../services/RecaptchaService';


@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './cuenta.html',
})
export class PageCuenta implements OnInit, OnDestroy{
  private fb = inject(FormBuilder);
  private recaptchaService = inject(RecaptchaService);

  // Señal para controlar el modal
  visualizarModal = signal<boolean>(false);
  recaptchaError = signal<boolean>(false);

  cuentaForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    terminos: [false, Validators.requiredTrue]
  });

   ngOnInit() {
    // Espera a que el DOM esté listo antes de renderizar el widget
    setTimeout(() => {
      this.recaptchaService.cargarV2('recaptcha-v2-container');
    }, 300);
  }

  ngOnDestroy() {
    this.recaptchaService.eliminarV2();
  }

  onSubmit() {
    const token = this.recaptchaService.obtenerTokenV2();

    if (!token) {
      this.recaptchaError.set(true);
      return;
    }

    if (this.cuentaForm.invalid) {
      this.cuentaForm.markAllAsTouched();
      return;
    }

    this.recaptchaError.set(false);

    const payload = {
      ...this.cuentaForm.value,
      recaptchaToken: token
    };

    // Aquí se llama el servicio de registro
    console.log('Payload cuenta:', payload);
  }

  abrirModal() { this.visualizarModal.set(true); }
  cerrarModal() { this.visualizarModal.set(false); }
}
