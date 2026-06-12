import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SecureAuthService, CambiarPasswordRecuperacionRequest } from '../../../../services/secure-auth.service';

// Validador personalizado para confirmar contraseña
function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('nuevaPassword');
  const confirmPassword = control.get('confirmarPassword');
  
  if (password && confirmPassword && password.value !== confirmPassword.value) {
    return { passwordMismatch: true };
  }
  return null;
}

@Component({
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './nueva-contrasena.html',
})
export class PageNuevaContrasena implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);
  
  isLoading = false;
  showTooltip = false;
  tooltipMessage = '';
  tooltipType: 'success' | 'error' = 'success';
  countdown = 0;
  private navigationTimeout?: ReturnType<typeof setTimeout>;
  private tooltipTimeout?: ReturnType<typeof setTimeout>;
  private countdownInterval?: ReturnType<typeof setInterval>;
  email: string = '';
  token: string = '';

  // Formulario reactivo
  passwordForm: FormGroup = this.fb.group({
    nuevaPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmarPassword: ['', [Validators.required]]
  }, { validators: passwordMatchValidator });

  get nuevaPassword() { return this.passwordForm.get('nuevaPassword'); }
  get confirmarPassword() { return this.passwordForm.get('confirmarPassword'); }

  ngOnInit(): void {
    // Recuperar datos guardados
    this.email = localStorage.getItem('recuperacionEmail') || '';
    this.token = localStorage.getItem('recuperacionToken') || '';
    
    // Verificar que el token tenga exactamente 6 dígitos
    if (this.token && this.token.length !== 6) {
      console.error(`❌ Token inválido - debe ser de 6 dígitos, tiene: ${this.token.length} dígitos`);
      this.showTooltipMessage(`Código inválido (${this.token.length} dígitos). Debe ser de 6 dígitos`, 'error');
      setTimeout(() => {
        this.router.navigate(['/recuperar-contrasena/verificar-codigo']);
      }, 3000);
      return;
    }
    
    if (!this.email || !this.token) {
      console.error('❌ Faltan datos: email o token no encontrados');
      console.log('Email:', this.email);
      console.log('Token:', this.token);
      
      this.showTooltipMessage('Error: Datos de recuperación no encontrados', 'error');
      setTimeout(() => {
        this.router.navigate(['/recuperar-contrasena/solicitar-correo']);
      }, 2000);
    }
  }

  showTooltipMessage(message: string, type: 'success' | 'error' = 'success'): void {
    this.tooltipMessage = message;
    this.tooltipType = type;
    this.showTooltip = true;
    
    // Si es éxito, iniciar contador de 3 segundos
    if (type === 'success') {
      this.startCountdown(3);
    }
    
    // Auto-cerrar después de 4 segundos si no hay contador
    if (type !== 'success') {
      this.tooltipTimeout = setTimeout(() => {
        this.showTooltip = false;
      }, 4000);
    }
  }

  startCountdown(seconds: number): void {
    this.countdown = seconds;
    
    // Limpiar intervalo anterior si existe
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
    
    this.countdownInterval = setInterval(() => {
      this.countdown--;
      
      // Actualizar mensaje del tooltip con el contador
      this.tooltipMessage = `¡Tu contraseña se cambió con éxito! Redirigiendo en ${this.countdown} segundos...`;
      
      if (this.countdown <= 0) {
        // Detener el contador
        if (this.countdownInterval) {
          clearInterval(this.countdownInterval);
          this.countdownInterval = undefined;
        }
        // Redirigir al login
        this.router.navigate(['/inicio-sesion']);
        this.isLoading = false;
        this.showTooltip = false;
      }
    }, 1000);
  }

  closeTooltip(): void {
    // Detener el contador si está activo
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
      this.countdownInterval = undefined;
    }
    if (this.tooltipTimeout) {
      clearTimeout(this.tooltipTimeout);
    }
    this.showTooltip = false;
    this.countdown = 0;
    // Redirigir inmediatamente si se cierra manualmente
    this.router.navigate(['/inicio-sesion']);
    this.isLoading = false;
  }

  onSubmit(): void {
    // Solo validar que las contraseñas cumplan los requisitos
    if (this.passwordForm.invalid) {
      Object.keys(this.passwordForm.controls).forEach(key => {
        const control = this.passwordForm.get(key);
        control?.markAsTouched();
      });
      return;
    }

    this.isLoading = true;

    const request: CambiarPasswordRecuperacionRequest = {
      email: this.email,
      nuevoPassword: this.passwordForm.value.nuevaPassword,
      token: this.token
    };

    // Llamada al servicio
    this.authService.cambiarPasswordRecuperacion(request).subscribe({
      next: (response) => {
        console.log('✅ Respuesta del servidor:', response);
        
        if (response.success) {
          console.log('✅ Contraseña actualizada exitosamente');
          
          // Limpiar datos temporales
          localStorage.removeItem('recuperacionEmail');
          localStorage.removeItem('recuperacionToken');
          localStorage.removeItem('recuperacionIdentificador');
          localStorage.removeItem('recuperacionTipo');
          
          // Mostrar tooltip de éxito con contador
          this.showTooltipMessage('¡Tu contraseña se cambió con éxito! Redirigiendo en 3 segundos...', 'success');
          
          // Navegar 
          this.navigationTimeout = setTimeout(() => {
            this.router.navigate(['/inicio-sesion']);
            this.isLoading = false;
          }, 2000);          
        } else {
          console.error('❌ Error en respuesta:', response.message);
          this.isLoading = false;
          this.showTooltipMessage(response.message || 'No se pudo cambiar la contraseña', 'error');

          // Navegar 
          this.navigationTimeout = setTimeout(() => {
            this.router.navigate(['/inicio-sesion']);
            this.isLoading = false;
          }, 2000);          
        }
      },
      error: (error) => {
        console.error('❌ Error detallado:');
        console.error('Status:', error.status);
        console.error('Message:', error.error?.message || error.message);
        
        this.isLoading = false;
        let errorMsg = 'Error al cambiar la contraseña';
        
        if (error.status === 400) {
          errorMsg = error.error?.message || 'Token inválido o expirado. Solicita un nuevo código';
        } else if (error.status === 404) {
          errorMsg = 'Usuario no encontrado';
        } else if (error.status === 401) {
          errorMsg = 'Token inválido. Solicita un nuevo código';
        }
        
        this.showTooltipMessage(errorMsg, 'error');
      }
    });
  }

  ngOnDestroy(): void {
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }
    if (this.tooltipTimeout) {
      clearTimeout(this.tooltipTimeout);
    }
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }
}