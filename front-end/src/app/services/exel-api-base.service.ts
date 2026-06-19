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
  IApiResponse,
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
      console.log(`📦 Imagen en caché para: ${referencia}`);
      return this.imagenesCache.get(referencia) || null;
    }

    try {
      const respuesta = await this.get<any>('productos/imagenes', { referencia: referencia });

      if (respuesta?.RESULT && respuesta.DATA && respuesta.DATA.length > 0) {
        const urlImagen = respuesta.DATA[0].url_imagen;
        this.imagenesCache.set(referencia, urlImagen);
        console.log(`✅ Imagen obtenida para: ${referencia}`);
        return urlImagen;
      }
      console.log(`⚠️ Sin imagen para: ${referencia}`);
      return null;
    } catch (error) {
      console.error(`Error al obtener imagen de ${referencia}:`, error);
      return null;
    }
  }

  /// Obtener imágenes de un solo producto
  async getImagenesProducto(referencia: string): Promise<string[]> {
    try {
      const respuesta = await this.get<any>('productos/imagenes', { referencias: referencia });

      const datos = respuesta?.data?.datos;
      if (datos && datos.length > 0 && datos[0].imagenes?.length > 0) {
        console.log(`✅ ${datos[0].imagenes.length} imágenes obtenidas para ${referencia}`);
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
      const respuesta = await this.get<any>('productos/imagenes', {
        referencias: referencias.join(','),
      });

      const datos = respuesta?.data?.datos;
      if (datos && datos.length > 0) {
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
      const respuesta = await this.get<any>('productos/imagenes', {
        referencias: referencias.join(','),
      });

      const datos = respuesta?.data?.datos;
      if (datos && datos.length > 0) {
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
      if (Stock) {
        var params = {
          pageSize: String(pageSize),
          page: String(page),
          stock: '',
        };
      } else {
        params = {
          pageSize: String(pageSize),
          page: String(page),
          stock: 'true',
        };
      }

      const respuesta = await this.get<{
        success: boolean;
        source: string;
        total: number;
        data: any[];
        pageSize: number;
        Pagina: number;
        Paginas: number;
      }>('productos', params);

      if (respuesta.success === true && respuesta.data) {
        console.log(
          `📦 Página ${respuesta.Pagina}/${respuesta.Paginas} — ${respuesta.data.length}/${respuesta.total} productos obtenidos`,
        );

        const productos: IProduct[] = respuesta.data.map((product: any) => ({
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
          total: respuesta.total,
          page: respuesta.Pagina,
          pageSize: respuesta.pageSize,
          totalPages: respuesta.Paginas,
        };
      }

      return { productos: [], total: 0, page, pageSize, totalPages: 0 };
    } catch (error) {
      console.error('Error al obtener los productos:', error);
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
          stock: String(p.stock || '0'), // Valor por defecto si no viene
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
}
