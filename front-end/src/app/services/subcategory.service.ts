// frontend/src/app/services/subcategory.service.ts (ACTUALIZADO)
import { Injectable } from '@angular/core';
import { Subcategory } from '../models/models-excel-norte/subcategory.model';
import { IApiSubcategoriesResponse } from '../interfaces/interface-excel-norte/subcategory.interface';
import { ExelApiBaseService } from './exel-api-base.service';

@Injectable({
    providedIn: 'root'
})
export class SubcategoryService extends ExelApiBaseService {
    
    async getAllSubcategories(): Promise<Subcategory[]> {
        try {
            const data = await this.get<IApiSubcategoriesResponse>('subcategorias');
            
            if (data.resultado && data.datos) {
                return data.datos.map(sub => new Subcategory(sub));
            }
            return [];
        } catch (error) {
            console.error('Error fetching subcategories:', error);
            throw error;
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
}