// front-end/src/app/auth/inicio-sesion/inicio-sesion.ts
import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SecureAuthService } from '../../services/secure-auth.service';
import { RecaptchaService } from '../../services/RecaptchaService';

@Component({
  selector: 'app-inicio-sesion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './inicio-sesion.html',
})
export class PageInicioSesion implements OnInit, OnDestroy{
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);
  private recaptchaService = inject(RecaptchaService);

  loginForm: FormGroup;

  // Señales estado
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, this.emailOrRfcValidator()]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

  }

  ngOnInit() {
    this.recaptchaService.cargarV3();
  }

  ngOnDestroy() {
    this.recaptchaService.eliminarV3();
  }

  private emailOrRfcValidator() {
    return (control: any) => {
      const value = control.value;
      if (!value) return { required: true };

      // Validar email (formato estándar)
      const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
      // Validar RFC (México - formato básico)
      const rfcRegex = /^[A-ZÑ&]{3,4}[0-9]{6}[A-Z0-9]{3}$/;

      if (emailRegex.test(value) || rfcRegex.test(value)) {
        return null; // Válido
      }

      return { invalidEmailOrRfc: true };
    };
  }

   async onSubmit(): Promise<void> {
    // Verificar si el formulario es inválido
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.errorMessage.set("Por favor, completa correctamente los campos.");
      return;
    }

    // Actualizar señales correctamente con .set()
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null); // Limpiar mensaje de éxito anterior

    const recaptchaToken = await this.recaptchaService.ejecutarV3('login');

    const loginData = {
      email: this.loginForm.get('email')?.value,
      password: this.loginForm.get('password')?.value,
      recaptchaToken,
    };
    console.log( 'Token google:', {recaptchaToken: loginData.email})

    console.log('🔐 Enviando login:', { email: loginData.email });

    this.authService.login(loginData).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        console.log('✅ Login exitoso:', response);
        if (response && response.user) {
          this.successMessage.set("¡Inicio de sesión exitoso! Redirigiendo al catálogo...");

          // Redirigir después de 2 segundos
          setTimeout(() => {
            console.log('🚀 Redirigiendo a /catalogo-producto');
            this.router.navigate(['/catalogo-producto']);
          }, 2000);
        } else {
          this.errorMessage.set("Error al iniciar sesión: respuesta inválida del servidor");
        }
      },
      error: (error) => {
        // Actualizar señales con .set()
        this.isLoading.set(false);
        console.error('❌ Error de login:', error);

        // ✅ Todos los errores usando .set()
        if (error.status === 0) {
          this.errorMessage.set("No se pudo conectar con el servidor. Verifica que el backend esté corriendo en http://localhost:5176");
        } else if (error.status === 401) {
          this.errorMessage.set('Credenciales incorrectas. Verifica tu correo y contraseña.');
        } else if (error.status === 404) {
          this.errorMessage.set('Endpoint de login no encontrado. Verifica la URL del backend.');
        } else if (error.error?.message) {
          this.errorMessage.set(error.error.message);
        } else {
          this.errorMessage.set(`Error ${error.status}: No se pudo iniciar sesión.`);
        }
        // Limpiar campo de contraseña
        this.loginForm.patchValue({ password: '' });
      }
    });
  }
}
