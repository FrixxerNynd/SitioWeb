import { Component, signal, OnInit } from '@angular/core';
import { UiBoton } from '../../../../../components/shared/boton/boton';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ExcelNorteCatalogoService } from '../../../../../services/exel-api-base.service';

@Component({
  imports: [UiBoton, CommonModule],
  templateUrl: './detalle-producto.html',
  styleUrl: './detalle-producto.css',
})
export class PageDetalleProducto implements OnInit {
  selectedImage: string = '';
  quantity: number = 1;
  activeTab: 'caracteristicas' | 'medidasDimensiones' = 'caracteristicas';

  // Signal para controlar el estado de carga
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

  constructor(
    private route: ActivatedRoute,
    private exelService: ExcelNorteCatalogoService,
  ) {}

  ngOnInit(): void {
    // Obtenemos el id (referencia) desde la URL
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.cargarProducto(id);
    } else {
      this.isLoading.set(false);
    }
  }

  async cargarProducto(referencia: string) {
    this.isLoading.set(true);
    try {
      const p = await this.exelService.getProductByReference(referencia);
      const m = await this.exelService.getMedidaProducto(referencia);
      if (p) {
        // Mapeamos los datos reales del producto a las variables de la vista
        this.product = {
          ...this.product,
          name: p.nombre,
          description: p.descripcion,
          category: p.categoria_nombre || p.categoria_id || 'Sin categoría',
          price: Number(p.precio) || 0,
          originalPrice: Number(p.precio_sin_oferta) || Number(p.precio) || 0,
          stock: Number(p.stock) || 0,
          satCode: p.codigoSat,
          barCode: p.codigoBarras,
          sku: p.sku,
          reference: p.referencia,
          brand: p.marca_nombre || p.marca_id || '',
          subcategory: p.subcategoria_nombre || p.subcategoria_id || 'Sin subcategoría',
          // Usar imagen_principal si existe, o imagenes, de lo contrario un placeholder
          images:
            p.imagenes && p.imagenes.length > 0
              ? p.imagenes
              : p.imagen_principal
                ? [p.imagen_principal]
                : ['https://placehold.co/600x400/eeeeee/888888?text=Sin+Imagen'],
        };
        if (m) {
          //Mapeo de datos faltantes de las medidas
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
        console.log(this.product);
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

  addToCart(): void {
    console.log('Agregado al carrito:', {
      product: this.product.name,
      quantity: this.quantity,
      price: this.product.price,
    });
    alert(`✅ ${this.quantity} unidad(es) agregadas al carrito`);
  }

  buyNow(): void {
    console.log('Comprar ahora:', {
      product: this.product.name,
      quantity: this.quantity,
    });
    alert(`🛒 Procediendo con la compra de ${this.quantity} unidad(es)`);
  }
}
