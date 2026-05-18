// frontend/src/app/services/category.service.ts
import { Injectable } from '@angular/core';
import { ExelApiBaseService } from './exel-api-base.service';
import { Category } from '../models/models-excel-norte/category.model';
import { IApiCategoriesResponse } from '../interfaces/interface-excel-norte/category.interface';

@Injectable({
    providedIn: 'root'
})
export class CategoryService extends ExelApiBaseService {
    
    async getAllCategories(): Promise<Category[]> {
        try {
            console.log('📡 Fetching categories from API...');
            const data = await this.get<IApiCategoriesResponse>('categorias');
            
            console.log('📦 Raw API response:', data);
            
            if (data && data.resultado && data.datos && Array.isArray(data.datos)) {
                const categories = data.datos.map(cat => new Category(cat));
                console.log(`✅ ${categories.length} categorías procesadas:`, categories);
                return categories;
            }
            
            console.warn('⚠️ No se encontraron categorías en la respuesta');
            return [];
        } catch (error) {
            console.error('❌ Error fetching categories:', error);
            return [];  // Siempre retornar un array vacío en caso de error
        }
    }
}