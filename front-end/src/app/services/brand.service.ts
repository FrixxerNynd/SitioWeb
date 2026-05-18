// frontend/src/app/services/brand.service.ts
import { Injectable } from '@angular/core';
import { ExelApiBaseService } from './exel-api-base.service';
import { Brand } from '../models/models-excel-norte/brand.model';
import { IApiBrandsResponse } from '../interfaces/interface-excel-norte/brand.interface';

@Injectable({
    providedIn: 'root'
})
export class BrandService extends ExelApiBaseService {
    
    async getAllBrands(): Promise<Brand[]> {
        try {
            console.log('📡 Fetching brands from API...');
            const data = await this.get<IApiBrandsResponse>('marcas');
            
            console.log('📦 Raw API response:', data);
            
            if (data && data.resultado && data.datos && Array.isArray(data.datos)) {
                const brands = data.datos.map(brand => new Brand(brand));
                console.log(`✅ ${brands.length} marcas procesadas:`, brands);
                return brands;
            }
            
            console.warn('⚠️ No se encontraron marcas en la respuesta');
            return [];
        } catch (error) {
            console.error('❌ Error fetching brands:', error);
            return [];  // Siempre retornar un array vacío en caso de error
        }
    }
}