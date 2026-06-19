import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiBoton } from "../../../components/shared/boton/boton";
import { AuthLayout } from "../../../layouts/auth-layout/auth-layout";
import { UiIconComponent } from "../../../components/shared/icono/icono.component";

interface Producto {
  id: number;
  imagen: string;
  nombre: string;
  categoria: string;
  cantidad: number;
  precio: number;
  subtotal: number;
}

interface Direccion {
  id: number;
  nombre: string;
  calle: string;
  numeroExterior: string;
  numeroInterior: string;
  colonia: string;
  ciudad: string;
  estado: string;
  codigoPostal: string;
  pais: string;
  esPrincipal: boolean;
}

interface DireccionSucursal {
  nombre: string;
  calle: string;
  numero: string;
  colonia: string;
  ciudad: string;
  estado: string;
  codigoPostal: string;
  horario: string;
}

@Component({
  selector: 'app-orden-compra',
  standalone: true,
  imports: [CommonModule, FormsModule, UiBoton, AuthLayout, UiIconComponent],
  templateUrl: './orden-compra.html',
  styleUrl: './orden-compra.css',
})
export class PageOrdenCompra {
  // Paso actual: 1 = Carrito, 2 = Confirmación
  pasoActual: number = 1;

  // Tipo de entrega: 'domicilio' | 'sucursal'
  tipoEntrega: 'domicilio' | 'sucursal' = 'domicilio';

  // Estados 
  estadoCambiarTipoPago: boolean = false
  tipoPagoTemp: string = '';

  // Direcciones guardadas
  direcciones: Direccion[] = [
    {
      id: 1,
      nombre: 'Casa',
      calle: 'Av. Juárez',
      numeroExterior: '123',
      numeroInterior: 'B',
      colonia: 'Centro',
      ciudad: 'Guadalajara',
      estado: 'Jalisco',
      codigoPostal: '44100',
      pais: 'México',
      esPrincipal: true
    },
    {
      id: 2,
      nombre: 'Oficina',
      calle: 'Av. Vallarta',
      numeroExterior: '456',
      numeroInterior: '2',
      colonia: 'Americana',
      ciudad: 'Guadalajara',
      estado: 'Jalisco',
      codigoPostal: '44160',
      pais: 'México',
      esPrincipal: false
    }
  ];

  // Dirección seleccionada
  direccionSeleccionada: Direccion | null = this.direcciones.find(d => d.esPrincipal) || null;

  // Dirección de sucursal (fija)
  sucursal: DireccionSucursal = {
    nombre: 'Sucursal Centro',
    calle: 'Av. 16 de Septiembre',
    numero: '789',
    colonia: 'Centro Histórico',
    ciudad: 'Guadalajara',
    estado: 'Jalisco',
    codigoPostal: '44100',
    horario: 'Lunes a Viernes 9:00 AM - 8:00 PM, Sábado 10:00 AM - 6:00 PM'
  };

  // Arreglo de productos
  productos: Producto[] = [
    {
      id: 1,
      imagen: 'Imagen',
      nombre: 'Terminal Integrada en Caja...',
      categoria: 'Impresión y Multifuncionales',
      cantidad: 3,
      precio: 3480.00,
      subtotal: 10440.00
    },
    {
      id: 2,
      imagen: 'Imagen',
      nombre: 'Terminal Integrada en Caja...',
      categoria: 'Impresión y Multifuncionales',
      cantidad: 4,
      precio: 3480.00,
      subtotal: 13920.00
    },
    {
      id: 3,
      imagen: 'Imagen',
      nombre: 'Terminal Integrada en Caja...',
      categoria: 'Impresión y Multifuncionales',
      cantidad: 3,
      precio: 3480.00,
      subtotal: 10440.00
    }
  ];

  // Información de la orden
  orden: any = {
    productos: this.productos,
    tipoPago: 'transferencia',
    totalFlete: 150.00,
    subtotal: 0,
    total: 0
  };

  constructor() {
    this.calcularTotales();
  }

  calcularTotales() {
    this.productos.forEach(producto => {
      producto.subtotal = producto.precio * producto.cantidad;
    });
    
    this.orden.subtotal = this.productos.reduce((sum, p) => sum + p.subtotal, 0);
    this.orden.total = this.orden.subtotal + this.orden.totalFlete;
  }

  actualizarCantidad(producto: Producto, nuevaCantidad: number) {
    producto.cantidad = nuevaCantidad;
    this.calcularTotales();
  }

  eliminarProducto(id: number) {
    this.productos = this.productos.filter(p => p.id !== id);
    this.calcularTotales();
  }

  get totalProductos(): number {
    return this.productos.reduce((sum, p) => sum + p.cantidad, 0);
  }

  cambiarTipoPago(tipo: 'efectivo' | 'tarjeta' | 'credito' | 'transferencia') {
    this.orden.tipoPago = tipo;
  }

  cambiarTipoEntrega(tipo: 'domicilio' | 'sucursal') {
    this.tipoEntrega = tipo;
  }

  seleccionarDireccion(direccion: Direccion) {
    this.direccionSeleccionada = direccion;
  }

  // Navegación entre pasos
  irAlSiguientePaso() {
    if (this.pasoActual === 1) {
      this.pasoActual = 2;
    }
  }

  irAlPasoAnterior() {
    if (this.pasoActual === 2) {
      this.pasoActual = 1;
    }
  }

  confirmarPedido() {
    console.log('Pedido confirmado:', {
      productos: this.productos,
      tipoPago: this.orden.tipoPago,
      tipoEntrega: this.tipoEntrega,
      direccion: this.tipoEntrega === 'domicilio' ? this.direccionSeleccionada : this.sucursal
    });
    alert('¡Pedido confirmado con éxito!');
  }

  // Métodos para editar/eliminar dirección
  editarDireccion(direccion: Direccion) {
    console.log('Editar dirección:', direccion);
  }

  eliminarDireccion(direccion: Direccion) {
    this.direcciones = this.direcciones.filter(d => d.id !== direccion.id);
    if (this.direccionSeleccionada?.id === direccion.id) {
      this.direccionSeleccionada = this.direcciones.length > 0 ? this.direcciones[0] : null;
    }
  }

  agregarDireccion() {
    console.log('Agregar nueva dirección');
  }

  // Métodos para cambiar tipo de pago
  cambiarTipoPagoNuevo() {
    this.estadoCambiarTipoPago = true;
    this.tipoPagoTemp = this.orden.tipoPago;
  }

  cancelarCambioTipoPago() {
    this.estadoCambiarTipoPago = false;
    this.orden.tipoPago = this.tipoPagoTemp;
  }

  guardarCambioTipoPago() {
    this.estadoCambiarTipoPago = false;
    this.tipoPagoTemp = this.orden.tipoPago;
  }
}