// front-end/src/app/services/exel-api-base.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from './cookie.service'; 


// ============================================
// INTERFACES
// ============================================

export interface IProduct {
    id: string;
    referencia: string;
    sku: string;
    nombre: string;
    stock: string;
    precio: string;
    precio_oferta: string | null;
    precio_sin_oferta: string;
    oferta: boolean;
    moneda: string;
    marca_id: string;
    marca_nombre: string;
    subcategoria_id: string;
    subcategoria_nombre: string;
    categoria_id: string;
    categoria_nombre: string;
    imagen_url?: string;
}

export interface IImagenProducto {
    id: string;
    producto_id: string;
    url: string;
    principal: boolean;
    orden: number;
}

export interface IBrand {
    id: string;
    nombre: string;
    logo_url?: string;
    descripcion?: string;
}

export interface ICategory {
    id_categoria: string;
    nombre_categoria: string;
}

export interface ISubcategory {
    id_subcategoria: string;
    nombre_subcategoria: string;
    id_categoria?: string;
    nombre_categoria?: string;
}

export interface IApiResponse<T> {
    resultado: boolean;
    mensaje: string;
    datos: T;
}

// ============================================
// MODELOS
// ============================================

export class Brand {
    id: string;
    nombre: string;
    nombreNormalizado: string;
    slug: string;
    logoUrl?: string;
    descripcion?: string;

    constructor(data: IBrand) {
        this.id = data.id;
        this.nombre = data.nombre;
        this.nombreNormalizado = this.normalizarTexto(data.nombre);
        this.slug = this.generarSlug(data.nombre);
        this.logoUrl = data.logo_url;
        this.descripcion = data.descripcion;
    }

    private normalizarTexto(texto: string): string {
        return texto.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '').trim();
    }

    private generarSlug(texto: string): string {
        return texto.toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-|-$/g, '');
    }

    getIniciales(): string {
        return this.nombre.split(' ').map(p => p.charAt(0)).join('').toUpperCase().substring(0, 2);
    }

    toSelectOption(): { value: string; label: string } {
        return { value: this.id, label: this.nombre };
    }
}

export class Category {
    id: string;
    nombre: string;
    nombreNormalizado: string;
    slug: string;

    constructor(data: ICategory) {
        this.id = data.id_categoria;
        this.nombre = data.nombre_categoria;
        this.nombreNormalizado = this.normalizarTexto(data.nombre_categoria);
        this.slug = this.generarSlug(data.nombre_categoria);
    }

    private normalizarTexto(texto: string): string {
        return texto.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '').trim();
    }

    private generarSlug(texto: string): string {
        return texto.toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-|-$/g, '');
    }

    getNombreFormateado(): string {
        return this.nombre.split(' ').map(p => p.charAt(0).toUpperCase() + p.slice(1).toLowerCase()).join(' ');
    }

    toSelectOption(): { value: string; label: string } {
        return { value: this.id, label: this.nombre };
    }
}

export class Subcategory {
    id: string;
    nombre: string;
    categoriaId?: string;
    categoriaNombre?: string;
    nombreNormalizado: string;
    slug: string;

    constructor(data: ISubcategory) {
        this.id = data.id_subcategoria;
        this.nombre = data.nombre_subcategoria;
        this.categoriaId = data.id_categoria;
        this.categoriaNombre = data.nombre_categoria;
        this.nombreNormalizado = this.normalizarTexto(data.nombre_subcategoria);
        this.slug = this.generarSlug(data.nombre_subcategoria);
    }

    private normalizarTexto(texto: string): string {
        return texto.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '').trim();
    }

    private generarSlug(texto: string): string {
        return texto.toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-|-$/g, '');
    }

    getNombreCompleto(): string {
        return this.categoriaNombre ? `${this.categoriaNombre} > ${this.nombre}` : this.nombre;
    }

    perteneceACategoria(categoriaId: string): boolean {
        return this.categoriaId === categoriaId;
    }

    toSelectOption(): { value: string; label: string; group?: string } {
        return { value: this.id, label: this.nombre, group: this.categoriaNombre };
    }
}

// ============================================
// SERVICIO CON AUTENTICACIÓN
// ============================================

@Injectable({
    providedIn: 'root'
})
export class ExcelNorteCatalogoService {
    private http = inject(HttpClient);
    private cookieService = inject(CookieService); // Inyectar CookieService
    private baseUrl = environment.apiExcelUrl;
    private apiKey = environment.Authorization;

    private imagenesCache = new Map<string, string>();

    private async get<T>(endpoint: string): Promise<T> {
        const url = `${this.baseUrl}/${endpoint}`;
        console.log(`🌐 GET: ${url}`);
        
        // CAMBIADO: usar cookieService en lugar de localStorage
        const token = this.cookieService.getCookie('token');
        
        let headers = new HttpHeaders({
            'Content-Type': 'application/json'
        });
        
        if (token) {
            headers = headers.set('Authorization', `Bearer ${token}`);
            console.log('🔑 Usando token de autenticación desde cookie');
        } else if (this.apiKey) {
            headers = headers.set('Authorization', this.apiKey);
            console.log('🔑 Usando API key del environment');
        } else {
            console.warn('⚠️ No hay token ni API key disponible');
        }
        
        try {
            const response = await firstValueFrom(this.http.get<T>(url, { headers }));
            return response;
        } catch (error) {
            console.error(`❌ Error en ${url}:`, error);
            throw error;
        }
    }

    // ========== MÉTODOS DE IMÁGENES ==========
    
    async getImagenesProducto(productId: string): Promise<IImagenProducto[]> {
        try {
            const data = await this.get<IApiResponse<IImagenProducto[]>>(`imagenes/${productId}`);
            if (data?.resultado && data.datos) {
                console.log(`✅ ${data.datos.length} imágenes obtenidas para producto ${productId}`);
                return data.datos;
            }
            return [];
        } catch (error) {
            console.error(`Error fetching images for product ${productId}:`, error);
            return [];
        }
    }

    async getImagenPrincipal(productId: string): Promise<string | null> {
        if (this.imagenesCache.has(productId)) {
            return this.imagenesCache.get(productId) || null;
        }

        try {
            const imagenes = await this.getImagenesProducto(productId);
            const imagenPrincipal = imagenes.find(img => img.principal === true) || imagenes[0];
            
            if (imagenPrincipal) {
                this.imagenesCache.set(productId, imagenPrincipal.url);
                return imagenPrincipal.url;
            }
            return null;
        } catch (error) {
            console.error(`Error getting main image for product ${productId}:`, error);
            return null;
        }
    }

    // ========== PRODUCTOS ==========
    
    async getProducts(): Promise<IProduct[]> {
        try {
            const imagenesData = await this.get<any>('imagenes');
            
            if (!imagenesData?.DATA || imagenesData.DATA.length === 0) {
                console.warn('⚠️ No se encontraron imágenes');
                return [];
            }
            
            console.log(`✅ ${imagenesData.DATA.length} productos obtenidos desde 'imagenes'`);
            
            const products: IProduct[] = imagenesData.DATA.map((item: any) => {
                const marcaInfo = this.extraerMarcaInfo(item.referencia, item.sku);
                const precioAleatorio = Math.floor(Math.random() * 4900) + 100;
                const stockAleatorio = Math.floor(Math.random() * 50);
                const tieneOferta = Math.random() > 0.7;
                
                return {
                    id: item.referencia,
                    referencia: item.referencia,
                    sku: item.sku,
                    nombre: this.generarNombreProducto(item.sku, item.referencia),
                    stock: stockAleatorio.toString(),
                    precio: precioAleatorio.toString(),
                    precio_oferta: tieneOferta ? (precioAleatorio * 0.8).toString() : null,
                    precio_sin_oferta: "0",
                    oferta: tieneOferta,
                    moneda: "MXN",
                    marca_id: marcaInfo.id,
                    marca_nombre: marcaInfo.nombre,
                    subcategoria_id: "",
                    subcategoria_nombre: "",
                    categoria_id: "",
                    categoria_nombre: "",
                    imagenes: item.imagenes || [],
                    imagen_url: item.imagenes && item.imagenes.length > 0 ? item.imagenes[0] : undefined
                };
            });
            
            return products;
            
        } catch (error) {
            console.error('Error fetching products:', error);
            return [];
        }
    }

    async getProductById(id: string): Promise<IProduct | null> {
        try {
            const allProducts = await this.getProducts();
            const product = allProducts.find(p => p.id === id || p.referencia === id);
            
            if (product) {
                console.log(`✅ Producto encontrado: ${product.referencia}`);
                return product;
            }
            
            console.warn(`⚠️ Producto no encontrado: ${id}`);
            return null;
            
        } catch (error) {
            console.error(`Error fetching product ${id}:`, error);
            return null;
        }
    }

    // ========== MÉTODOS AUXILIARES ==========
    
    private extraerMarcaInfo(referencia: string, sku: string): { id: string; nombre: string } {
        const match = (referencia || sku).match(/^([A-Za-z0-9]+)/);
        const codigo = match ? match[1] : 'GEN';
        
        const marcas: Record<string, { id: string; nombre: string }> = {
            '3M': { id: '3M', nombre: '3M' },
            '3MP': { id: '3MP', nombre: '3M' },
            'ACP': { id: 'ACP', nombre: 'Acco' },
            'ADC': { id: 'ADC', nombre: 'Adata' },
            'ALP': { id: 'ALP', nombre: 'Alpe' },
            'AZP': { id: 'AZP', nombre: 'Azor' },
            'BAP': { id: 'BAP', nombre: 'Baco' },
            'BNE': { id: 'BNE', nombre: 'BenQ' },
            'BOC': { id: 'BOC', nombre: 'Brother' },
            'BRP': { id: 'BRP', nombre: 'Bostik' },
            'CAC': { id: 'CAC', nombre: 'Canon' },
            'DEC': { id: 'DEC', nombre: 'Dell' },
            'DEH': { id: 'DEH', nombre: 'Dell' },
            'HNV': { id: 'HNV', nombre: 'HP' },
            'HPC': { id: 'HPC', nombre: 'HP' },
            'HPH': { id: 'HPH', nombre: 'HP' },
            'HRT': { id: 'HRT', nombre: 'Honor' },
            'ITC': { id: 'ITC', nombre: 'Intcomex' },
            'KNC': { id: 'KNC', nombre: 'Kingston' },
            'LEC': { id: 'LEC', nombre: 'Lexmark' },
            'LGH': { id: 'LGH', nombre: 'LG' },
            'LNC': { id: 'LNC', nombre: 'Lenovo' },
            'LOC': { id: 'LOC', nombre: 'Logitech' },
            'MBP': { id: 'MBP', nombre: 'Maped' },
            'MNC': { id: 'MNC', nombre: '3M' },
            'MRT': { id: 'MRT', nombre: 'Motorola' },
            'OKC': { id: 'OKC', nombre: 'OKI' },
            'SAH': { id: 'SAH', nombre: 'Samsung' },
            'TLC': { id: 'TLC', nombre: 'TP-Link' },
            'TRV': { id: 'TRV', nombre: 'Tripp Lite' },
            'XIT': { id: 'XIT', nombre: 'Xiaomi' },
            'XEC': { id: 'XEC', nombre: 'Xerox' },
            'ZEP': { id: 'ZEP', nombre: 'Zebra' }
        };
        
        return marcas[codigo] || { id: 'GEN', nombre: 'General' };
    }

    private generarNombreProducto(sku: string, referencia: string): string {
        if (sku && sku.length > 0 && sku !== referencia) {
            return `Producto ${sku}`;
        }
        return `Producto ${referencia}`;
    }

    // ========== MARCAS ==========
    
    async getAllBrands(): Promise<Brand[]> {
        try {
            const response = await this.get<any>('marcas');
            
            console.log('📦 Respuesta de marcas:', response);
            
            if (response && response.resultado === true && response.datos && Array.isArray(response.datos)) {
                console.log(`✅ ${response.datos.length} marcas obtenidas`);
                
                return response.datos.map((brand: any) => new Brand({
                    id: brand.id,
                    nombre: brand.nombre,
                    logo_url: brand.logo_url,
                    descripcion: brand.descripcion
                }));
            }
            
            console.warn('⚠️ No se encontraron marcas en la respuesta');
            return [];
        } catch (error) {
            console.error('Error fetching brands:', error);
            return [];
        }
    }

    async getBrandById(id: string): Promise<Brand | null> {
        const brands = await this.getAllBrands();
        return brands.find(b => b.id === id) || null;
    }

    // ========== CATEGORÍAS ==========
    
// front-end/src/app/services/exel-api-base.service.ts

async getAllCategories(): Promise<Category[]> {
    try {
        const response = await this.get<any>('categorias');
        
        console.log('📦 Respuesta de categorías:', response);
        
        // Verificar la estructura de la respuesta
        if (response && response.resultado === true && response.datos && Array.isArray(response.datos)) {
            console.log(`✅ ${response.datos.length} categorías obtenidas`);
            
            // Mostrar primeras 5 categorías para debug
            console.log('📋 Ejemplo de categorías:', response.datos.slice(0, 5));
            
            return response.datos.map((cat: any) => new Category({
                id_categoria: cat.id,
                nombre_categoria: cat.nombre
            }));
        }
        
        console.warn('⚠️ No se encontraron categorías en la respuesta');
        return [];
    } catch (error) {
        console.error('Error fetching categories:', error);
        return [];
    }
}

    async getCategoryById(id: string): Promise<Category | null> {
        const categories = await this.getAllCategories();
        return categories.find(c => c.id === id) || null;
    }

    // ========== SUBCATEGORÍAS ==========
    
    async getAllSubcategories(): Promise<Subcategory[]> {
        try {
            const response = await this.get<any>('subcategorias');
            
            if (response && response.resultado === true && response.datos && Array.isArray(response.datos)) {
                console.log(`✅ ${response.datos.length} subcategorías obtenidas`);
                return response.datos.map((sub: any) => new Subcategory({
                    id_subcategoria: sub.id,
                    nombre_subcategoria: sub.nombre,
                    id_categoria: sub.id_categoria,
                    nombre_categoria: sub.nombre_categoria
                }));
            }
            return [];
        } catch (error) {
            console.error('Error fetching subcategories:', error);
            return [];
        }
    }

    async getSubcategoriesByCategory(categoryId: string): Promise<Subcategory[]> {
        const allSubcategories = await this.getAllSubcategories();
        return allSubcategories.filter(sub => sub.categoriaId === categoryId);
    }

    async getSubcategoryById(id: string): Promise<Subcategory | null> {
        const subcategories = await this.getAllSubcategories();
        return subcategories.find(sub => sub.id === id) || null;
    }

    // ========== MÉTODOS UTILITARIOS ==========
    
    async loadAllCatalogs(): Promise<{
        brands: Brand[];
        categories: Category[];
        subcategories: Subcategory[];
    }> {
        try {
            const [brands, categories, subcategories] = await Promise.all([
                this.getAllBrands(),
                this.getAllCategories(),
                this.getAllSubcategories()
            ]);
            
            console.log('✅ Catálogos cargados:', {
                marcas: brands.length,
                categorias: categories.length,
                subcategorias: subcategories.length
            });
            
            return { brands, categories, subcategories };
            
        } catch (error) {
            console.error('Error loading catalogs:', error);
            return { brands: [], categories: [], subcategories: [] };
        }
    }

    async getPriceRange(): Promise<{ min: number; max: number }> {
        const products = await this.getProducts();
        if (products.length === 0) return { min: 0, max: 100000 };
        
        const prices = products.map(p => parseFloat(p.precio)).filter(p => !isNaN(p));
        return {
            min: Math.min(...prices),
            max: Math.max(...prices)
        };
    }

    formatPrice(price: number | string): string {
        const num = typeof price === 'string' ? parseFloat(price) : price;
        if (isNaN(num)) return '$0.00';
        
        return new Intl.NumberFormat('es-MX', {
            style: 'currency',
            currency: 'MXN',
            minimumFractionDigits: 2
        }).format(num);
    }

    clearImageCache(): void {
        this.imagenesCache.clear();
        console.log('🗑️ Caché de imágenes limpiada');
    }
}