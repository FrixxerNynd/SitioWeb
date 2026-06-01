import prisma from "../config/db.js";
import ExelService from "./exel.service.js";
import logger from "../utils/logger.js";
import PedidoCreateDto from "../utils/DTO's/Request/PedidocreateDto.js";
import PedidoResponseDto from "../utils/DTO's/Response/PedidoResponseDto.js";

// Dirección estática de la sucursal central
const CABS_SUCURSAL_ADDRESS = {
  street: "Beatriz Prada",
  extNum: "450",
  intNum: "N/A",
  neighborhood: "Col. Jardines de Durango",
  city: "Victoria de Durango",
  state: "Durango",
  cp: "34020",
  country: "México",
};

class CartService {

  // Obtener carrito de un usuario con los cálculos financieros al día
  async getCart(userId) {
    let cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    // Inicializar carrito en DB si el usuario entra por primera vez
    if (!cart) {
      cart = await prisma.cart.create({
        data: { userId, paymentType: "Transferencia", subtotal: 0, total: 0 },
        include: { items: true },
      });
    }

    const updated = await this.recalculateCartTotals(cart.id);
    return new PedidoResponseDto(updated);
  }

  // PANTALLA 1: Agregar, modificar o remover ítems validando el stock de Exel del Norte
  async updateItemQuantity(userId, productId, quantity) {
    const cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    if (!cart) throw new Error("Carrito no encontrado para este usuario.");

    if (quantity <= 0) {
      // Eliminar ítem del carrito
      await prisma.cartItem.deleteMany({
        where: { cartId: cart.id, productId },
      });
    } else {
      // ✅ USA CACHÉ: Trae TODOS los productos de Redis (o de la API si no hay caché)
      //    El resultado ya viene en formato ProductosResponseDto[], NO en el formato crudo de la API
      const allProducts = await ExelService.fetchExternalProductsWithCache();

      // Busca el producto específico dentro del array cacheado
      const product = allProducts?.find((p) => p.id === productId);

      if (!product) {
        throw new Error("El producto seleccionado ya no existe en el catálogo.");
      }

      // Validar stock disponible
      const availableStock = parseInt(product.stock || 0);
      if (quantity > availableStock) {
        throw new Error(
          `Stock insuficiente. Solo quedan ${availableStock} unidades disponibles.`
        );
      }

      // Insertar o actualizar el ítem en el carrito
      const existingItem = cart.items.find((item) => item.productId === productId);

      if (existingItem) {
        await prisma.cartItem.update({
          where: { id: existingItem.id },
          data: {
            quantity,
            totalPrice: parseFloat(product.precio) * quantity,
          },
        });
      } else {
        await prisma.cartItem.create({
          data: {
            cartId: cart.id,
            productId,
            sku: product.sku,
            name: product.nombre,
            price: parseFloat(product.precio),
            quantity,
            totalPrice: parseFloat(product.precio) * quantity,
          },
        });
      }
    }

    const updated = await this.recalculateCartTotals(cart.id);
    return new PedidoResponseDto(updated);
  }

  // PANTALLA 1: Cambiar el tipo de pago asignado (Efectivo, Transferencia, Crédito)
  async setPaymentType(userId, paymentType) {
    const cart = await prisma.cart.findUnique({ where: { userId } });
    if (!cart) throw new Error("Carrito no encontrado.");

    const updated = await prisma.cart.update({
      where: { id: cart.id },
      data: { paymentType },
      include: { items: true },
    });
    return new PedidoResponseDto(updated);
  }

  // PANTALLA 2, 3 y 4: Selección de Entrega y cálculo preciso de flete
  async setDeliveryMethod(userId, method, addressId = null) {
    const cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });
    if (!cart) throw new Error("Carrito no encontrado.");

    let shippingAddress = null;
    let shippingCost = 0.0;

    if (method === "Sucursal") {
      shippingAddress = CABS_SUCURSAL_ADDRESS;
      shippingCost = 0.0; // Recoger en sucursal es gratis
    } else if (method === "Domicilio") {
      if (!addressId) {
        throw new Error(
          "Debe seleccionar una dirección registrada para la entrega a domicilio."
        );
      }

      const address = await prisma.userAddress.findFirst({
        where: { id: parseInt(addressId), userId },
      });
      if (!address) {
        throw new Error("La dirección de entrega seleccionada no existe.");
      }

      shippingAddress = address;
      // Regla: Envío menor a $2,000 genera un cargo por flete de $90.00
      shippingCost = cart.subtotal < 2000 ? 90.0 : 0.0;
    } else {
      throw new Error("Método de entrega inválido.");
    }

    await prisma.cart.update({
      where: { id: cart.id },
      data: {
        deliveryMethod: method,
        shippingCost,
        frozenAddress: JSON.stringify(shippingAddress),
      },
    });

    const updated = await this.recalculateCartTotals(cart.id);
    return new PedidoResponseDto(updated);
  }

  // PANTALLA 7: Checkout Final - Convierte el carrito en una orden persistente (Transaccional)
  async processCheckout(userId) {
    const cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    if (!cart) throw new Error("Carrito no encontrado.");
    if (!cart.items || cart.items.length === 0) {
      throw new Error("El carrito está vacío.");
    }
    if (!cart.deliveryMethod) {
      throw new Error("No ha seleccionado el método de entrega.");
    }

    const pedidoDto = new PedidoCreateDto({
      usuarioId: userId,
      productoIds: cart.items.map((i) => i.productId),
      subtotal: cart.subtotal,
      flete: cart.shippingCost,
      iva: 0,
      total: cart.total,
    });

    // Ejecutar checkout mediante una transacción ACID en base de datos
    return await prisma.$transaction(async (tx) => {
      // 1. Crear la Orden de Venta Final
      const order = await tx.order.create({
        data: {
          userId: pedidoDto.usuarioId,
          paymentType: cart.paymentType,
          deliveryMethod: cart.deliveryMethod,
          shippingCost: pedidoDto.flete,
          subtotal: pedidoDto.subtotal,
          total: pedidoDto.total,
          shippingAddress: cart.frozenAddress,
          status: "PAGADO_PENDIENTE_SURTIDO",
          items: {
            create: cart.items.map((item) => ({
              productId: item.productId,
              sku: item.sku,
              name: item.name,
              price: item.price,
              quantity: item.quantity,
              totalPrice: item.totalPrice,
            })),
          },
        },
        include: { items: true },
      });

      // 2. Limpiar items del carrito procesado
      await tx.cartItem.deleteMany({ where: { cartId: cart.id } });

      // 3. Resetear montos del carrito
      await tx.cart.update({
        where: { id: cart.id },
        data: {
          subtotal: 0,
          total: 0,
          deliveryMethod: null,
          frozenAddress: null,
          shippingCost: 0,
        },
      });

      logger.info(
        `Pedido #${order.id} generado exitosamente para el usuario ${userId}`
      );
      return new PedidoResponseDto(order);
    });
  }

  // Lógica interna reutilizable para el cálculo de totales financieros
  async recalculateCartTotals(cartId) {
    const cart = await prisma.cart.findUnique({
      where: { id: cartId },
      include: { items: true },
    });

    const subtotal = cart.items.reduce((acc, item) => acc + item.totalPrice, 0);
    let shippingCost = cart.shippingCost;

    if (cart.deliveryMethod === "Domicilio") {
      shippingCost = subtotal < 2000 ? 90.0 : 0.0;
    }

    const total = subtotal + shippingCost;

    return await prisma.cart.update({
      where: { id: cartId },
      data: { subtotal, shippingCost, total },
      include: { items: true },
    });
  }
}

export default new CartService();