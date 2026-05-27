import CartService from '../services/cart.services';

class CartController {

    async fetchCart(req, res, next) {
        try {
            // Suponiendo que el middleware de autenticación inyecta req.user
            const userId = req.user?.id || 1; 
            const cart = await CartService.getCart(userId);
            return res.status(200).json(cart);
        } catch (error) {
            next(error);
        }
    }

    async updateCartItem(req, res, next) {
        try {
            const userId = req.user?.id || 1;
            const { productId, quantity } = req.body;
            const updatedCart = await CartService.updateItemQuantity(userId, productId, quantity);
            return res.status(200).json(updatedCart);
        } catch (error) {
            next(error);
        }
    }

    async configurePayment(req, res, next) {
        try {
            const userId = req.user?.id || 1;
            const { paymentType } = req.body;
            const updatedCart = await CartService.setPaymentType(userId, paymentType);
            return res.status(200).json(updatedCart);
        } catch (error) {
            next(error);
        }
    }

    async configureDelivery(req, res, next) {
        try {
            const userId = req.user?.id || 1;
            const { method, addressId } = req.body;
            const updatedCart = await CartService.setDeliveryMethod(userId, method, addressId);
            return res.status(200).json(updatedCart);
        } catch (error) {
            next(error);
        }
    }

    async checkout(req, res, next) {
        try {
            const userId = req.user?.id || 1;
            const orderReceipt = await CartService.processCheckout(userId);
            return res.status(201).json({
                message: "¡Orden de compra generada con éxito en CABS Computación!",
                order: orderReceipt
            });
        } catch (error) {
            next(error);
        }
    }
}

export default new CartController();