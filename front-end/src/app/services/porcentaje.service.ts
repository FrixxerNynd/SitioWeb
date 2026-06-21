import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from './cookie.service';
import {
  IPercentage,
  IPercentageResponse,
  IPercentageSingleResponse,
} from '../interfaces/interface-excel-norte/percentage.interface';

@Injectable({
  providedIn: 'root',
})
export class PorcentajeService {
  private http = inject(HttpClient);
  private cookieService = inject(CookieService);
  private baseUrl = environment.backendUrl;

  private async get<T>(endpoint: string): Promise<T> {
    const url = `${this.baseUrl}/${endpoint}`;

    const token = this.cookieService.getCookie('token');

    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    try {
      const response = await firstValueFrom(this.http.get<T>(url, { headers }));
      return response;
    } catch (error) {
      console.error(`Error en GET ${url}:`, error);
      throw error;
    }
  }

  private async post<T>(endpoint: string, body: any): Promise<T> {
    const url = `${this.baseUrl}/${endpoint}`;

    const token = this.cookieService.getCookie('token');

    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    try {
      const response = await firstValueFrom(this.http.post<T>(url, body, { headers }));
      return response;
    } catch (error) {
      console.error(`Error en POST ${url}:`, error);
      throw error;
    }
  }

  private async delete<T>(endpoint: string): Promise<T> {
    const url = `${this.baseUrl}/${endpoint}`;

    const token = this.cookieService.getCookie('token');

    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    try {
      const response = await firstValueFrom(this.http.delete<T>(url, { headers }));
      return response;
    } catch (error) {
      console.error(`Error en DELETE ${url}:`, error);
      throw error;
    }
  }

  async getAll(): Promise<IPercentage[]> {
    try {
      const respuesta = await this.get<IPercentageResponse>('porcentajes');
      if (respuesta.success && respuesta.data) {
        return respuesta.data;
      }
      return [];
    } catch (error) {
      console.error('Error al obtener porcentajes:', error);
      return [];
    }
  }

  async getByCategoria(id_categoria: string): Promise<IPercentage | null> {
    try {
      const respuesta = await this.get<IPercentageSingleResponse>(`porcentajes/categoria/${id_categoria}`);
      if (respuesta.success && respuesta.data) {
        return respuesta.data;
      }
      return null;
    } catch (error) {
      console.error(`Error al obtener porcentaje de categoria ${id_categoria}:`, error);
      return null;
    }
  }

  async save(id_categoria: string, nombre_categoria: string, porcentaje: number): Promise<IPercentage | null> {
    try {
      const respuesta = await this.post<IPercentageSingleResponse>('porcentajes', {
        id_categoria,
        nombre_categoria,
        porcentaje,
      });
      if (respuesta.success && respuesta.data) {
        return respuesta.data;
      }
      return null;
    } catch (error) {
      console.error('Error al guardar porcentaje:', error);
      return null;
    }
  }

  async eliminar(id: number): Promise<boolean> {
    try {
      const respuesta = await this.delete<{ success: boolean }>(`porcentajes/${id}`);
      return respuesta.success;
    } catch (error) {
      console.error(`Error al eliminar porcentaje ${id}:`, error);
      return false;
    }
  }
}
