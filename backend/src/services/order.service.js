import prisma from '../config/db.js';
import logger from '../utils/Helpers/logger.js';
import OrderResponseDto from "../utils/DTO's/Response/Pedidos/OrderResponseDto.js";

// ── Adaptador: traduce un registro Order de Prisma al DTO de respuesta ──
const toResponseDto = (order) => {
  // La dirección se guarda como JSON string en la BD; se parsea de forma segura
  let direccionEntrega = null;
  if (order.shippingAddress) {
    try {
      direccionEntrega = JSON.parse(order.shippingAddress);
    } catch {
      direccionEntrega = order.shippingAddress;
    }
  }

  return new OrderResponseDto({
    id:              order.id,
    estado:          order.status      ?? '',
    fechaPedido:     order.createdAt   ?? new Date(),
    metodoPago:      order.paymentType ?? '',
    metodoEntrega:   order.deliveryMethod ?? '',
    direccionEntrega,
    subtotal:        order.subtotal    ?? 0,
    flete:           order.shippingCost ?? 0,
    total:           order.total       ?? 0,
    productos: (order.items ?? []).map((item) => ({
      sku:           item.sku,
      referencia:    String(item.productId),   // ID del catálogo Exel como referencia
      nombre:        item.name,
      precio:        item.price,
      cantidad:      item.quantity,
      totalProducto: item.totalPrice,
    })),
  });
};

// ═══════════════════════════════════════════════════════════
//  SERVICIO - Lógica de negocio de Órdenes de Venta
// ═══════════════════════════════════════════════════════════

class OrderService {

  // ─── READ - Listar todos los pedidos del usuario autenticado ───
  async getOrders(userId) {
    const orders = await prisma.order.findMany({
      where:   { userId: parseInt(userId) },
      include: { items: true },
      orderBy: { createdAt: 'desc' },
    });

    return orders.map(toResponseDto);
  }

  // ─── READ - Obtener un pedido específico por ID ───
  async getOrderById(userId, orderId) {
    const order = await prisma.order.findUnique({
      where:   { id: parseInt(orderId) },
      include: { items: true },
    });

    if (!order)
      throw new Error('El pedido no existe.');

    // Garantiza que el usuario solo pueda ver sus propios pedidos
    if (order.userId !== parseInt(userId))
      throw new Error('No tienes permiso para ver este pedido.');

    return toResponseDto(order);
  }

  // ─── UPDATE - Cambiar el estado de un pedido ───
  async updateOrderStatus(userId, orderId, status) {
    const ESTADOS_VALIDOS = [
      'PAGADO_PENDIENTE_SURTIDO',
      'EN_PREPARACION',
      'EN_CAMINO',
      'ENTREGADO',
      'CANCELADO',
    ];

    if (!ESTADOS_VALIDOS.includes(status)) {
      throw new Error(
        `Estado inválido. Los estados permitidos son: ${ESTADOS_VALIDOS.join(', ')}`
      );
    }

    const order = await prisma.order.findUnique({
      where:   { id: parseInt(orderId) },
      include: { items: true },
    });

    if (!order)
      throw new Error('El pedido no existe.');

    if (order.userId !== parseInt(userId))
      throw new Error('No tienes permiso para modificar este pedido.');

    const updated = await prisma.order.update({
      where:   { id: parseInt(orderId) },
      data:    { status },
      include: { items: true },
    });

    logger.info(`Pedido #${orderId} actualizado al estado "${status}" por el usuario ${userId}`);
    return toResponseDto(updated);
  }
}

export default new OrderService();
