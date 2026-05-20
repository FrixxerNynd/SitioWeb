// front-end/src/app/pages/usuario/pages/catalogo-producto/detalle-producto/detalle-producto.ts
import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ExcelNorteCatalogoService, IProduct } from '../../../../../services/exel-api-base.service';

@Component({
    selector: 'app-detalle-producto',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './detalle-producto.html',
})
export class PageDetalleProducto implements OnInit {
    private route = inject(ActivatedRoute);
    private catalogoService = inject(ExcelNorteCatalogoService);
    
    product = signal<IProduct | null>(null);
    isLoading = signal(true);
    error = signal<string | null>(null);
    selectedImageIndex = signal(0);
    
    // Para el carrusel de imágenes
    imagenesProducto = signal<string[]>([]);
    
    ngOnInit() {
        const productId = this.route.snapshot.paramMap.get('id');
        if (productId) {
            this.loadProduct(productId);
        } else {
            this.error.set('No se especificó un producto');
            this.isLoading.set(false);
        }
    }
    
    async loadProduct(id: string) {
        this.isLoading.set(true);
        try {
            const product = await this.catalogoService.getProductById(id);
            if (product) {
                this.product.set(product);
                // Cargar todas las imágenes del producto
                if (product.imagen_url) {
                    this.imagenesProducto.set([product.imagen_url]);
                }
                // Intentar cargar más imágenes si las hay
                const imagenes = await this.catalogoService.getImagenesProducto(product.id);
                if (imagenes && imagenes.length > 0) {
                    const urls = imagenes.map(img => img.url).filter(url => url);
                    if (urls.length > 0) {
                        this.imagenesProducto.set(urls);
                    }
                }
            } else {
                this.error.set('Producto no encontrado');
            }
        } catch (err) {
            console.error('Error loading product:', err);
            this.error.set('Error al cargar el producto');
        } finally {
            this.isLoading.set(false);
        }
    }
    
    cambiarImagen(index: number) {
        this.selectedImageIndex.set(index);
    }
    
    formatPrice(price: string | number): string {
        return this.catalogoService.formatPrice(price);
    }
    
    tieneOferta(product: IProduct): boolean {
        return !!product.precio_oferta && 
               parseFloat(product.precio_oferta) < parseFloat(product.precio);
    }
    
    getPorcentajeDescuento(product: IProduct): number {
        if (!this.tieneOferta(product)) return 0;
        const original = parseFloat(product.precio);
        const oferta = parseFloat(product.precio_oferta!);
        return Math.round(((original - oferta) / original) * 100);
    }
    
    agregarAlCarrito() {
        const product = this.product();
        if (product) {
            console.log('Producto agregado al carrito:', product);
            alert(`Producto "${product.nombre}" agregado al carrito`);
        }
    }
    
    getStockClass(stock: string): string {
        const stockNum = this.getStockNumber(stock);
        if (stockNum <= 0) return 'bg-danger';
        if (stockNum < 10) return 'bg-warning';
        return 'bg-success';
    }
    
    getStockText(stock: string): string {
        const stockNum = this.getStockNumber(stock);
        if (stockNum <= 0) return 'Agotado';
        if (stockNum < 10) return '¡Últimas unidades!';
        return 'En stock';
    }
    
    // 👈 NUEVOS MÉTODOS PARA MANEJAR STOCK
    getStockNumber(stock: string): number {
        return parseInt(stock) || 0;
    }
    
    isStockDisponible(stock: string): boolean {
        return this.getStockNumber(stock) > 0;
    }
    
    isStockBajo(stock: string): boolean {
        const stockNum = this.getStockNumber(stock);
        return stockNum > 0 && stockNum < 10;
    }
}