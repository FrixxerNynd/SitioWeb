import { Component, Input, Output, EventEmitter, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiBoton } from '../../../../../../components/shared/boton/boton';
import { UiIconComponent } from "../../../../../../components/shared/icono/icono.component";

@Component({
  selector: 'modal-detalles-orden',
  standalone: true,
  imports: [CommonModule, UiBoton, UiIconComponent],
  templateUrl: './detalles-orden.html',
  styleUrls: ['./detalles-orden.css']
})
export class ModalDetallesOrdenComponent implements OnChanges {
  @Input() orden: any = null;
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() cerrar = new EventEmitter<void>();
  @Output() cancelar = new EventEmitter<any>();

  // Signals internas
  ordenData = signal<any>(null);
  mostrarEsqueleto = signal(true);
  monstrarDatos = signal(false);
  errorDeConexion = signal(false);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['orden'] && this.orden) {
      this.ordenData.set(this.orden);
      this.cargarDatos();
    }
  }

  // Getters
  get idDocumento(): string {
    return this.ordenData()?.id || '';
  }

  // Formatear moneda
  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
      minimumFractionDigits: 2,
      useGrouping: true
    }).format(value);
  }

  // Formatear fecha
  formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('es-MX', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(date);
  }

  // Total calculado
  getTotalCalculado(): number {
    const orden = this.ordenData();
    if (!orden) return 0;
    return (orden.subtotal || 0) + (orden.iva || 0) + (orden.flete || 0);
  }

  // Cargar datos
  cargarDatos() {
    this.mostrarEsqueleto.set(true);
    this.monstrarDatos.set(false);
    // Temporal eliminar a futuro
    setTimeout(() => {
      this.mostrarEsqueleto.set(false);
      this.monstrarDatos.set(true);
    }, 500);
  }

  // Cerrar panel
  cerrarPanel() {
    this.visibleChange.emit(false);
    this.cerrar.emit();
  }

  cancelarOrden() {
    this.cancelar.emit(this.ordenData());
    this.cerrarPanel();
  }
}