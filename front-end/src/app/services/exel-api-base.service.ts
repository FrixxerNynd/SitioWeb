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
        
        // Agregar parámetros query string si existen
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
        // Verificar caché
        if (this.imagenesCache.has(claveProducto)) {
            console.log(`📦 Imagen en caché para: ${claveProducto}`);
            return this.imagenesCache.get(claveProducto) || null;
        }
        
        try {
            // Usar params correctamente
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

    // Obtener todos los productos (con imágenes)
    async getAllProducts(): Promise<IProduct[]> {
        try {
            const respuesta = await this.get<IApiResponse<any[]>>('productos');

            if (respuesta.resultado === true && respuesta.datos) {
                console.log(`📦 ${respuesta.datos.length} productos obtenidos`);

                const productos: IProduct[] = [];

                // Cargar productos uno por uno para mejor control
                for (const product of respuesta.datos) {                    
                    productos.push({
                        id: product.id,
                        referencia: product.referencia,
                        sku: product.sku,
                        nombre: product.nombre,
                        stock: product.stock,
                        precio: product.precio,
                        precio_oferta: product.precio_oferta,
                        precio_sin_oferta: product.precio_sin_oferta,
                        oferta: product.oferta,
                        moneda: product.moneda,
                        marca_id: product.marca_id,
                        marca_nombre: product.marca_nombre,
                        subcategoria_id: product.subcategoria_id || "",
                        subcategoria_nombre: product.subcategoria_nombre || "",
                        categoria_id: product.categoria_id || "",
                        categoria_nombre: product.categoria_nombre || "",
                    });
                }

                console.log(`✅ ${productos.length} productos cargados`);
                return productos;
            }

            return [];

        } catch (error) {
            console.error('Error al obtener los productos:', error);
            return [];
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

    // Formatear precio
    formatPrice(price: number | string): string {
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
}