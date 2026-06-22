// front-end/src/app/pages/usuario/pages/catalogo-producto/detalle-producto/detalle-producto.ts

import { Component, signal, OnInit, inject } from '@angular/core';
import { UiBoton } from '../../../../../components/shared/boton/boton';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ExcelNorteCatalogoService } from '../../../../../services/exel-api-base.service';
import { CartService } from '../../../../../services/cart.service';
import { UiIconComponent } from '../../../../../components/shared/icono/icono.component';

@Component({
  imports: [UiBoton, CommonModule, FormsModule, UiIconComponent],
  templateUrl: './detalle-producto.html',
  styleUrl: './detalle-producto.css',
})
export class PageDetalleProducto implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private exelService = inject(ExcelNorteCatalogoService);
  private cartService = inject(CartService);

  selectedImage: string = '';
  quantity: number = 1;
  activeTab: 'caracteristicas' | 'medidasDimensiones' = 'caracteristicas';
  isLoading = signal<boolean>(true);

  product: any = {
    name: 'Cargando...',
    category: '',
    description: '',
    price: 0,
    originalPrice: 0,
    stock: 0,
    sku: '',
    satCode: '',
    barcode: '',
    reference: '',
    brand: '',
    subcategory: '',
    height: '',
    width: '',
    depth: '',
    weight: '',
    weightUnit: '',
    volume: '',
    volumeUnit: '',
    images: [],
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.cargarProducto(id);
    } else {
      this.isLoading.set(false);
    }
  }

  // Función que mapea el ID de categoría a su nombre
  getCategoryName(categoriaId: string): string {
    if (!categoriaId) return 'Sin categoría';

    const categoryMap: { [key: string]: string } = {
      '1': 'Cómputo',
      '2': 'Impresión y Multifuncionales',
      '3': 'Consumibles',
      '4': 'Almacenamiento',
      '5': 'Electrónica de Consumo',
      '6': 'Cámara Video y Proyección',
      '8': 'Audio y Entretenimiento',
      '9': 'Redes',
      '10': 'Software y Garantías',
      '11': 'Energía y Cables',
      '14': 'Telefonía',
      '15': 'Servidores y Almacenamiento',
      '16': 'Papel',
      '17': 'Oficina y Escolar',
      '18': 'Puntos de Venta',
      '19': 'Videovigilancia',
      '20': 'Telefonía Empresarial',
      '21': 'Seguridad',
      '23': 'Digitalización de Documentos'
    };

    return categoryMap[categoriaId] || `Categoría ${categoriaId}`;
  }

  async cargarProducto(referencia: string) {
    this.isLoading.set(true);
    try {
      const p = await this.exelService.getProductByReference(referencia);
      const m = await this.exelService.getMedidaProducto(referencia);
      
      if (p) {
        const categoriaId = p.categoria_id || '';
        const categoriaNombre = this.getCategoryName(categoriaId);
        
        console.log(`🔍 Categoría ID: ${categoriaId} → Nombre: ${categoriaNombre}`);
        
        this.product = {
          ...this.product,
          name: p.nombre || 'Producto sin nombre',
          description: p.descripcion || '',
          category: categoriaNombre,
          categoryId: categoriaId,
          price: Number(p.precio) || 0,
          originalPrice: Number(p.precio_sin_oferta) || Number(p.precio) || 0,
          stock: Number(p.stock) || 0,
          satCode: p.codigoSat || '',
          barCode: p.codigoBarras || '',
          sku: p.sku || '',
          reference: p.referencia || '',
          brand: p.marca_nombre || p.marca_id || '',
          subcategory: p.subcategoria_nombre || p.subcategoria_id || 'Sin subcategoría',
          images:
            p.imagenes && p.imagenes.length > 0
              ? p.imagenes
              : p.imagen_principal
                ? [p.imagen_principal]
                : ['https://placehold.co/600x400/eeeeee/888888?text=Sin+Imagen'],
        };
        
        if (m) {
          this.product = {
            ...this.product,
            height: m.altura || 'N/A',
            width: m.ancho || 'N/A',
            depth: m.largo || 'N/A',
            weight: m.peso || 'N/A',
            weightUnit: m.medida_peso || 'N/A',
            volume: m.volumen || 'N/A',
            volumeUnit: m.medida_volumen || 'N/A',
          };
        }
        this.selectedImage = this.product.images[0];
        console.log('📦 Producto cargado:', this.product);
      } else {
        console.warn('No se encontró el producto o la respuesta fue vacía.');
        this.product.name = 'Producto no encontrado';
      }
    } catch (error) {
      console.error('Error al cargar los datos del producto:', error);
      this.product.name = 'Error al cargar el producto';
    } finally {
      this.isLoading.set(false);
    }
  }

  selectImage(image: string): void {
    this.selectedImage = image;
  }

  actualizarCantidad(nuevaCantidad: number): void {
    this.quantity = Math.max(1, nuevaCantidad);
    console.log('📦 Cantidad actualizada:', this.quantity);
  }

  /**
   * 🔥 Agregar producto al carrito o actualizar cantidad si ya existe
   */
  async addToCart(): Promise<void> {
    if (!this.product.reference) {
      console.error('Error: Producto sin referencia', this.product);
      alert('Error: El producto no tiene una referencia válida');
      return;
    }

    this.quantity = Math.max(1, this.quantity);

    if (this.product.stock < this.quantity) {
      alert(`Stock insuficiente. Solo quedan ${this.product.stock} unidades disponibles.`);
      return;
    }

    try {
      console.log('🛒 Agregando/Actualizando al carrito:', {
        reference: this.product.reference,
        quantity: this.quantity,
        name: this.product.name,
        price: this.product.price,
        stock: this.product.stock,
        sku: this.product.sku,
        category: this.product.category
      });

      // 🔥 PRIMERO: Verificar si el producto ya está en el carrito
      const cartResponse = await this.cartService.getCart();
      console.log('📦 Carrito actual:', cartResponse);

      if (cartResponse.success && cartResponse.data) {
        const items = cartResponse.data.items || [];
        
        // Buscar si el producto ya existe en el carrito
        const existingItem = items.find(item => item.productId === this.product.reference);
        
        if (existingItem) {
          // 🔥 Si existe, actualizar la cantidad usando PUT
          console.log(`🔄 Producto "${this.product.name}" ya existe en el carrito. Actualizando cantidad...`);
          
          const updateResult = await this.cartService.updateItemQuantity(
            this.product.reference,
            this.quantity
          );
          
          console.log('✅ Cantidad actualizada:', updateResult);
          alert(`Cantidad de "${this.product.name}" actualizada a ${this.quantity} unidad(es)`);
        } else {
          // 🔥 Si no existe, agregar como nuevo usando POST
          console.log(`➕ Producto "${this.product.name}" no existe en el carrito. Agregando...`);
          
          const addResult = await this.cartService.addItemFull({
            productId: this.product.reference,
            quantity: this.quantity,
            price: this.product.price,
            stock: this.product.stock,
            name: this.product.name,
            sku: this.product.sku
          });
          
          console.log('✅ Producto agregado:', addResult);
          alert(`${this.quantity} unidad(es) de "${this.product.name}" agregadas al carrito`);
        }
      }
    } catch (error: any) {
      console.error('❌ Error al agregar/actualizar al carrito:', error);
      let mensajeError = 'Error al agregar el producto al carrito';
      if (error.error?.message) {
        mensajeError = error.error.message;
      } else if (error.message) {
        mensajeError = error.message;
      }
      alert(`${mensajeError}`);
    }
  }

  async buyNow(): Promise<void> {
    if (!this.product.reference) {
      alert('Error: Producto sin referencia');
      return;
    }

    this.quantity = Math.max(1, this.quantity);

    if (this.product.stock < this.quantity) {
      alert(`Stock insuficiente. Solo quedan ${this.product.stock} unidades disponibles.`);
      return;
    }

    try {
      // Primero verificar si ya existe en el carrito
      const cartResponse = await this.cartService.getCart();
      
      if (cartResponse.success && cartResponse.data) {
        const items = cartResponse.data.items || [];
        const existingItem = items.find(item => item.productId === this.product.reference);
        
        if (existingItem) {
          // Actualizar cantidad
          await this.cartService.updateItemQuantity(this.product.reference, this.quantity);
          console.log('✅ Cantidad actualizada para compra rápida');
        } else {
          // Agregar nuevo
          await this.cartService.addItemFull({
            productId: this.product.reference,
            quantity: this.quantity,
            price: this.product.price,
            stock: this.product.stock,
            name: this.product.name,
            sku: this.product.sku
          });
          console.log('✅ Producto agregado para compra rápida');
        }
      }
      
      // Navegar al checkout
      this.router.navigate(['/orden-compra/comprar']);
    } catch (error: any) {
      console.error('Error al agregar al carrito:', error);
      let mensajeError = 'Error al agregar el producto al carrito';
      if (error.error?.message) {
        mensajeError = error.error.message;
      }
      alert(`${mensajeError}`);
    }
  }
}