import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiIconComponent } from '../icono/icono.component';

@Component({
  selector: 'ui-boton',
  imports: [CommonModule, UiIconComponent],
  templateUrl: './boton.html',
  styleUrl: './boton.css',
})
export class UiBoton implements OnInit {

  @Input() type: 'submit' | 'reset' | 'button' = 'button';
  @Input() tipo: 'primario' | 'secundario' | 'cerrar' | 'icono' | 'contador' | 'btn-tabla-pie' = 'primario';
  @Input() text: string = 'texto boton';
  @Input() viewIcon: boolean = false;
  @Input() size: 'xs' | 'sm' | 'md' | 'lg' | 'xl' = 'md';
  @Input() disabled: boolean = false;
  @Input() nomeIco: string = 'home';
  @Input() widthType: 'fixed' | 'full' | 'auto' = 'auto';
  @Input() customWidth: string = '';

  // Inputs para activar el botón automáticamente
  @Input() enableWhen: any = null;
  @Input() enableWhenFormValid: boolean = false;

  // Input del contador 
  @Input() contador: number = 0;
  @Output() contadorChange = new EventEmitter<number>();

  // Control de tamaño
  @Input() sizeControl: 'xs' | 'sm' | 'md' | 'lg' | 'xl' = 'md';

  get fullWidth(): boolean {
    return this.widthType === 'full';
  }

  get buttonWidth(): string {
    if (this.widthType === 'full') return '100%';
    if (this.widthType === 'fixed' && this.customWidth) return this.customWidth;
    return 'auto';
  }

  // Obtener clases de tamaño
  get sizeClasses(): string {
    const sizes = {
      'xs': 'px-2 py-1 text-xs',
      'sm': 'px-3 py-1.5 text-sm',
      'md': 'px-4 py-2 text-sm',
      'lg': 'px-5 py-2.5 text-base',
      'xl': 'px-6 py-3 text-lg'
    };
    return sizes[this.size] || sizes['md'];
  }

  // Obtener clases de tamaño para el contador
  get contadorSizeClasses(): string {
    const sizes = {
      'xs': 'h-7 text-xs',
      'sm': 'h-8 text-sm',
      'md': 'h-10 text-sm',
      'lg': 'h-12 text-base',
      'xl': 'h-14 text-lg'
    };
    return sizes[this.size] || sizes['md'];
  }

  // Obtener clases de tamaño para los botones del contador
  get contadorBtnSizeClasses(): string {
    const sizes = {
      'xs': 'w-7 h-7',
      'sm': 'w-8 h-8',
      'md': 'w-10 h-10',
      'lg': 'w-12 h-12',
      'xl': 'w-14 h-14'
    };
    return sizes[this.size] || sizes['md'];
  }

  // Obtener tamaño de icono
  get iconSize(): string {
    const sizes = {
      'xs': 'w-3 h-3',
      'sm': 'w-3.5 h-3.5',
      'md': 'w-4 h-4',
      'lg': 'w-5 h-5',
      'xl': 'w-6 h-6'
    };
    return sizes[this.size] || sizes['md'];
  }

  @Output() btnClick = new EventEmitter<Event>();

  ngOnInit() {
    // Si hay enableWhen con valor, habilitar
    if (this.enableWhen !== null && this.enableWhen !== undefined) {
      this.disabled = false;
    }
  }

  ngOnChanges(changes: any) {
    // Reaccionar a cambios en enableWhen
    if (changes['enableWhen'] && changes['enableWhen'].currentValue) {
      this.disabled = false;
    }
    // Reaccionar a cambios en enableWhenFormValid
    if (changes['enableWhenFormValid']) {
      this.disabled = !changes['enableWhenFormValid'].currentValue;
    }
  }

  onClick(event: Event) {
    if (!this.disabled) {
      this.btnClick.emit(event);
    }
  }

  incrementar() {
    this.contador++;
    this.contadorChange.emit(this.contador);
  }

  decrementar() {
    if (this.contador > 0) {
      this.contador--;
      this.contadorChange.emit(this.contador);
    }
  }
}