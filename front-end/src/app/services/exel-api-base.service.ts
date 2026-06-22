// front-end/src/app/services/exel-api-base.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from './cookie.service';
import {
  IProductosPageResponse,
  IProduct,
  IBrand,
  ICategory,
} from '../interfaces/interface-excel-norte/excel-norte-interface';

@Injectable({
  providedIn: 'root',
})
export class ExcelNorteCatalogoService {
  private http = inject(HttpClient);
  private cookieService = inject(CookieService);
  private baseUrl = environment.backendUrl;
  private apiKey = environment.Authorization;

  private imagenesCache = new Map<string, string>();

  // Método GET con soporte para parámetros
  private async get<T>(endpoint: string, params?: { [key: string]: string }): Promise<T> {
    let url = `${this.baseUrl}/${endpoint}`;

    if (params) {
      const queryParams = new URLSearchParams();
      Object.keys(params).forEach((key) => {
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
      'Content-Type': 'application/json',
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

  // Método POST con soporte para body + query params
  private async post<T>(endpoint: string, body: any, params?: { [key: string]: string }): Promise<T> {
    let url = `${this.baseUrl}/${endpoint}`;

    if (params) {
      const queryParams = new URLSearchParams();
      Object.keys(params).forEach((key) => {
        if (params[key]) {
          queryParams.append(key, params[key]);
        }
      });
      if (queryParams.toString()) {
        url += `?${queryParams.toString()}`;
      }
    }

    console.log(`🌐 POST: ${url}`, body);

    const token = this.cookieService.getCookie('token');

    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    } else if (this.apiKey) {
      headers = headers.set('Authorization', this.apiKey);
    }

    try {
      const response = await firstValueFrom(this.http.post<T>(url, body, { headers }));
      return response;
    } catch (error) {
      console.error(`❌ Error en POST ${url}:`, error);
      throw error;
    }
  }

  // Obtener todas las categorías
  async getAllCategories(): Promise<ICategory[]> {
    try {
      const respuesta = await this.get<{ success: boolean; data: any }>('productos/categorias');

      if (respuesta.success && respuesta.data) {
        console.log(`✅ ${respuesta.data.length} categorías obtenidas`);

        const categorias: ICategory[] = [];

        for (const cat of respuesta.data) {
          categorias.push({
            id_categoria: cat.id,
            nombre_categoria: cat.nombre_categoria,
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
      const respuesta = await this.get<{ success: boolean; data: any }>('productos/marcas');

      if (respuesta.success && respuesta.data) {
        console.log(`✅ ${respuesta.data.length} marcas obtenidas`);

        const marcas: IBrand[] = [];

        for (const brand of respuesta.data) {
          marcas.push({
            id: brand.id,
            nombre: brand.nombre_marca,
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
  async getImagenProducto(referencia: string): Promise<string | null> {
    if (this.imagenesCache.has(referencia)) {
      return this.imagenesCache.get(referencia) || null;
    }

    try {
      const respuesta = await this.get<{ success: boolean; data: { referencia: string; imagenes: string[] }[] }>(
        'productos/imagenes', { referencia: referencia }
      );

      if (respuesta.success && respuesta.data?.length > 0) {
        const imagenes = respuesta.data[0].imagenes;
        if (imagenes?.length > 0) {
          this.imagenesCache.set(referencia, imagenes[0]);
          return imagenes[0];
        }
      }
      return null;
    } catch (error) {
      console.error(`Error al obtener imagen de ${referencia}:`, error);
      return null;
    }
  }

  /// Obtener imágenes de un solo producto
  async getImagenesProducto(referencia: string): Promise<string[]> {
    try {
      const respuesta = await this.get<{ success: boolean; data: { referencia: string; imagenes: string[] }[] }>(
        'productos/imagenes', { referencias: referencia }
      );

      const datos = respuesta.data;
      if (datos?.length > 0 && datos[0].imagenes?.length > 0) {
        return datos[0].imagenes;
      }
      return [];
    } catch (error) {
      console.error(`Error al obtener imágenes de ${referencia}:`, error);
      return [];
    }
  }

  // Obtener todas las imágenes de múltiples productos — un solo request
  async getImagenesMultiplesProductos(referencias: string[]): Promise<Map<string, string[]>> {
    const imagenesMap = new Map<string, string[]>();
    if (referencias.length === 0) return imagenesMap;

    try {
      const respuesta = await this.get<{ success: boolean; data: { referencia: string; imagenes: string[] }[] }>(
        'productos/imagenes', { referencias: referencias.join(',') }
      );

      const datos = respuesta.data;
      if (datos?.length > 0) {
        for (const item of datos) {
          imagenesMap.set(item.referencia, item.imagenes ?? []);
        }
      }
    } catch (error) {
      console.error('Error al obtener imágenes múltiples:', error);
    }

    return imagenesMap;
  }

  // Obtener imagen principal de múltiples productos — un solo request
  async getImagenesPrincipalesMultiples(
    referencias: string[],
  ): Promise<Map<string, string | null>> {
    const imagenesMap = new Map<string, string | null>();
    if (referencias.length === 0) return imagenesMap;

    try {
      const respuesta = await this.get<{ success: boolean; data: { referencia: string; imagenes: string[] }[] }>(
        'productos/imagenes', { referencias: referencias.join(',') }
      );

      const datos = respuesta.data;
      if (datos?.length > 0) {
        for (const item of datos) {
          imagenesMap.set(item.referencia, item.imagenes?.[0] ?? null);
        }
      }
    } catch (error) {
      console.error('Error al obtener imágenes principales:', error);
    }

    return imagenesMap;
  }

  // Obtener todos los productos (con imágenes optimizado)
  async getAllProducts(
    includeImages: boolean = true,
    page: number = 1,
    pageSize: number = 50,
    Stock: boolean,
  ): Promise<IProductosPageResponse> {
    try {
      const params: { [key: string]: string } = {
        pageSize: String(pageSize),
        page: String(page),
      };
      if (Stock) {
        params['stock'] = '';
      } else {
        params['stock'] = 'true';
      }

      const respuesta = await this.get<{
        success: boolean;
        data: any[];
        page: number;
        total_pages: number;
      }>('productos', params);

      if (respuesta.success === true && respuesta.data) {
        const currentPage = respuesta.page;
        const totalPages = respuesta.total_pages;
        const estimatedTotal = currentPage < totalPages
          ? totalPages * pageSize
          : (totalPages - 1) * pageSize + respuesta.data.length;

        console.log(
          `📦 Página ${currentPage}/${totalPages} — ${respuesta.data.length} productos en esta página`,
        );

        const productos: IProduct[] = respuesta.data.map((product: any) => ({
          id: String(product.id),
          referencia: product.referencia,
          sku: product.sku,
          nombre: product.nombre,
          descripcion: product.descripcion,
          codigoSat: product.codigoSat,
          codigoBarras: product.codigoBarras,
          stock: String(product.stock),
          precio: String(product.precio),
          precio_oferta: product.precio_oferta ? String(product.precio_oferta) : null,
          precio_sin_oferta: String(product.precio_sin_oferta || product.precio),
          oferta: Boolean(product.oferta),
          moneda: product.moneda,
          marca_id: String(product.marca_id),
          marca_nombre: product.marca_nombre,
          subcategoria_id: String(product.subcategoria_id || ''),
          subcategoria_nombre: product.subcategoria_nombre || '',
          categoria_id: String(product.categoria_id || ''),
          categoria_nombre: product.categoria_nombre || '',
          imagenes: [],
          imagen_principal: null,
        }));

        if (includeImages && productos.length > 0) {
          const referencias = productos.map((p) => p.referencia || p.sku).filter((ref) => ref);
          const [imagenesPrincipales, todasImagenes] = await Promise.all([
            this.getImagenesPrincipalesMultiples(referencias),
            this.getImagenesMultiplesProductos(referencias),
          ]);

          for (const producto of productos) {
            const clave = producto.referencia || producto.sku;
            if (clave) {
              producto.imagen_principal = imagenesPrincipales.get(clave) || null;
              producto.imagenes = todasImagenes.get(clave) || [];
            }
          }
        }

        return {
          productos,
          total: estimatedTotal,
          page: currentPage,
          pageSize,
          totalPages,
        };
      }

      return { productos: [], total: 0, page, pageSize, totalPages: 0 };
    } catch (error) {
      console.error('Error al obtener los productos:', error);
      return { productos: [], total: 0, page, pageSize, totalPages: 0 };
    }
  }

  async getFilteredProducts(
    filters: {
      categoria?: string[];
      subcategoria?: string[];
      marca?: string[];
      searchTerm?: string;
      precioMin?: number;
      precioMax?: number;
    },
    page: number = 1,
    pageSize: number = 50,
    Stock: boolean,
    includeImages: boolean = true,
  ): Promise<IProductosPageResponse> {
    try {
      const params: { [key: string]: string } = {
        pageSize: String(pageSize),
        page: String(page),
      };
      if (!Stock) {
        params['stock'] = 'true';
      }

      const body: any = {};
      if (filters.categoria && filters.categoria.length > 0) body.categoria = filters.categoria;
      if (filters.subcategoria && filters.subcategoria.length > 0) body.subcategoria = filters.subcategoria;
      if (filters.marca && filters.marca.length > 0) body.marca = filters.marca;
      if (filters.searchTerm) body.searchTerm = filters.searchTerm;
      if (filters.precioMin !== undefined) body.precioMin = filters.precioMin;
      if (filters.precioMax !== undefined) body.precioMax = filters.precioMax;

      const respuesta = await this.post<{
        success: boolean;
        data: any[];
        page: number;
        total_pages: number;
      }>('productos', body, params);

      if (respuesta.success === true && respuesta.data) {
        const currentPage = respuesta.page;
        const totalPages = respuesta.total_pages;
        const estimatedTotal = currentPage < totalPages
          ? totalPages * pageSize
          : (totalPages - 1) * pageSize + respuesta.data.length;

        const productos: IProduct[] = respuesta.data.map((product: any) => ({
          id: String(product.id),
          referencia: product.referencia,
          sku: product.sku,
          nombre: product.nombre,
          descripcion: product.descripcion,
          codigoSat: product.codigoSat,
          codigoBarras: product.codigoBarras,
          stock: String(product.stock),
          precio: String(product.precio),
          precio_oferta: product.precio_oferta ? String(product.precio_oferta) : null,
          precio_sin_oferta: String(product.precio_sin_oferta || product.precio),
          oferta: Boolean(product.oferta),
          moneda: product.moneda,
          marca_id: String(product.marca_id),
          marca_nombre: product.marca_nombre,
          subcategoria_id: String(product.subcategoria_id || ''),
          subcategoria_nombre: product.subcategoria_nombre || '',
          categoria_id: String(product.categoria_id || ''),
          categoria_nombre: product.categoria_nombre || '',
          imagenes: [],
          imagen_principal: null,
        }));

        if (includeImages && productos.length > 0) {
          const referencias = productos.map((p) => p.referencia || p.sku).filter((ref) => ref);
          const [imagenesPrincipales, todasImagenes] = await Promise.all([
            this.getImagenesPrincipalesMultiples(referencias),
            this.getImagenesMultiplesProductos(referencias),
          ]);

          for (const producto of productos) {
            const clave = producto.referencia || producto.sku;
            if (clave) {
              producto.imagen_principal = imagenesPrincipales.get(clave) || null;
              producto.imagenes = todasImagenes.get(clave) || [];
            }
          }
        }

        return {
          productos,
          total: estimatedTotal,
          page: currentPage,
          pageSize,
          totalPages,
        };
      }

      return { productos: [], total: 0, page, pageSize, totalPages: 0 };
    } catch (error) {
      console.error('Error al obtener productos filtrados:', error);
      return { productos: [], total: 0, page, pageSize, totalPages: 0 };
    }
  }

  //Obtener la informacion del producto con la referencia
  async getProductByReference(reference: string): Promise<IProduct | null> {
    try {
      // Pasamos la referencia directamente en la URL para que concuerde con el backend /api/productos/:referencia
      const response = await this.get<{ success: boolean; data: any }>(`productos/${reference}`);

      // Solo se verifica que response.success sea true y response.data exista
      if (response.success && response.data) {
        const p = response.data;

        // Mapeamos directamente al objeto sin usar .map()
        const producto: IProduct = {
          id: String(p.id || ''),
          referencia: p.referencia || '',
          sku: p.sku || '',
          nombre: p.nombre || '',
          descripcion: p.descripcion || '',
          stock: String(p.stock || '0'), // Valor por defecto si no viene
          codigoSat: p.codigoSAT || '',
          codigoBarras: p.codigoBarras || '',
          precio: String(p.precio || '0'),
          precio_oferta: p.precio_oferta ? String(p.precio_oferta) : null,
          precio_sin_oferta: String(p.precio_sin_oferta || p.precio || '0'),
          oferta: Boolean(p.oferta),
          moneda: p.moneda || 'MXN', // Valor por defecto si no viene

          // Ajuste de los mapeos según los campos de la respuesta JSON
          marca_id: String(p.marca_id || ''),
          marca_nombre: p.marca || '', // En el JSON viene solo como 'marca'
          subcategoria_id: String(p.subcategoria || ''), // JSON usa 'subcategoria'
          subcategoria_nombre: p.subcategoria_nombre || '',
          categoria_id: String(p.categoria || ''), // JSON usa 'categoria'
          categoria_nombre: p.categoria_nombre || '',

          imagenes: [],
          imagen_principal: null,
        };

        // Obtener las imágenes del producto
        const imagenes = await this.getImagenesProducto(producto.referencia || producto.sku || '');
        producto.imagenes = imagenes;
        producto.imagen_principal = imagenes.length > 0 ? imagenes[0] : null;

        return producto; // Se retorna el objeto
      }

      return null;
    } catch (error) {
      console.error(`Error al obtener producto ${reference}:`, error);
      return null;
    }
  }

  //Obtener las medidas por referencia
  async getMedidaProducto(referencia: string): Promise<any> {
    try {
      console.log('🔍 Buscando medida para referencia:', referencia);
      const respuesta = await this.get<any>(`productos/medida/${referencia}`);
      return respuesta?.data;
    } catch (error) {
      console.error(`Error al obtener medida de ${referencia}:`, error);
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
      minimumFractionDigits: 2,
    }).format(num);
  }

  // Limpiar caché de imágenes
  clearImageCache(): void {
    this.imagenesCache.clear();
    console.log('🗑️ Caché de imágenes limpiada');
  }

// front-end/src/app/services/exel-api-base.service.ts

// Agregar este método después de getMedidaProducto

/**
 * Obtiene información de productos por nombre para el carrito
 * Busca productos que coincidan con los nombres proporcionados
 */
async getProductosInfoParaCarrito(nombres: string[]): Promise<Map<string, { 
  referencia: string; 
  precio: string; 
  imagen_principal: string | null;
  sku: string;
}>> {
  const resultado = new Map();
  
  if (!nombres || nombres.length === 0) {
    return resultado;
  }

  try {
    console.log('🔍 Buscando información para productos:', nombres);
    
    // Buscar productos por nombre usando el endpoint de productos
    // Usamos el método getFilteredProducts con searchTerm para buscar por nombre
    const productosResponse = await this.getFilteredProducts(
      { searchTerm: nombres.join(' ') }, // Buscar por todos los nombres
      1, // página 1
      100, // hasta 100 productos
      true, // incluir sin stock
      true // incluir imágenes
    );

    if (productosResponse && productosResponse.productos) {
      // Crear un mapa de nombres en minúscula para búsqueda flexible
      const productosMap = new Map();
      for (const producto of productosResponse.productos) {
        const nombreLower = producto.nombre.toLowerCase().trim();
        productosMap.set(nombreLower, producto);
      }

      // Buscar cada producto por nombre
      for (const nombre of nombres) {
        const nombreLower = nombre.toLowerCase().trim();
        
        // Buscar coincidencia exacta o parcial
        let productoEncontrado = null;
        
        // Primero buscar coincidencia exacta
        if (productosMap.has(nombreLower)) {
          productoEncontrado = productosMap.get(nombreLower);
        } else {
          // Buscar coincidencia parcial (el nombre del carrito puede ser parte del nombre completo)
          for (const [key, value] of productosMap) {
            if (key.includes(nombreLower) || nombreLower.includes(key)) {
              productoEncontrado = value;
              break;
            }
          }
        }

        if (productoEncontrado) {
          resultado.set(nombre, {
            referencia: productoEncontrado.referencia || productoEncontrado.sku || '',
            precio: productoEncontrado.precio || '0',
            imagen_principal: productoEncontrado.imagen_principal || null,
            sku: productoEncontrado.sku || ''
          });
        } else {
          // Si no se encuentra, guardar con valores por defecto
          resultado.set(nombre, {
            referencia: nombre,
            precio: '0',
            imagen_principal: null,
            sku: ''
          });
        }
      }
    }

    console.log('✅ Información de productos obtenida:', resultado.size);
    return resultado;
  } catch (error) {
    console.error('❌ Error al obtener información de productos:', error);
    return resultado;
  }
}  
}
