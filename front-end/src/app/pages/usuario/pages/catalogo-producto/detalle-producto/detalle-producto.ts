import { Component, signal } from '@angular/core';
import { UiBoton } from '../../../../../components/shared/boton/boton';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  imports: [UiBoton, CommonModule],
  templateUrl: './detalle-producto.html',
  styleUrl: './detalle-producto.css',
})
export class PageDetalleProducto {
  selectedImage: string = '';
  quantity: number = 1;
  activeTab: 'caracteristicas' | 'medidasDimensiones' = 'caracteristicas';
  
  // Signal para controlar el estado de carga
  isLoading = signal<boolean>(true);

  product = {
    name: 'Terminal Integrada en Caja A35 Soft Restaurant® Payments',
    category: 'Impresión y Multifuncionales',
    description: 'Lorem ipsum es un texto de relleno estándar utilizado en diseño gráfico, editorial y web para previsualizar maquetas y tipografías antes de insertar el contenido definitivo.',
    price: 3480,
    originalPrice: 3990,
    stock: 15,
    sku: 'TER-A35-001',
    satCode: '43211501',
    barcode: '750123456789',
    reference: 'REF-A35-2024',
    brand: 'Soft Restaurant',
    subcategory: 'Terminales de Pago',
    height: '18.5 cm',
    width: '12.3 cm',
    depth: '5.2 cm',
    weight: '450 g',
    weightUnit: 'gramos',
    volume: '1.2 L',
    volumeUnit: 'litros',
    images: [
      'https://placehold.co/600x400/1e3a5f/white?text=Terminal+A35',
      'https://placehold.co/600x400/2c7da0/white?text=Vista+Posterior',
      'https://placehold.co/600x400/61a5c2/white?text=Vista+Lateral',
      'https://placehold.co/600x400/89c2d9/white?text=Interfaz'
    ]
  };

  constructor() {
    this.selectedImage = this.product.images[0];
    
    // Simular carga de datos (elimina esto cuando tengas datos reales)
    setTimeout(() => {
      this.isLoading.set(false);
    }, 20000);
  }

  selectImage(image: string): void {
    this.selectedImage = image;
  }

  addToCart(): void {
    console.log('Agregado al carrito:', {
      product: this.product.name,
      quantity: this.quantity,
      price: this.product.price
    });
    alert(`✅ ${this.quantity} unidad(es) agregadas al carrito`);
  }

  buyNow(): void {
    console.log('Comprar ahora:', {
      product: this.product.name,
      quantity: this.quantity
    });
    alert(`🛒 Procediendo con la compra de ${this.quantity} unidad(es)`);
  }
}