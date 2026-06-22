import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from './cookie.service';
import {
  OrderTotalResponse,
  OrderApiResponse,
  OrderSingleResponse,
  Order,
} from '../interfaces/order.interface';

@Injectable({
  providedIn: 'root',
})
export class OrdenService {
  private http = inject(HttpClient);
  private cookieService = inject(CookieService);
  private baseUrl = environment.backendUrl;

  private async get<T>(endpoint: string): Promise<T> {
    const url = `${this.baseUrl}/${endpoint}`;
    const token = this.cookieService.getCookie('token');
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (token) headers = headers.set('Authorization', `Bearer ${token}`);
    try {
      return await firstValueFrom(this.http.get<T>(url, { headers }));
    } catch (error) {
      console.error(`Error en GET ${url}:`, error);
      throw error;
    }
  }

  private async patch<T>(endpoint: string, body: any): Promise<T> {
    const url = `${this.baseUrl}/${endpoint}`;
    const token = this.cookieService.getCookie('token');
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (token) headers = headers.set('Authorization', `Bearer ${token}`);
    try {
      return await firstValueFrom(this.http.patch<T>(url, body, { headers }));
    } catch (error) {
      console.error(`Error en PATCH ${url}:`, error);
      throw error;
    }
  }

  private async del<T>(endpoint: string): Promise<T> {
    const url = `${this.baseUrl}/${endpoint}`;
    const token = this.cookieService.getCookie('token');
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (token) headers = headers.set('Authorization', `Bearer ${token}`);
    try {
      return await firstValueFrom(this.http.delete<T>(url, { headers }));
    } catch (error) {
      console.error(`Error en DELETE ${url}:`, error);
      throw error;
    }
  }

  async getAll(): Promise<Order[]> {
    try {
      const respuesta = await this.get<OrderApiResponse>('orders');
      if (respuesta.success) return respuesta.data;
      return [];
    } catch (error) {
      console.error('Error al obtener órdenes:', error);
      return [];
    }
  }

  async getById(id: number): Promise<Order | null> {
    try {
      const respuesta = await this.get<OrderSingleResponse>(`orders/${id}`);
      if (respuesta.success) return respuesta.data;
      return null;
    } catch (error) {
      console.error(`Error al obtener orden ${id}:`, error);
      return null;
    }
  }

  async updateStatus(id: number, status: string): Promise<Order | null> {
    try {
      const respuesta = await this.patch<OrderSingleResponse>(
        `orders/${id}/status`,
        { status },
      );
      if (respuesta.success) return respuesta.data;
      return null;
    } catch (error) {
      console.error(`Error al actualizar estado de orden ${id}:`, error);
      return null;
    }
  }

  async deleteOrder(id: number): Promise<boolean> {
    try {
      const respuesta = await this.del<{ success: boolean }>(
        `orders/${id}`,
      );
      return respuesta.success;
    } catch (error) {
      console.error(`Error al eliminar orden ${id}:`, error);
      return false;
    }
  }

  async getTotalOrdenes(): Promise<number> {
    try {
      const respuesta = await this.get<OrderTotalResponse>(
        'orders/summary/total',
      );
      if (respuesta.success) return respuesta.data;
      return 0;
    } catch (error) {
      console.error('Error al obtener total de órdenes:', error);
      return 0;
    }
  }
}
