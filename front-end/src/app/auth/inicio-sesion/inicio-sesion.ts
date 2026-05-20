// front-end/src/app/auth/inicio-sesion/inicio-sesion.ts
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SecureAuthService } from '../../services/secure-auth.service';

@Component({
  selector: 'app-inicio-sesion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './inicio-sesion.html',
})
export class PageInicioSesion {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  loginForm: FormGroup;
  
  // Estados 
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

constructor() {
  this.loginForm = this.fb.group({
    email: ['', [Validators.required, this.emailOrRfcValidator()]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
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

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.errorMessage.set("Por favor, completa correctamente los campos.");
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const loginData = {
      email: this.loginForm.get('email')?.value,
      password: this.loginForm.get('password')?.value
    };

    console.log('🔐 Enviando login:', { email: loginData.email });

    this.authService.login(loginData).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        console.log('✅ Login exitoso:', response);
        
        if (response && response.user) {
          console.log('🚀 Redirigiendo a /catalogo-producto');
          this.router.navigate(['/catalogo-producto']);
        } else {
          this.errorMessage.set("Error al iniciar sesión: respuesta inválida del servidor");
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        console.error('❌ Error de login:', error);
        
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
        
        this.loginForm.patchValue({ password: '' });
      }
    });
  }
}