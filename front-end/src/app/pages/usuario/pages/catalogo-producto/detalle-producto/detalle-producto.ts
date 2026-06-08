import { Component, signal} from '@angular/core';
import { UiBoton } from '../../../../../components/shared/boton/boton';

@Component({
  templateUrl: './detalle-producto.html',
  styleUrl: './detalle-producto.css',
})
export class  PageDetalleProducto {
  selectedImage: string = '';
  quantity: number = 1;
  activeTab: 'features' | 'specs' = 'features';

  product = {
    name: 'Terminal Integrada en Caja A35 Soft Restaurant® Payments',
    category: 'Impresión y Multifuncionales',
    description: 'Lorem ipsum es un texto de relleno estándar utilizado en diseño gráfico, editorial y web para previsualizar maquetas y tipografías antes de insertar el contenido definitivo.',
    price: 3480,
    originalPrice: 3990,
    stock: 15,
    images: [
      'https://placehold.co/600x400/1e3a5f/white?text=Terminal+A35',
      'https://placehold.co/600x400/2c7da0/white?text=Vista+Posterior',
      'https://placehold.co/600x400/61a5c2/white?text=Vista+Lateral',
      'https://placehold.co/600x400/89c2d9/white?text=Interfaz'
    ],
    features: [
      'Pantalla táctil de 7 pulgadas HD',
      'Procesador Quad-core 1.8 GHz',
      'Conectividad WiFi y Bluetooth 5.0',
      'Soporta pagos contactless',
      'Batería de respaldo de 4 horas',
      'Compatible con todos los bancos principales'
    ],
    specifications: {
      'Dimensiones': '18.5 x 12.3 x 5.2 cm',
      'Peso': '450 gramos',
      'Material': 'Policarbonato reforzado',
      'Puertos': 'USB-C, RJ45, USB 3.0',
      'Temperatura operativa': '-10°C a 45°C',
      'Garantía': '18 meses'
    }
  };

  constructor() {
    this.selectedImage = this.product.images[0];
  }

  selectImage(image: string): void {
    this.selectedImage = image;
  }

  incrementQuantity(): void {
    if (this.quantity < this.product.stock) {
      this.quantity++;
    }
  }

  decrementQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
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


