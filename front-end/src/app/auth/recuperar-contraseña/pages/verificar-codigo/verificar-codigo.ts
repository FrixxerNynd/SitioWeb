import { Component, inject, OnInit, OnDestroy, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { UiBoton } from '../../../../components/shared/boton/boton';

@Component({
  imports: [ReactiveFormsModule, RouterModule, CommonModule, UiBoton],
  templateUrl: './verificar-codigo.html',
})
export class PageVerificarCodigo implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  
  @Input() emailRecuperacion: string = '';
  
  isLoading = false;
  private navigationTimeout?: ReturnType<typeof setTimeout>;
  
  // Formulario con los 6 dígitos del código
  codigoForm: FormGroup = this.fb.group({
    codigo1: ['', [Validators.required, Validators.pattern(/^\d$/)]],
    codigo2: ['', [Validators.required, Validators.pattern(/^\d$/)]],
    codigo3: ['', [Validators.required, Validators.pattern(/^\d$/)]],
    codigo4: ['', [Validators.required, Validators.pattern(/^\d$/)]],
    codigo5: ['', [Validators.required, Validators.pattern(/^\d$/)]],
    codigo6: ['', [Validators.required, Validators.pattern(/^\d$/)]]
  });

  ngOnInit(): void {
    // Verificar que tenemos los datos necesarios del paso anterior
    const savedIdentificador = localStorage.getItem('recuperacionIdentificador');
    const savedEmail = localStorage.getItem('recuperacionEmail');
    
    if (!savedIdentificador) {
      this.router.navigate(['/recuperar-contrasena/solicitar-correo']);
    }
    
    if (savedEmail) {
      this.emailRecuperacion = savedEmail;
    }
  }

  getCodigoCompleto(): string {
    const codigo = `${this.codigoForm.get('codigo1')?.value || ''}${
                   this.codigoForm.get('codigo2')?.value || ''}${
                   this.codigoForm.get('codigo3')?.value || ''}${
                   this.codigoForm.get('codigo4')?.value || ''}${
                   this.codigoForm.get('codigo5')?.value || ''}${
                   this.codigoForm.get('codigo6')?.value || ''}`;
    
    return codigo;
  }

  onSubmit(): void {
    // Marcar todos los campos como touched para validación
    Object.keys(this.codigoForm.controls).forEach(key => {
      this.codigoForm.get(key)?.markAsTouched();
    });
    
    if (this.codigoForm.invalid) {
      console.error('❌ Formulario inválido - algunos campos no tienen 1 dígito');
      return;
    }
    
    const codigoCompleto = this.getCodigoCompleto();
    
    // Validar que el código tenga exactamente 6 dígitos
    if (codigoCompleto.length !== 6) {
      console.error('❌ Código incompleto - tiene', codigoCompleto.length, 'dígitos');
      return;
    }

    this.isLoading = true;
    
    // Guardar el token y email para usarlos en el siguiente paso
    localStorage.setItem('recuperacionToken', codigoCompleto);
    localStorage.setItem('recuperacionEmail', this.emailRecuperacion);
    
    this.navigationTimeout = setTimeout(() => {
      this.router.navigate(['/recuperar-contrasena/nueva-contrasena']);
      this.isLoading = false;
    }, 800);
  }

  reenviarCodigo(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/recuperar-contrasena/solicitar-correo']);
  }

  // Validar que solo se ingresen números
  onInput(event: Event, currentIndex: number): void {
    const input = event.target as HTMLInputElement;
    // Solo permitir números
    let value = input.value.replace(/\D/g, '');
    
    if (value.length > 1) {
      value = value.charAt(0);
    }
    
    input.value = value;
    this.codigoForm.get(`codigo${currentIndex}`)?.setValue(value);
    
    // Auto-focus al siguiente input
    if (value && value.length === 1 && currentIndex < 6) {
      const nextInput = document.getElementById(`codigo${currentIndex + 1}`) as HTMLInputElement;
      if (nextInput) {
        nextInput.focus();
      }
    }
    
    // Si se completa el último input, verificar si todos están llenos
    if (currentIndex === 6 && value && value.length === 1) {
      setTimeout(() => {
        if (this.codigoForm.valid) {
          console.log('✅ Código completo detectado, enviando...');
          this.onSubmit();
        }
      }, 100);
    }
  }

  onKeyDown(event: KeyboardEvent, currentIndex: number): void {
    const input = event.target as HTMLInputElement;
    
    // Permitir teclas de control
    if (event.key === 'ArrowLeft' || event.key === 'ArrowRight' || 
        event.key === 'Tab' || event.key === 'Enter') {
      return;
    }
    
    // Si es backspace y el campo está vacío, ir al anterior
    if (event.key === 'Backspace' && !input.value && currentIndex > 1) {
      const prevInput = document.getElementById(`codigo${currentIndex - 1}`) as HTMLInputElement;
      if (prevInput) {
        prevInput.focus();
        prevInput.value = '';
        this.codigoForm.get(`codigo${currentIndex - 1}`)?.setValue('');
        event.preventDefault();
      }
    }
  }

  // Manejar pegado de código completo
  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pastedText = event.clipboardData?.getData('text') || '';
    const digits = pastedText.replace(/\D/g, '').split('').slice(0, 6);
    
    digits.forEach((digit, index) => {
      const controlName = `codigo${index + 1}`;
      if (this.codigoForm.get(controlName)) {
        this.codigoForm.get(controlName)?.setValue(digit);
        const input = document.getElementById(controlName) as HTMLInputElement;
        if (input) {
          input.value = digit;
        }
      }
    });
    
    // Enfocar el siguiente input después del último pegado
    const nextIndex = digits.length + 1;
    if (nextIndex <= 6) {
      const nextInput = document.getElementById(`codigo${nextIndex}`) as HTMLInputElement;
      if (nextInput) {
        nextInput.focus();
      }
    } else {
      // Si se pegaron 6 dígitos, enviar automáticamente
      setTimeout(() => {
        if (this.codigoForm.valid) {
          this.onSubmit();
        }
      }, 100);
    }
  }

  ngOnDestroy(): void {
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }
  }
}