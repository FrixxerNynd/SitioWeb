import { Component, Input, Output, EventEmitter, signal, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiBoton } from '../../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../../components/shared/icono/icono.component';

@Component({
  selector: 'modal-detalles-usuario',
  standalone: true,
  imports: [CommonModule, UiBoton, UiIconComponent],
  templateUrl: './detalles-usuario.html',
  styleUrl: './detalles-usuario.css'
})
export class ModalDetallesUsuarioComponent {
  @Input() usuario: any = null;
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() cerrar = new EventEmitter<void>();

  usuarioData = signal<any>(null);
  mostrarEsqueleto = signal(true);
  monstrarDatos = signal(false);
  errorDeConexion = signal(false);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['usuario'] && this.usuario) {
      this.usuarioData.set(this.usuario);
      this.cargarDatos();
    }
  }

  get idDocumento(): string {
    return this.usuarioData()?.id || '';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
      minimumFractionDigits: 2
    }).format(value);
  }

  cargarDatos() {
    this.mostrarEsqueleto.set(true);
    this.monstrarDatos.set(false);
    setTimeout(() => {
      this.mostrarEsqueleto.set(false);
      this.monstrarDatos.set(true);
    }, 500);
  }

  cerrarPanel() {
    this.visibleChange.emit(false);
    this.cerrar.emit();
  }
}