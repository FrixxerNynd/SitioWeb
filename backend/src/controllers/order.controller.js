import orderService from "../services/order.service.js";
import cartService from "../services/cart.service.js";

// ── Helper: extrae y valida el userId del token JWT ──
const getUserId = (req) => {
  const id = parseInt(req.user?.id);
  if (isNaN(id)) throw new Error("Token inválido: userId no encontrado");
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
        parseInt(req.params.id),
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // PATCH /api/orders/:id/status (público, sin auth)
  async updateOrderStatus(req, res, next) {
    try {
      const { userId, status } = req.body;
      const result = await orderService.updateOrderStatus(
        userId ? parseInt(userId) : null,
        parseInt(req.params.id),
        status,
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // POST /api/orders/checkout
  async checkout(req, res, next) {
    try {
      const result = await cartService.processCheckout(getUserId(req));
      res.status(201).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // DELETE /api/orders/:id
  async deleteOrder(req, res, next) {
    try {
      await orderService.deleteOrder(getUserId(req), parseInt(req.params.id));
      res
        .status(200)
        .json({ success: true, message: "Pedido eliminado exitosamente" });
    } catch (error) {
      next(error);
    }
  }

  // GET /api/orders/summary/total
  async getExistingOrders(req, res, next) {
    try {
      const result = await orderService.getExistingOrders(getUserId(req));
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // GET /api/orders/all — Todas las órdenes (sin filtro por usuario)
  async getAllOrders(req, res, next) {
    try {
      const result = await orderService.getAllOrders();
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }
}

export default new OrderController();
