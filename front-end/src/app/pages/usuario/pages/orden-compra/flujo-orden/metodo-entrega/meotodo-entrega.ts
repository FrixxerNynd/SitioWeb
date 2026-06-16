import { Component } from '@angular/core';
import { UiIconComponent } from '../../../../../../components/shared/icono/icono.component';
import { CommonModule } from '@angular/common';
import { UiBoton } from '../../../../../../components/shared/boton/boton';

interface Locales {
  id: number;
  nombre: string;
  calle: string;
  colonia: string;
  estado: string;
  n_exterior: number;
  ciudad: string;
  codigo_postal: string;
  n_interior: string;
  pais: string;
}
interface Producto {
  id: number;
  nombre: string;
  descripcion: string;
  precio: number;
  precioOriginal: number;
  cantidad: number;
  imagen: string;
}

@Component({
  selector: 'app-meotodo-entrega',
  imports: [UiIconComponent, CommonModule,UiBoton],
  templateUrl: './meotodo-entrega.html',
  styleUrl: './meotodo-entrega.css',
})
export class PageMeotodoEntrega {
  tipoEntrega: 'domicilio' | 'sucursal' = 'domicilio';
  direccionSeleccionadaId: number = 1;

  // 3 Ejemplos básicos para entrega a domicilio (Casas / Oficinas de usuarios)
  direccionesDomicilio: Locales[] = [
    {
      id: 1,
      nombre: "Mi Casa",
      calle: "Av. Universidad",
      colonia: "Clavería",
      estado: "Ciudad de México",
      n_exterior: 410,
      ciudad: "Azcapotzalco",
      codigo_postal: "02080",
      n_interior: "Dep. 302",
      pais: "México"
    },
    {
      id: 2,
      nombre: "Oficina Trabajo",
      calle: "Paseo de la Reforma",
      colonia: "Juárez",
      estado: "Ciudad de México",
      n_exterior: 250,
      ciudad: "Cuauhtémoc",
      codigo_postal: "06600",
      n_interior: "Piso 12",
      pais: "México"
    },
    {
      id: 3,
      nombre: "Casa de mis papás",
      calle: "Calle de las Flores",
      colonia: "Jardines del Valle",
      estado: "Jalisco",
      n_exterior: 89,
      ciudad: "Zapopan",
      codigo_postal: "45138",
      n_interior: "-",
      pais: "México"
    }
  ];

  // 3 Ejemplos básicos para puntos de retiro (Sucursales físicas / Bodegas de CABS)
  sucursalesDisponibles: Locales[] = [
    {
      id: 101, // IDs distintos para no cruzarse con domicilio
      nombre: "CABS - Sucursal",
      calle: "Beatriz Prada",
      colonia: "Col. Jardines de Durango",
      estado: "Durango",
      n_exterior: 450,
      ciudad: "Victoria de Durango",
      codigo_postal: "34020",
      n_interior: "-",
      pais: "México"
    },
    
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
  formatearPrecio(valor: number): string {
    return `$${valor.toLocaleString('es-MX', { minimumFractionDigits: 2 })}`;
  }

  // Función auxiliar para retornar los datos correctos en el *ngFor sin alterar el HTML
  get locales(): Locales[] {
    return this.tipoEntrega === 'domicilio' 
      ? this.direccionesDomicilio 
      : this.sucursalesDisponibles;
  }

  
  quitar(id: number): void {
    this.productos = this.productos.filter(p => p.id !== id);
  }

}