// frontend/src/app/services/product.service.ts
import { Injectable } from '@angular/core';
import { ExelApiBaseService } from './exel-api-base.service';
import { IApiResponse, IProduct } from '../interfaces/interface-excel-norte/excel-norte-interface';

@Injectable({
    providedIn: 'root'
})
export class ProductService extends ExelApiBaseService {
    
    async getProducts(filters?: {
        categoria?: string;
        subcategoria?: string;
        marca?: string;
        sin_stock?: boolean;
    }): Promise<IProduct[]> {
        try {
            let endpoint = 'productos';
            const params = new URLSearchParams();
            
            if (filters?.categoria) params.append('categoria', filters.categoria);
            if (filters?.subcategoria) params.append('subcategoria', filters.subcategoria);
            if (filters?.marca) params.append('marca', filters.marca);
            if (filters?.sin_stock) params.append('sin_stock', 'true');
            
            const queryString = params.toString();
            if (queryString) {
                endpoint += `?${queryString}`;
            }
            
            const data = await this.get<IApiResponse<IProduct[]>>(endpoint);
            
            if (data.resultado && data.datos) {
                return data.datos;
            }
            return [];
        } catch (error) {
            console.error('Error fetching products:', error);
            throw error;
        }
    }
}