import { Component, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RecaptchaService } from '../../../../services/RecaptchaService';
import { PreRegistroService } from '../../pre-registro.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './cuenta.html',
})
export class PageCuenta implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private recaptchaService = inject(RecaptchaService);
  private preRegistroService = inject(PreRegistroService);

  // Estado de UI
  visualizarModal = signal<boolean>(false);
  recaptchaError  = signal<boolean>(false);
  cargando        = signal<boolean>(false);
  errorMensaje    = signal<string | null>(null);

  cuentaForm: FormGroup = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    terminos: [false, Validators.requiredTrue],
  });

  ngOnInit() {
    // Espera a que el DOM esté listo antes de renderizar el widget
    setTimeout(() => {
      this.recaptchaService.cargarV2('recaptcha-v2-container');
    }, 300);

    const estado = this.preRegistroService.obtenerEstado();
    if (estado.email) {
      this.cuentaForm.patchValue({
        email: estado.email
      });
    }
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
    this.errorMensaje.set(null);
    this.cargando.set(true);

    // Guardar datos del último paso
    this.preRegistroService.guardarPaso({
      email:         this.cuentaForm.value.email,
      contrasena:    this.cuentaForm.value.password,
      recaptchaToken: token,
    });

    // Llamar al endpoint
    this.preRegistroService.registrar().subscribe({
      next: () => {
        this.preRegistroService.limpiar();
        this.cargando.set(false);
        // Navegar a login con mensaje de éxito via state
        this.router.navigate(['/inicio-sesion'], {
          state: { mensaje: 'Pre-registro enviado. Tu cuenta será activada en breve por un administrador.' }
        });
      },
      error: (err) => {
        this.cargando.set(false);
        const status = err?.status;
        if (status === 400 || status === 409) {
          this.errorMensaje.set(err?.error?.message ?? 'Datos inválidos. Verifica tu información.');
          console.log("Respuesta: ",err?.error)
        } else if (status === 403) {
          this.errorMensaje.set(err?.error?.message ?? 'Tu cuenta está inactiva.');
        } else {
          this.errorMensaje.set('Ocurrió un error al enviar tu solicitud. Intenta de nuevo.');
        }
      }
    });
  }

  abrirModal()  { this.visualizarModal.set(true); }
  cerrarModal() { this.visualizarModal.set(false); }
}
