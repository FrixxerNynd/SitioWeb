import prisma from '../config/db';
import ExelService from './exel.service';
import logger from '../utils/logger';

// Dirección estática de la sucursal central obtenida de los requerimientos de diseño (Pág. 3)
const CABS_SUCURSAL_ADDRESS = {
    street: "Beatriz Prada", // [cite: 118, 119]
    extNum: "450", // [cite: 126, 127]
    intNum: "N/A",
    neighborhood: "Col. Jardines de Durango", // [cite: 120, 121]
    city: "Victoria de Durango", // [cite: 128, 129]
    state: "Durango", // [cite: 122, 123]
    cp: "34020", // [cite: 130, 132]
    country: "México" // [cite: 131, 133]
};

class CartService {

    // Obtener carrito de un usuario con los cálculos financieros al día
    async getCart(userId) {
        let cart = await prisma.cart.findUnique({
            where: { userId },
            include: { items: true }
        });

        // Inicializar carrito en DB si el usuario entra por primera vez
        if (!cart) {
            cart = await prisma.cart.create({
                data: { userId, paymentType: 'Transferencia', subtotal: 0, total: 0 },
                include: { items: true }
            });
        }

        return await this.recalculateCartTotals(cart.id);
    }

    // PANTALLA 1: Agregar, modificar o remover ítems validando el stock de Exel del Norte
    async updateItemQuantity(userId, productId, quantity) {
        const cart = await this.getCart(userId);

        if (quantity <= 0) {
            // Eliminar ítem del carrito (Acción "Quitar" en PDF)
            await prisma.cartItem.deleteMany({
                where: { cartId: cart.id, productId }
            });
        } else {
            // Regla de Negocio: Validar existencias reales en tiempo real con el catálogo externo
            const externalData = await ExelService.fetchExternalProducts({ id: productId });
            const externalProduct = externalData?.datos?.find(p => p.id === productId);
            
            if (!externalProduct) throw new Error("El producto seleccionado ya no existe en el catálogo.");
            
            const availableStock = parseInt(externalProduct.stock || 0); //
            if (quantity > availableStock) {
                throw new Error(`Stock insuficiente. Solo quedan ${availableStock} unidades disponibles.`);
            }

            const existingItem = cart.items.find(item => item.productId === productId);

            if (existingItem) {
                await prisma.cartItem.update({
                    where: { id: existingItem.id },
                    data: { quantity, totalPrice: parseFloat(externalProduct.precio) * quantity }
                });
            } else {
                await prisma.cartItem.create({
                    data: {
                        cartId: cart.id,
                        productId,
                        sku: externalProduct.sku, //
                        name: externalProduct.nombre, //
                        price: parseFloat(externalProduct.precio), //
                        quantity,
                        totalPrice: parseFloat(externalProduct.precio) * quantity
                    }
                });
            }
        }

        return await this.recalculateCartTotals(cart.id);
    }

    // PANTALLA 1: Cambiar el tipo de pago asignado (Efectivo, Transferencia, Crédito)
    async setPaymentType(userId, paymentType) {
        const cart = await this.getCart(userId);
        return await prisma.cart.update({
            where: { id: cart.id },
            data: { paymentType },
            include: { items: true }
        });
    }

    // PANTALLA 2, 3 y 4: Selección de Entrega y cálculo preciso de flete
    async setDeliveryMethod(userId, method, addressId = null) {
        const cart = await this.getCart(userId);
        let shippingAddress = null;
        let shippingCost = 0.00;

        if (method === "Sucursal") {
            shippingAddress = CABS_SUCURSAL_ADDRESS;
            shippingCost = 0.00; // Recoger en sucursal es gratis según diseño [cite: 42, 114]
        } else if (method === "Domicilio") {
            if (!addressId) throw new Error("Debe seleccionar una dirección registrada para la entrega a domicilio.");
            
            const address = await prisma.userAddress.findFirst({
                where: { id: parseInt(addressId), userId }
            });
            if (!address) throw new Error("La dirección de entrega seleccionada no existe.");

            shippingAddress = address;
            // Regla del PDF: Envío menor a $2,000 genera un cargo por flete de $90.00 
            shippingCost = cart.subtotal < 2000 ? 90.00 : 0.00; // [cite: 85, 225, 269]
        } else {
            throw new Error("Método de entrega inválido.");
        }

        await prisma.cart.update({
            where: { id: cart.id },
            data: {
                deliveryMethod: method,
                shippingCost,
                // Almacenamos la estructura completa de dirección de forma estática en formato JSON para el pedido
                frozenAddress: JSON.stringify(shippingAddress) 
            }
        });

        return await this.recalculateCartTotals(cart.id);
    }

    // PANTALLA 7: Checkout Final - Convierte el carrito en una orden persistente (Transaccional)
    async processCheckout(userId) {
        const cart = await this.getCart(userId);

        if (!cart.items || cart.items.length === 0) throw new Error("El carrito está vacío.");
        if (!cart.deliveryMethod) throw new Error("No ha seleccionado el método de entrega.");

        // Ejecutar checkout mediante una transacción ACID en base de datos
        return await prisma.$transaction(async (tx) => {
            
            // 1. Crear la Orden de Venta Final
            const order = await tx.order.create({
                data: {
                    userId,
                    paymentType: cart.paymentType,
                    deliveryMethod: cart.deliveryMethod,
                    shippingCost: cart.shippingCost,
                    subtotal: cart.subtotal,
                    total: cart.total,
                    shippingAddress: cart.frozenAddress,
                    status: "PAGADO_PENDIENTE_SURTIDO",
                    items: {
                        create: cart.items.map(item => ({
                            productId: item.productId,
                            sku: item.sku,
                            name: item.name,
                            price: item.price,
                            quantity: item.quantity,
                            totalPrice: item.totalPrice
                        }))
                    }
                },
                include: { items: true }
            });

            // 2. Limpiar items del carrito del usuario ya procesado
            await tx.cartItem.deleteMany({ where: { cartId: cart.id } });
            
            // 3. Resetear montos del contenedor del carrito
            await tx.cart.update({
                where: { id: cart.id },
                data: { subtotal: 0, total: 0, deliveryMethod: null, frozenAddress: null, shippingCost: 0 }
            });

            logger.info(`Pedido #${order.id} generado exitosamente para el usuario ${userId}`);
            return order;
        });
    }

    // Lógica interna reutilizable para el cálculo de totales financieros en tiempo real
    async recalculateCartTotals(cartId) {
        const cart = await prisma.cart.findUnique({
            where: { id: cartId },
            include: { items: true }
        });

        const subtotal = cart.items.reduce((acc, item) => acc + item.totalPrice, 0);
        let shippingCost = cart.shippingCost;

        if (cart.deliveryMethod === "Domicilio") {
            shippingCost = subtotal < 2000 ? 90.00 : 0.00; // 
        }

        const total = subtotal + shippingCost;

        return await prisma.cart.update({
            where: { id: cartId },
            data: { subtotal, shippingCost, total },
            include: { items: true }
        });
    }
}

export default new CartService();