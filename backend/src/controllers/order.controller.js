import orderService from '../services/order.service.js';

// ── Helper: extrae y valida el userId del token JWT ──
const getUserId = (req) => {
  const id = parseInt(req.user?.id);
  if (isNaN(id)) throw new Error('Token inválido: userId no encontrado');
  return id;
};

// ═══════════════════════════════════════════════════════════
//  CONTROLADOR - Manejo de requests y responses HTTP
// ═══════════════════════════════════════════════════════════

class OrderController {

  // GET /api/orders
  async getOrders(req, res, next) {
    try {
      const result = await orderService.getOrders(getUserId(req));
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // GET /api/orders/:id
  async getOrderById(req, res, next) {
    try {
      const result = await orderService.getOrderById(
        getUserId(req),
        parseInt(req.params.id)
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // PATCH /api/orders/:id/status
  async updateOrderStatus(req, res, next) {
    try {
      const result = await orderService.updateOrderStatus(
        getUserId(req),
        parseInt(req.params.id),
        req.body.status
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }
}

export default new OrderController();
