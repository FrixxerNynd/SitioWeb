import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'ui-producto-card',
  imports: [CommonModule, RouterLink],
  templateUrl: './producto-card.html',
  styleUrl: './producto-card.css',
})
export class UIProductoCard {
  @Input() nombre: string = '';
  @Input() precio: number = 0;
  @Input() precioOriginal: number = 0;
  @Input() descuento: number = 0;
  @Input() imagen?: string = '';
  @Input() oferta: boolean = false;
  @Input() stock: number = 0;
  @Input() marca: string = '';
  @Input() categoria: string = '';
  @Input() referencia: string = '';

  @Output() agregar = new EventEmitter<void>();

  get tieneOferta(): boolean {
    return this.oferta && this.descuento > 0;
  }

  get stockClass(): string {
    if (this.stock === 0) return 'stock-agotado';
    if (this.stock <= 5) return 'stock-bajo';
    return 'stock-disponible';
  }
  onAgregar() {
    this.agregar.emit();
  }
}
