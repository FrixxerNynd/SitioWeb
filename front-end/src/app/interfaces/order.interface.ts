export interface OrderTotalResponse {
  success: boolean;
  data: number;
}

export interface OrderItem {
  sku: string;
  referencia: string;
  nombre: string;
  precio: number;
  cantidad: number;
  totalProducto: number;
}

export interface Order {
  id: number;
  estado: string;
  fechaPedido: string;
  metodoPago: string;
  metodoEntrega: string;
  direccionEntrega: object | null;
  subtotal: number;
  flete: number;
  total: number;
  productos: OrderItem[];
}

export interface OrderListResponse {
  success: boolean;
  data: Order[];
}

export interface OrderSingleResponse {
  success: boolean;
  data: Order;
}

export interface OrderApiResponse {
  success: boolean;
  data: Order[];
}
