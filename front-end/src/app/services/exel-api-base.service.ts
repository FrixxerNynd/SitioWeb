// front-end/src/app/services/exel-api-base.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from './cookie.service';
import { IProduct, IBrand, ICategory, IApiResponse } from '../interfaces/interface-excel-norte/excel-norte-interface';

@Injectable({
    providedIn: 'root'
})
export class ExcelNorteCatalogoService {
    private http = inject(HttpClient);
    private cookieService = inject(CookieService);
    private baseUrl = environment.apiExcelUrl;
    private apiKey = environment.Authorization;

    private imagenesCache = new Map<string, string>();

    // Método GET con soporte para parámetros
    private async get<T>(endpoint: string, params?: { [key: string]: string }): Promise<T> {
        let url = `${this.baseUrl}/${endpoint}`;
        
        if (params) {
            const queryParams = new URLSearchParams();
            Object.keys(params).forEach(key => {
                if (params[key]) {
                    queryParams.append(key, params[key]);
                }
            });
            if (queryParams.toString()) {
                url += `?${queryParams.toString()}`;
            }
        }
        
        console.log(`🌐 GET: ${url}`);

        const token = this.cookieService.getCookie('token');

        let headers = new HttpHeaders({
            'Content-Type': 'application/json'
        });

        if (token) {
            headers = headers.set('Authorization', `Bearer ${token}`);
        } else if (this.apiKey) {
            headers = headers.set('Authorization', this.apiKey);
        }

        try {
            const response = await firstValueFrom(this.http.get<T>(url, { headers }));
            return response;
        } catch (error) {
            console.error(`❌ Error en ${url}:`, error);
            throw error;
        }
    }

    // Obtener todas las categorías
    async getAllCategories(): Promise<ICategory[]> {
        try {
            const respuesta = await this.get<IApiResponse<any[]>>('categorias');

            if (respuesta.resultado === true && respuesta.datos) {
                console.log(`✅ ${respuesta.datos.length} categorías obtenidas`);

                const categorias: ICategory[] = [];

                for (const cat of respuesta.datos) {
                    categorias.push({
                        id_categoria: cat.id_categoria,
                        nombre_categoria: cat.nombre_categoria
                    });
                }

                return categorias;
            }

            return [];

        } catch (error) {
            console.error('Error al obtener las categorias', error);
            return [];
        }
    }

    // Obtener todas las marcas
    async getAllBrands(): Promise<IBrand[]> {
        try {
            const respuesta = await this.get<IApiResponse<any[]>>('marcas');

            if (respuesta.resultado === true && respuesta.datos) {
                console.log(`✅ ${respuesta.datos.length} marcas obtenidas`);

                const marcas: IBrand[] = [];

                for (const brand of respuesta.datos) {
                    marcas.push({
                        id: brand.id,
                        nombre: brand.nombre,
                        logo_url: brand.logo_url,
                        descripcion: brand.descripcion
                    });
                }

                return marcas;
            }

            return [];

        } catch (error) {
            console.error('Error al obtener las marcas', error);
            return [];
        }
    }

    // Obtener imagen de un producto (con caché)
    async getImagenProducto(claveProducto: string): Promise<string | null> {
        if (this.imagenesCache.has(claveProducto)) {
            console.log(`📦 Imagen en caché para: ${claveProducto}`);
            return this.imagenesCache.get(claveProducto) || null;
        }
        
        try {
            const respuesta = await this.get<any>('imagenes', { clave_producto: claveProducto });

            if (respuesta?.RESULT && respuesta.DATA && respuesta.DATA.length > 0) {
                const urlImagen = respuesta.DATA[0].url_imagen;
                this.imagenesCache.set(claveProducto, urlImagen);
                console.log(`✅ Imagen obtenida para: ${claveProducto}`);
                return urlImagen;
            }
            console.log(`⚠️ Sin imagen para: ${claveProducto}`);
            return null;
        } catch (error) {
            console.error(`Error al obtener imagen de ${claveProducto}:`, error);
            return null;
        }
    }

    // Obtener todas las imágenes de un producto
    async getImagenesProducto(claveProducto: string): Promise<string[]> {
        try {
            const respuesta = await this.get<any>('imagenes', { clave_producto: claveProducto });

            if (respuesta?.RESULT && respuesta.DATA && respuesta.DATA.length > 0) {
                const urls = respuesta.DATA.map((img: any) => img.url_imagen);
                console.log(`✅ ${urls.length} imágenes obtenidas para ${claveProducto}`);
                return urls;
            }
            return [];
        } catch (error) {
            console.error(`Error al obtener imágenes de ${claveProducto}:`, error);
            return [];
        }
    }

    // Obtener todas las imágenes de múltiples productos (optimizado)
    async getImagenesMultiplesProductos(clavesProducto: string[]): Promise<Map<string, string[]>> {
        const imagenesMap = new Map<string, string[]>();
        
        const batchSize = 10;
        for (let i = 0; i < clavesProducto.length; i += batchSize) {
            const batch = clavesProducto.slice(i, i + batchSize);
            const promises = batch.map(clave => this.getImagenesProducto(clave));
            const resultados = await Promise.all(promises);
            
            batch.forEach((clave, index) => {
                imagenesMap.set(clave, resultados[index]);
            });
        }
        
        return imagenesMap;
    }

    // Obtener todas las imágenes principales de productos (optimizado)
    async getImagenesPrincipalesMultiples(clavesProducto: string[]): Promise<Map<string, string | null>> {
        const imagenesMap = new Map<string, string | null>();
        
        const batchSize = 10;
        for (let i = 0; i < clavesProducto.length; i += batchSize) {
            const batch = clavesProducto.slice(i, i + batchSize);
            const promises = batch.map(clave => this.getImagenProducto(clave));
            const resultados = await Promise.all(promises);
            
            batch.forEach((clave, index) => {
                imagenesMap.set(clave, resultados[index]);
            });
        }
        
        return imagenesMap;
    }

    // Obtener todos los productos (con imágenes optimizado)
    async getAllProducts(includeImages: boolean = true): Promise<IProduct[]> {
        try {
            const respuesta = await this.get<IApiResponse<any[]>>('productos');

            if (respuesta.resultado === true && respuesta.datos) {
                console.log(`📦 ${respuesta.datos.length} productos obtenidos`);

                const productos: IProduct[] = [];

                // Crear los productos base sin imágenes (todos como strings)
                for (const product of respuesta.datos) {                    
                    productos.push({
                        id: String(product.id),
                        referencia: product.referencia,
                        sku: product.sku,
                        nombre: product.nombre,
                        stock: String(product.stock),
                        precio: String(product.precio),
                        precio_oferta: product.precio_oferta ? String(product.precio_oferta) : null,
                        precio_sin_oferta: String(product.precio_sin_oferta || product.precio),
                        oferta: Boolean(product.oferta),
                        moneda: product.moneda,
                        marca_id: String(product.marca_id),
                        marca_nombre: product.marca_nombre,
                        subcategoria_id: String(product.subcategoria_id || ""),
                        subcategoria_nombre: product.subcategoria_nombre || "",
                        categoria_id: String(product.categoria_id || ""),
                        categoria_nombre: product.categoria_nombre || "",
                        imagenes: [],
                        imagen_principal: null
                    });
                }

                if (includeImages && productos.length > 0) {
                    console.log('Cargando imágenes para todos los productos...');
                    
                    const referencias = productos.map(p => p.referencia || p.sku).filter(ref => ref);
                    
                    const imagenesPrincipales = await this.getImagenesPrincipalesMultiples(referencias);
                    const todasImagenes = await this.getImagenesMultiplesProductos(referencias);
                    
                    for (const producto of productos) {
                        const clave = producto.referencia || producto.sku;
                        if (clave) {
                            producto.imagen_principal = imagenesPrincipales.get(clave) || null;
                            producto.imagenes = todasImagenes.get(clave) || [];
                            
                            if (producto.imagen_principal) {
                                console.log(`✅ Imagen cargada para: ${producto.nombre}`);
                            }
                        }
                    }
                    
                    console.log(`✅ Imágenes cargadas para ${productos.length} productos`);
                }

                return productos;
            }

            return [];

        } catch (error) {
            console.error('Error al obtener los productos:', error);
            return [];
        }
    }

    // Obtener productos por categoría con imágenes
    async getProductsByCategory(categoriaId: string, includeImages: boolean = true): Promise<IProduct[]> {
        const allProducts = await this.getAllProducts(false);
        // Comparación directa de strings
        const filteredProducts = allProducts.filter(p => p.categoria_id === categoriaId);
        
        if (includeImages && filteredProducts.length > 0) {
            const referencias = filteredProducts.map(p => p.referencia || p.sku).filter(ref => ref);
            const imagenesPrincipales = await this.getImagenesPrincipalesMultiples(referencias);
            const todasImagenes = await this.getImagenesMultiplesProductos(referencias);
            
            for (const producto of filteredProducts) {
                const clave = producto.referencia || producto.sku;
                if (clave) {
                    producto.imagen_principal = imagenesPrincipales.get(clave) || null;
                    producto.imagenes = todasImagenes.get(clave) || [];
                }
            }
        }
        
        return filteredProducts;
    }

    // Obtener productos por marca con imágenes
    async getProductsByBrand(marcaId: string, includeImages: boolean = true): Promise<IProduct[]> {
        const allProducts = await this.getAllProducts(false);
        // Comparación directa de strings
        const filteredProducts = allProducts.filter(p => p.marca_id === marcaId);
        
        if (includeImages && filteredProducts.length > 0) {
            const referencias = filteredProducts.map(p => p.referencia || p.sku).filter(ref => ref);
            const imagenesPrincipales = await this.getImagenesPrincipalesMultiples(referencias);
            const todasImagenes = await this.getImagenesMultiplesProductos(referencias);
            
            for (const producto of filteredProducts) {
                const clave = producto.referencia || producto.sku;
                if (clave) {
                    producto.imagen_principal = imagenesPrincipales.get(clave) || null;
                    producto.imagenes = todasImagenes.get(clave) || [];
                }
            }
        }
        
        return filteredProducts;
    }

    // Obtener un solo producto con sus imágenes
    async getProductById(productId: string): Promise<IProduct | null> {
        try {
            // Obtener todos los productos (sin imágenes para optimizar)
            const allProducts = await this.getAllProducts(false);
            // Comparación directa de strings
            const product = allProducts.find(p => p.id === productId);
            
            if (product) {
                const clave = product.referencia || product.sku;
                if (clave) {
                    // Cargar imágenes solo para este producto
                    product.imagen_principal = await this.getImagenProducto(clave);
                    product.imagenes = await this.getImagenesProducto(clave);
                }
                return product;
            }
            
            return null;
        } catch (error) {
            console.error(`Error al obtener producto ${productId}:`, error);
            return null;
        }
    }

    // Formatear precio (conversión a número solo para formato)
    formatPrice(price: string | number): string {
        const num = typeof price === 'string' ? parseFloat(price) : price;
        if (isNaN(num)) return '$0.00';
        
        return new Intl.NumberFormat('es-MX', {
            style: 'currency',
            currency: 'MXN',
            minimumFractionDigits: 2
        }).format(num);
    }

    // Limpiar caché de imágenes
    clearImageCache(): void {
        this.imagenesCache.clear();
        console.log('🗑️ Caché de imágenes limpiada');
    }

    // Precargar imágenes para productos visibles
    async preloadImagesForProducts(productos: IProduct[]): Promise<void> {
        const referencias = productos
            .map(p => p.referencia || p.sku)
            .filter(ref => ref && !this.imagenesCache.has(ref));
        
        if (referencias.length > 0) {
            console.log(`🖼️ Precargando ${referencias.length} imágenes...`);
            await this.getImagenesPrincipalesMultiples(referencias);
        }
    }
}