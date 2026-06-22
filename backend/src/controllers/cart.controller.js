import cartService from "../services/cart.service.js";


const getUserId = (req) => {
  const id = parseInt(req.user?.id);
  if (isNaN(id)) throw new Error("Token inválido: userId no encontrado");
  return id;
};
// ═══════════════════════════════════════════════════════════
//  CONTROLADOR - Manejo de requests y responses HTTP
// ═══════════════════════════════════════════════════════════

class CartController {
  // GET /api/cart
  async getCart(req, res, next) {
    console.log("Usuario del token: ", req?.user)
    try {

      const result = await cartService.getCart(getUserId(req), req.user?.name);
      res.status(200).json({ success: true, data: result });

    } catch (error) {
      next(error);
    }
  }

  

  // GET /api/cart/item/:productId
  async getCartItem(req, res, next) {
    try {
      const result = await cartService.getCartItem(
        getUserId(req),
        req.params.productId
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // POST /api/cart/item
// backend/src/controllers/cart.controller.js

async addItem(req, res, next) {
    try {
        const { productId, quantity } = req.body;
        
        console.log('📥 Recibiendo solicitud addItem:');
        console.log('  productId:', productId);
        console.log('  quantity:', quantity);
        
        const result = await cartService.addItem(
            getUserId(req),
            productId,
            parseInt(quantity)
        );
        
        console.log('✅ Producto agregado correctamente');
        res.status(201).json({ success: true, data: result });
    } catch (error) {
        console.error('❌ Error en addItem:', error.message);
        next(error);
    }
}

  // PUT /api/cart/item/:productId
  async updateItemQuantity(req, res, next) {
    try {
      const result = await cartService.updateItemQuantity(
        getUserId(req),
        req.params.productId,
        parseInt(req.body.quantity)
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // DELETE /api/cart/item/:productId
  async removeItem(req, res, next) {
    try {
      const result = await cartService.removeItem(
        getUserId(req),
        req.params.productId
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // PATCH /api/cart/payment
  async setPaymentType(req, res, next) {
    try {
      const result = await cartService.setPaymentType(
        getUserId(req),
        req.body.paymentType
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // PATCH /api/cart/delivery
  async setDeliveryMethod(req, res, next) {
    try {
      const { method, addressId } = req.body;
      const result = await cartService.setDeliveryMethod(
        getUserId(req),
        method,
        addressId
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // POST /api/cart/checkout
  async processCheckout(req, res, next) {
    try {
      const result = await cartService.processCheckout(getUserId(req));
      res.status(201).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }
}

export default new CartController();