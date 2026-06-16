import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UitipografiaComponent } from '../../../../../../components/shared/tipografia/tipografia.component';
import { FormsModule } from '@angular/forms';
import {UiBoton} from '../../../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../../../components/shared/icono/icono.component';

interface Producto {
  id: number;
  nombre: string;
  descripcion: string;
  precio: number;
  precioOriginal: number;
  cantidad: number;
  imagen: string;
}

type TipoPago = 'efectivo' | 'transferencia' | 'credito';

@Component({
  selector: 'app-carrito',
  standalone: true,
  imports: [CommonModule,FormsModule, UiBoton],
  templateUrl: './producto.html',
})
export class  PageCarritoComponent {

  tipoPagoSeleccionado: TipoPago = 'efectivo';

  tiposPago: { valor: TipoPago; etiqueta: string }[] = [
    { valor: 'efectivo',      etiqueta: 'Efectivo' },
    { valor: 'transferencia', etiqueta: 'Transferencia' },
    { valor: 'credito',       etiqueta: 'Crédito' },
  ];

  productos: Producto[] = [
    {
      id: 1,
      nombre: 'Terminal Integrada en...',
      descripcion: 'Impresión y Multifunciona...',
      precio: 3480,
      precioOriginal: 3480,
      cantidad: 0,
      imagen: 'img_temp/img_producto.png',
    },
    {
      id: 2,
      nombre: 'Terminal Integrada en...',
      descripcion: 'Impresión y Multifuncionales',
      precio: 3480,
      precioOriginal: 3480,
      cantidad: 0,
      imagen: 'img_temp/img_producto.png',
    },
    {
      id: 3,
      nombre: 'Terminal Integrada en...',
      descripcion: 'Impresión y Multifuncionales',
      precio: 3480,
      precioOriginal: 3480,
      cantidad: 0,
      imagen: 'img_temp/img_producto.png',
    },
    {
      id: 4,
      nombre: 'Terminal Integrada en...',
      descripcion: 'Impresión y Multifuncionales',
      precio: 3480,
      precioOriginal: 3480,
      cantidad: 0,
      imagen: 'img_temp/img_producto.png',
    },
    {
      id: 5,
      nombre: 'Terminal Integrada en...',
      descripcion: 'Impresión y Multifuncionales',
      precio: 3480,
      precioOriginal: 3480,
      cantidad: 0,
      imagen: 'img_temp/img_producto.png',
    },
  ];

  incrementar(producto: Producto): void {
    producto.cantidad++;
  } 

  decrementar(producto: Producto): void {
    if (producto.cantidad > 0) {
      producto.cantidad--;
    }
  }

  quitar(id: number): void {
    this.productos = this.productos.filter(p => p.id !== id);
  }

  get subtotal(): number {
    return this.productos.reduce(
      (acc, p) => acc + p.precio * Math.max(p.cantidad, 1),
      0
    );
  }

  get total(): number {
    return this.subtotal + 90;
  }

  seleccionarPago(tipo: TipoPago): void {
    this.tipoPagoSeleccionado = tipo;
  }

  siguiente(): void {
    console.log('Proceder al siguiente paso', {
      productos: this.productos,
      tipoPago: this.tipoPagoSeleccionado,
      total: this.total,
    });
  }

  formatearPrecio(valor: number): string {
    return `$${valor.toLocaleString('es-MX', { minimumFractionDigits: 2 })}`;
  }
}