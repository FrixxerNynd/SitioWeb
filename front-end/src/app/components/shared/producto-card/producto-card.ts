import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-producto-card',
  imports: [CommonModule],
  templateUrl: './producto-card.html',
  styleUrl: './producto-card.css',
})

//no se de donde obtiene la info btw
export class ProductoCard {
  @Input() nombre: string = '';
  @Input() precio: number = 0;
  @Input() precioOriginal: number = 0;
  @Input() descuento: number = 0;
  @Input() imagen: string = '';

  @Output() agregar = new EventEmitter<void>();
  @Output() verDetalles = new EventEmitter<void>();

  onAgregar() {
    this.agregar.emit();
  }

  onVerDetalles() {
    this.verDetalles.emit();
  }
}
