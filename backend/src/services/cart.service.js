import prisma from "../config/db.js";
import ExelService from "./exel.service.js";
import logger from "../utils/logger.js";
import PedidoCreateDto from "../utils/DTO's/Request/PedidoCreateDto.js";
import PedidoResponseDto from "../utils/DTO's/Response/PedidoResponseDto.js";

// ── Adaptador: traduce el objeto de Prisma al formato que espera PedidoResponseDto
const toResponseDto = (data) => new PedidoResponseDto({
  id:            data.id,
  estado:        data.status         ?? data.estado       ?? '',
  fechaPedido:   data.createdAt      ?? data.fechaPedido  ?? new Date(),
  clienteNombre: data.userId?.toString() ?? '',
  transportista: data.deliveryMethod ?? '',
  numeroFactura: data.id?.toString() ?? '',
  productos:     (data.items ?? []).map(i => `${i.name} x${i.quantity}`),
  subtotal:      data.subtotal       ?? 0,
  flete:         data.shippingCost   ?? 0,
  iva:           0,
  total:         data.total          ?? 0,
});

// Dirección estática de la sucursal central obtenida de los requerimientos de diseño (Pág. 3)
const CABS_SUCURSAL_ADDRESS = {
  street:       "Beatriz Prada",
  extNum:       "450",
  intNum:       "N/A",
  neighborhood: "Col. Jardines de Durango",
  city:         "Victoria de Durango",
  state:        "Durango",
  cp:           "34020",
  country:      "México",
};

// ═══════════════════════════════════════════════════════════
//  SERVICIO - Lógica de negocio del carrito
// ═══════════════════════════════════════════════════════════

class CartService {

  // ─── READ - Obtener el carrito completo del usuario ───
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
    return toResponseDto(updated);
  }

  // ─── READ - Obtener un ítem específico del carrito por productId ───
  async getCartItem(userId, productId) {
    const cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    if (!cart) throw new Error("El carrito del usuario no existe.");

    const item = cart.items.find((i) => i.productId === productId);
    if (!item) throw new Error("El producto no se encuentra en el carrito.");

    return item;
  }

  // ─── CREATE - Agregar un producto nuevo al carrito ───
  async addItem(userId, productId, quantity) {
    if (quantity <= 0) throw new Error("La cantidad debe ser mayor a 0.");

    let cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    // Crear carrito si todavía no existe
    if (!cart) {
      cart = await prisma.cart.create({
        data: { userId, paymentType: "Transferencia", subtotal: 0, total: 0 },
        include: { items: true },
      });
    }

    const alreadyInCart = cart.items.find((i) => i.productId === productId);
    if (alreadyInCart) {
      throw new Error(
        "El producto ya está en el carrito. Use updateItemQuantity para modificar la cantidad."
      );
    }

    // Validar existencias reales en el catálogo externo
    const externalData = await ExelService.fetchExternalProducts({ id: productId });
    const externalProduct = externalData?.find((p) => p.id === productId);

    if (!externalProduct)
      throw new Error("El producto seleccionado ya no existe en el catálogo.");

    const availableStock = parseInt(externalProduct.stock || 0);
    if (quantity > availableStock) {
      throw new Error(
        `Stock insuficiente. Solo quedan ${availableStock} unidades disponibles.`
      );
    }

    await prisma.cartItem.create({
      data: {
        cartId:     cart.id,
        productId,
        sku:        externalProduct.sku,
        name:       externalProduct.nombre,
        price:      parseFloat(externalProduct.precio),
        quantity,
        totalPrice: parseFloat(externalProduct.precio) * quantity,
      },
    });

    const updated = await this.recalculateCartTotals(cart.id);
    return toResponseDto(updated);
  }

  // ─── UPDATE - Modificar la cantidad de un ítem ya existente ───
  async updateItemQuantity(userId, productId, quantity) {
    const cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    if (!cart) throw new Error("El carrito del usuario no existe.");

    // Si llega quantity 0 o menor se delega al DELETE
    if (quantity <= 0) {
      return await this.removeItem(userId, productId);
    }

    // Validar existencias reales en el catálogo externo
    const externalData = await ExelService.fetchExternalProducts({ id: productId });
    const externalProduct = externalData?.find((p) => p.id === productId);

    if (!externalProduct)
      throw new Error("El producto seleccionado ya no existe en el catálogo.");

    const availableStock = parseInt(externalProduct.stock || 0);
    if (quantity > availableStock) {
      throw new Error(
        `Stock insuficiente. Solo quedan ${availableStock} unidades disponibles.`
      );
    }

    const existingItem = cart.items.find((item) => item.productId === productId);

    if (existingItem) {
      await prisma.cartItem.update({
        where: { id: existingItem.id },
        data: {
          quantity,
          totalPrice: parseFloat(externalProduct.precio) * quantity,
        },
      });
    } else {
      // Si el ítem no existe aún se delega al CREATE
      return await this.addItem(userId, productId, quantity);
    }

    const updated = await this.recalculateCartTotals(cart.id);
    return toResponseDto(updated);
  }

  // ─── DELETE - Eliminar un producto específico del carrito ───
  async removeItem(userId, productId) {
    const cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    if (!cart) throw new Error("El carrito del usuario no existe.");

    await prisma.cartItem.deleteMany({
      where: { cartId: cart.id, productId },
    });

    const updated = await this.recalculateCartTotals(cart.id);
    return toResponseDto(updated);
  }

  // ─── UPDATE - Cambiar el tipo de pago del carrito ───
  async setPaymentType(userId, paymentType) {
    const cart = await prisma.cart.findUnique({ where: { userId } });
    if (!cart) throw new Error("El carrito del usuario no existe.");

    const updated = await prisma.cart.update({
      where: { id: cart.id },
      data: { paymentType },
      include: { items: true },
    });
    return toResponseDto(updated);
  }

  // ─── UPDATE - Seleccionar método de entrega y calcular flete ───
  async setDeliveryMethod(userId, method, addressId = null) {
    const cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    if (!cart) throw new Error("El carrito del usuario no existe.");

    let shippingAddress = null;
    let shippingCost    = 0.0;

    if (method === "Sucursal") {
      shippingAddress = CABS_SUCURSAL_ADDRESS;
      shippingCost    = 0.0; // Recoger en sucursal es gratis

    } else if (method === "Domicilio") {
      if (!addressId)
        throw new Error(
          "Debe seleccionar una dirección registrada para la entrega a domicilio."
        );

      const address = await prisma.userAddress.findFirst({
        where: { id: parseInt(addressId), userId },
      });
      if (!address)
        throw new Error("La dirección de entrega seleccionada no existe.");

      shippingAddress = address;
      // Regla del PDF: envío menor a $2,000 genera cargo de $90.00
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
    return toResponseDto(updated);
  }

  // ─── CREATE - Checkout: convierte el carrito en una Orden persistente (transaccional) ───
  async processCheckout(userId) {
    const cart = await prisma.cart.findUnique({
      where: { userId },
      include: { items: true },
    });

    if (!cart || !cart.items || cart.items.length === 0)
      throw new Error("El carrito está vacío.");
    if (!cart.deliveryMethod)
      throw new Error("No ha seleccionado el método de entrega.");

    const pedidoDto = new PedidoCreateDto({
      usuarioId:   userId,
      productoIds: cart.items.map((i) => i.productId),
      subtotal:    cart.subtotal,
      flete:       cart.shippingCost,
      iva:         0,
      total:       cart.total,
    });

    return await prisma.$transaction(async (tx) => {
      // 1. Crear la Orden de Venta Final
      const order = await tx.order.create({
        data: {
          userId:          pedidoDto.usuarioId,
          paymentType:     cart.paymentType,
          deliveryMethod:  cart.deliveryMethod,
          shippingCost:    pedidoDto.flete,
          subtotal:        pedidoDto.subtotal,
          total:           pedidoDto.total,
          shippingAddress: cart.frozenAddress,
          status:          "PAGADO_PENDIENTE_SURTIDO",
          items: {
            create: cart.items.map((item) => ({
              productId:  item.productId,
              sku:        item.sku,
              name:       item.name,
              price:      item.price,
              quantity:   item.quantity,
              totalPrice: item.totalPrice,
            })),
          },
        },
        include: { items: true },
      });

      // 2. Limpiar ítems del carrito ya procesado
      await tx.cartItem.deleteMany({ where: { cartId: cart.id } });

      // 3. Resetear montos del carrito
      await tx.cart.update({
        where: { id: cart.id },
        data: {
          subtotal:       0,
          total:          0,
          deliveryMethod: null,
          frozenAddress:  null,
          shippingCost:   0,
        },
      });

      logger.info(`Pedido #${order.id} generado exitosamente para el usuario ${userId}`);
      return toResponseDto(order);
    });
  }

  // ─── UPDATE (interno) - Recalcular totales financieros del carrito ───
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
