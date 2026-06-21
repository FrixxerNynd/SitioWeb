import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from './cookie.service';
import {
  OrderTotalResponse,
  OrderApiResponse,
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

  async getTotalOrdenes(): Promise<number> {
    try {
      const respuesta = await this.get<OrderTotalResponse>('orders/summary/total');
      if (respuesta.success) {
        return respuesta.data;
      }
      return 0;
    } catch (error) {
      console.error('Error al obtener total de órdenes:', error);
      return 0;
    }
  }

  async getAll(): Promise<Order[]> {
    try {
      const respuesta = await this.get<OrderApiResponse>('orders');
      if (respuesta.success) {
        return respuesta.data;
      }
      return [];
    } catch (error) {
      console.error('Error al obtener órdenes:', error);
      return [];
    }
  }
}
