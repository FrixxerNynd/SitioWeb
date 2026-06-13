/**
 * DTO de ítem dentro de una orden de venta.
 * Expone los campos clave del producto tal como fueron congelados en el momento de la compra.
 */
class OrderItemResponseDto {
  /**
   * @param {Object} data
   * @param {string} data.sku          - SKU del producto en el catálogo Exel
   * @param {string} data.referencia   - ID del producto usado como referencia de catálogo
   * @param {string} data.nombre       - Nombre del producto al momento de la compra
   * @param {number} data.precio       - Precio unitario al momento de la compra
   * @param {number} data.cantidad     - Unidades compradas
   * @param {number} data.totalProducto - precio * cantidad
   */
  constructor(data = {}) {
    this.sku          = data.sku          ?? '';
    this.referencia   = data.referencia   ?? '';
    this.nombre       = data.nombre       ?? '';
    this.precio       = data.precio       ?? 0;
    this.cantidad     = data.cantidad     ?? 0;
    this.totalProducto = data.totalProducto ?? 0;
  }
}

// ─────────────────────────────────────────────────────────────
//  DTO principal de respuesta de una Orden de Venta
// ─────────────────────────────────────────────────────────────
class OrderResponseDto {
  /**
   * @param {Object} data
   * @param {number}               data.id
   * @param {string}               [data.estado]
   * @param {Date|string}          [data.fechaPedido]
   * @param {string}               [data.metodoPago]
   * @param {string}               [data.metodoEntrega]
   * @param {Object|string|null}   [data.direccionEntrega] - JSON almacenado en BD
   * @param {number}               [data.subtotal]
   * @param {number}               [data.flete]
   * @param {number}               [data.total]
   * @param {OrderItemResponseDto[]} [data.productos]
   */
  constructor(data = {}) {
    this.id              = data.id              ?? 0;
    this.estado          = data.estado          ?? '';
    this.fechaPedido     = data.fechaPedido ? new Date(data.fechaPedido) : new Date();
    this.metodoPago      = data.metodoPago      ?? '';
    this.metodoEntrega   = data.metodoEntrega   ?? '';
    this.direccionEntrega = data.direccionEntrega ?? null;
    this.subtotal        = data.subtotal        ?? 0;
    this.flete           = data.flete           ?? 0;
    this.total           = data.total           ?? 0;
    this.productos       = (data.productos ?? []).map((p) => new OrderItemResponseDto(p));
  }
}

export default OrderResponseDto;
