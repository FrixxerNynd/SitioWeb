import { Component, inject, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SecureAuthService, RecuperarCuentaRequest } from '../../../../services/secure-auth.service'; 

// Validador personalizado para email o RFC
function emailOrRFCValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value?.trim() || '';
  
  if (!value) {
    return { required: true };
  }
  
  // Validar si es email
  const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  const isValidEmail = emailPattern.test(value);
  
  // Validar si es RFC (13 caracteres: 4 letras, 6 números, 3 alfanuméricos)
  const rfcPattern = /^[A-ZÑ&]{4}\d{6}[A-Z0-9]{3}$/;
  const isValidRFC = rfcPattern.test(value.toUpperCase());
  
  if (!isValidEmail && !isValidRFC) {
    return { invalidEmailOrRFC: true };
  }
  
  return null;
}

@Component({
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './solicitar-correo.html',
})
export class PageSolicitarCorreo implements OnDestroy {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);
  
  isLoading = false;
  private navigationTimeout?: ReturnType<typeof setTimeout>;

  // Formulario reactivo con un solo campo
  recuperarForm: FormGroup = this.fb.group({
    identificador: ['', [Validators.required, emailOrRFCValidator]]
  });

  get identificador() { return this.recuperarForm.get('identificador'); }

  // Determina si el identificador es email o RFC
  private identificarTipo(valor: string): { tipo: 'email' | 'rfc', valor: string } {
    const trimmed = valor.trim();
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    
    if (emailPattern.test(trimmed)) {
      return { tipo: 'email', valor: trimmed };
    } else {
      return { tipo: 'rfc', valor: trimmed.toUpperCase() };
    }
  }

  onSubmit(): void {
    // Solo validar formato, no mostrar errores del backend
    if (this.recuperarForm.invalid) {
      this.identificador?.markAsTouched();
      return;
    }

    this.isLoading = true;

    const identificadorValue = this.recuperarForm.value.identificador;
    const { tipo, valor } = this.identificarTipo(identificadorValue);
    
    let request: RecuperarCuentaRequest;
    
    if (tipo === 'email') {
      request = {
        email: valor,
        rfc: ''
      };
    } else {
      request = {
        email: '',
        rfc: valor
      };
    }

    console.log(`📧 Solicitando recuperación por ${tipo}:`, valor);

    // Guardar el identificador y tipo
    localStorage.setItem('recuperacionIdentificador', identificadorValue);
    localStorage.setItem('recuperacionTipo', tipo);
    
    // NO guardamos el email todavía, porque si es RFC no tenemos el email real
    // El email se guardará cuando el backend responda con él
    // Por ahora solo guardamos el identificador original

    // Llamada al servicio - AHORA ESCUCHAMOS LA RESPUESTA para obtener el email real
    this.authService.recuperarCuenta(request).subscribe({
      next: (response) => {
        console.log('✅ Respuesta del servidor:', response);
        
        // Si el backend devuelve el email en la respuesta, lo guardamos
        if (response.token) {
          // Algunos backends devuelven el email en el token o en la respuesta
          // Ajusta esto según lo que devuelva tu backend
          console.log('Token recibido:', response.token);
        }
        
        // Guardar el email original si era email, o el que vino en la respuesta
        if (tipo === 'email') {
          localStorage.setItem('recuperacionEmail', valor);
        }
        // Si es RFC, necesitas que el backend te devuelva el email
        // Por ahora, como fallback, guardamos el identificador
        else {
          // En un backend correcto, aquí deberías guardar el email que viene en response.email
          // localStorage.setItem('recuperacionEmail', response.email);
          localStorage.setItem('recuperacionEmail', identificadorValue);
        }
      },
      error: (error) => {
        console.log('❌ Error en petición:', error);
        // Como fallback, guardamos el identificador
        localStorage.setItem('recuperacionEmail', identificadorValue);
      }
    });

    // Navegar inmediatamente sin esperar respuesta
    this.navigationTimeout = setTimeout(() => {
      this.router.navigate(['/recuperar-contrasena/verificar-codigo']);
      this.isLoading = false;
    }, 800);
  }

  ngOnDestroy(): void {
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }
  }
}