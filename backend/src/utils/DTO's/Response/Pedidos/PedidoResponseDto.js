// backend/src/utils/DTO's/Response/Pedidos/PedidoResponseDto.js

class PedidoResponseDto {
  /**
   * @param {Object} data
   * @param {number} data.id
   * @param {string} [data.estado]
   * @param {Date|string} [data.fechaPedido]
   * @param {string} [data.clienteNombre]
   * @param {string} [data.transportista]
   * @param {string} [data.numeroFactura]
   * @param {string[]} [data.productos] 
   * @param {Array<{productId: string, sku: string, name: string, price: number, quantity: number, totalPrice: number}>} [data.items] 
   * @param {number} data.subtotal
   * @param {number} data.flete
   * @param {number} data.iva
   * @param {number} data.total
   */
  constructor(data = {}) {
    this.id = data.id ?? 0;
    this.estado = data.estado ?? '';
    this.fechaPedido = data.fechaPedido ? new Date(data.fechaPedido) : new Date();
    this.clienteNombre = data.clienteNombre ?? '';
    this.transportista = data.transportista ?? '';
    this.numeroFactura = data.numeroFactura ?? '';
    this.productos = data.productos ?? [];
    this.items = data.items ?? []; 
    this.subtotal = data.subtotal ?? 0;
    this.flete = data.flete ?? 0;
    this.iva = data.iva ?? 0;
    this.total = data.total ?? 0;
  }
}

export default PedidoResponseDto;