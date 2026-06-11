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
  @Input() tipo: 'primario' | 'secundario' | 'cerrar' | 'icono' | 'contador' = 'primario';
  @Input() text: string = 'texto boton';
  @Input() viewIcon: boolean = false;
  @Input() size: 'xs' | 'md' | 'lg' = 'xs';
  @Input() disabled: boolean = true; 
  @Input() nomeIco: string = 'home';
  @Input() widthType: 'fixed' | 'full' | 'auto' = 'auto';
  @Input() customWidth: string = '';
  
  // Inputs para activar el botón automáticamente
  @Input() enableWhen: any = null; 
  @Input() enableWhenFormValid: boolean = false;

  contador: number = 0;

  
  get fullWidth(): boolean {
    return this.widthType === 'full';
  }
  
  get buttonWidth(): string {
    if (this.widthType === 'full') return '100%';
    if (this.widthType === 'fixed' && this.customWidth) return this.customWidth;
    return 'auto';
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
}

decrementar() {
  if (this.contador > 0) {
    this.contador--;
  }
}

}