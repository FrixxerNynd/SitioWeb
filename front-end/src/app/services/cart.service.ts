// front-end/src/app/services/cart.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CartItem {
  productId: string;
  sku: string;
  name: string;
  price: number;
  quantity: number;
  totalPrice: number;
}

export type PaymentType = 'Efectivo' | 'Transferencia' | 'Crédito';

export interface CartResponse {
  success: boolean;
  data: {
    id: number;
    estado: string;
    fechaPedido: string;
    clienteNombre: string;
    transportista: string;
    numeroFactura: string;
    productos: string[];
    items: CartItem[];
    subtotal: number;
    flete: number;
    iva: number;
    total: number;
    paymentType?: string;
  };
}

export interface Address {
  id?: number;
  country: string;
  state: string;
  city: string;
  cp: string;
  neighborhood: string;
  street: string;
  extNum: string;
  intNum?: string;
  nombre?: string;
  esPrincipal?: boolean;
}

export interface AddItemRequest {
  productId: string;
  quantity: number;
  price: number;
  stock: number;
  name: string;
  sku: string;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private http = inject(HttpClient);
  private baseUrl = environment.backendUrl;

  private getToken(): string | null {
    const token = localStorage.getItem('token');
    console.log('🔑 Token desde localStorage:', token ? '✅ Presente' : '❌ No encontrado');
    if (token) {
      console.log('🔑 Token (primeros 30 chars):', token.substring(0, 30) + '...');
      return token;
    }
    return null;
  }

  private getHeaders(): HttpHeaders {
    const token = this.getToken();
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
      console.log('📤 Header Authorization:', `Bearer ${token.substring(0, 30)}...`);
    } else {
      console.warn('⚠️ No hay token, las peticiones autenticadas fallarán');
    }

    return headers;
  }

  // ========== CARRITO ==========
  
  async getCart(): Promise<CartResponse> {
    const url = `${this.baseUrl}/cart`;
    console.log('🛒 GET Carrito:', url);
    
    const headers = this.getHeaders();
    
    try {
      const response = await firstValueFrom(
        this.http.get<CartResponse>(url, { headers })
      );
      console.log('📦 RESPUESTA DEL CARRITO:', response);
      return response;
    } catch (error: any) {
      console.error('❌ Error en getCart:', error);
      throw error;
    }
  }

  async addItemFull(itemData: AddItemRequest): Promise<any> {
    const url = `${this.baseUrl}/cart/item`;
    console.log('➕ POST Agregar item:', url, itemData);
    
    const headers = this.getHeaders();
    const response = await firstValueFrom(
      this.http.post<any>(url, itemData, { headers })
    );
    console.log('✅ Respuesta:', response);
    return response;
  }

  async updateItemQuantity(productId: string, quantity: number): Promise<any> {
    const url = `${this.baseUrl}/cart/item/${productId}`;
    const body = { quantity };
    console.log('🔄 PUT Actualizar cantidad:', url, body);
    
    const headers = this.getHeaders();
    const response = await firstValueFrom(
      this.http.put<any>(url, body, { headers })
    );
    console.log('✅ Respuesta:', response);
    return response;
  }

  async removeItem(productId: string): Promise<any> {
    const url = `${this.baseUrl}/cart/item/${encodeURIComponent(productId)}`;
    console.log('🗑️ DELETE Eliminar item:', url);

    const headers = this.getHeaders();
    
    try {
      const response = await firstValueFrom(
        this.http.delete<any>(url, { headers })
      );
      console.log('✅ Respuesta DELETE:', response);
      return response;
    } catch (error: any) {
      console.error('❌ Error en removeItem:', error);
      throw error;
    }
  }

  // ========== TIPO DE PAGO ==========
  
  async setPaymentType(paymentType: PaymentType): Promise<any> {
    const url = `${this.baseUrl}/cart/payment`;
    const body = { paymentType };
    console.log('💳 PATCH Tipo de pago:', url, body);
    
    const headers = this.getHeaders();
    const response = await firstValueFrom(
      this.http.patch<any>(url, body, { headers })
    );
    console.log('✅ Respuesta:', response);
    return response;
  }

  // ========== MÉTODO DE ENTREGA ==========
  
  async setDeliveryMethod(method: 'Domicilio' | 'Sucursal', addressId?: number): Promise<any> {
    const url = `${this.baseUrl}/cart/delivery`;
    const body: any = { method };
    if (addressId) {
      body.addressId = addressId;
    }
    console.log('📦 PATCH Método de entrega:', url, body);
    
    const headers = this.getHeaders();
    const response = await firstValueFrom(
      this.http.patch<any>(url, body, { headers })
    );
    console.log('✅ Respuesta:', response);
    return response;
  }

  // ========== CHECKOUT ==========
  
  async checkout(): Promise<any> {
    const url = `${this.baseUrl}/cart/checkout`;
    console.log('✅ POST Checkout:', url);
    
    const headers = this.getHeaders();
    const response = await firstValueFrom(
      this.http.post<any>(url, {}, { headers })
    );
    console.log('✅ Respuesta checkout:', response);
    return response;
  }

  // ========== DIRECCIONES ==========
  
  async getAddresses(): Promise<any> {
    const url = `${this.baseUrl}/cart/addresses`;
    console.log('📍 GET Direcciones:', url);
    
    const headers = this.getHeaders();
    console.log('📍 Headers enviados:', headers.keys());
    console.log('📍 Authorization:', headers.get('Authorization') ? '✅ Presente' : '❌ No presente');
    
    try {
      const response = await firstValueFrom(
        this.http.get<any>(url, { headers })
      );
      console.log('📍 Respuesta direcciones (status):', response);
      console.log('📍 Respuesta completa:', JSON.stringify(response, null, 2));
      return response;
    } catch (error: any) {
      console.error('❌ Error en getAddresses:', error);
      console.error('❌ Status:', error.status);
      console.error('❌ Message:', error.message);
      if (error.error) {
        console.error('❌ Error body:', error.error);
      }
      throw error;
    }
  }

  async addAddress(address: Address): Promise<any> {
    const url = `${this.baseUrl}/cart/addresses`;
    console.log('📍 POST Agregar dirección:', url, address);
    
    const headers = this.getHeaders();
    const response = await firstValueFrom(
      this.http.post<any>(url, address, { headers })
    );
    console.log('✅ Respuesta:', response);
    return response;
  }

  async updateAddress(addressId: number, address: Address): Promise<any> {
    const url = `${this.baseUrl}/cart/addresses/${addressId}`;
    console.log('📍 PUT Actualizar dirección:', url, address);
    
    const headers = this.getHeaders();
    const response = await firstValueFrom(
      this.http.put<any>(url, address, { headers })
    );
    console.log('✅ Respuesta:', response);
    return response;
  }

  async deleteAddress(addressId: number): Promise<any> {
    const url = `${this.baseUrl}/cart/addresses/${addressId}`;
    console.log('📍 DELETE Eliminar dirección:', url);
    
    const headers = this.getHeaders();
    const response = await firstValueFrom(
      this.http.delete<any>(url, { headers })
    );
    console.log('✅ Respuesta:', response);
    return response;
  }
}