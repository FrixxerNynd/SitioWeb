// front-end/src/app/services/mexico-api.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

export interface IMexicoState {
  d_estado: string;
  d_codigo: string;
}

export interface IMexicoCity {
  d_ciudad: string;
  d_estado: string;
  D_mnpio: string;
  d_codigo: string;
}

export interface IMexicoZipCode {
  d_codigo: string;
  d_asenta: string;
  D_mnpio: string;
  d_estado: string;
  d_ciudad: string;
}

export interface IMexicoSettlement {
  d_asenta: string;
  d_tipo_asenta: string;
  D_mnpio: string;
  d_estado: string;
  d_ciudad: string;
  d_codigo: string;
}

export interface IApiMexicoResponse<T> {
  meta: {
    page: number;
    per_page: number;
    total: number;
    total_pages: number;
  };
  data: T[];
}

@Injectable({
  providedIn: 'root'
})
export class MexicoApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiMexicoUrl || '/api-mexico';

  async getStates(): Promise<IMexicoState[]> {
    try {
      console.log('🌐 GET Estados:', `${this.baseUrl}/estado?per_page=100`);
      const response = await firstValueFrom(
        this.http.get<IApiMexicoResponse<IMexicoState>>(`${this.baseUrl}/estado?per_page=100`)
      );
      console.log('✅ Estados obtenidos:', response?.data?.length);
      return response?.data || [];
    } catch (error) {
      console.error('Error fetching states:', error);
      return [];
    }
  }

  async getCities(): Promise<IMexicoCity[]> {
    try {
      console.log('🌐 GET Ciudades:', `${this.baseUrl}/ciudad?per_page=500`);
      const response = await firstValueFrom(
        this.http.get<IApiMexicoResponse<IMexicoCity>>(`${this.baseUrl}/ciudad?per_page=500`)
      );
      console.log('✅ Ciudades obtenidas:', response?.data?.length);
      return response?.data || [];
    } catch (error) {
      console.error('Error fetching cities:', error);
      return [];
    }
  }

  async getZipCodeInfo(zipCode: string): Promise<IMexicoZipCode | null> {
    try {
      console.log('🌐 GET Código Postal:', `${this.baseUrl}/codigo-postal/${zipCode}`);
      const response = await firstValueFrom(
        this.http.get<IApiMexicoResponse<IMexicoZipCode>>(`${this.baseUrl}/codigo-postal/${zipCode}`)
      );
      if (response?.data && response.data.length > 0) {
        return response.data[0];
      }
      return null;
    } catch (error) {
      console.error(`Error fetching zip code ${zipCode}:`, error);
      return null;
    }
  }

  async getSettlementsByZipCode(zipCode: string): Promise<IMexicoSettlement[]> {
    try {
      console.log(`🌐 GET Colonias por CP: ${this.baseUrl}/colonia?per_page=500`);
      const response = await firstValueFrom(
        this.http.get<IApiMexicoResponse<IMexicoSettlement>>(`${this.baseUrl}/colonia?per_page=500`)
      );
      if (response?.data) {
        const filtered = response.data.filter(s => s.d_codigo === zipCode);
        console.log(`✅ Colonias encontradas para CP ${zipCode}:`, filtered.length);
        return filtered;
      }
      return [];
    } catch (error) {
      console.error('Error fetching settlements:', error);
      return [];
    }
  }

  async loadAddressDataByZipCode(zipCode: string): Promise<{
    zipInfo: IMexicoZipCode | null;
    settlements: IMexicoSettlement[];
    state: IMexicoState | null;
    city: IMexicoCity | null;
  }> {
    try {
      const [zipInfo, settlements] = await Promise.all([
        this.getZipCodeInfo(zipCode),
        this.getSettlementsByZipCode(zipCode)
      ]);
      
      let state: IMexicoState | null = null;
      let city: IMexicoCity | null = null;
      
      if (zipInfo) {
        const states = await this.getStates();
        state = states.find(s => s.d_estado === zipInfo.d_estado) || null;
        
        const cities = await this.getCities();
        city = cities.find(c => c.d_ciudad === zipInfo.d_ciudad && c.d_estado === zipInfo.d_estado) || null;
      }
      
      return { zipInfo, settlements, state, city };
    } catch (error) {
      console.error('Error loading address data:', error);
      return { zipInfo: null, settlements: [], state: null, city: null };
    }
  }
}