import prisma from "../config/db.js";
import ExelService from "./exelService.js";
import logger from "../utils/Helpers/logger.js";
import PedidoCreateDto from "../utils/DTO's/Request/Pedidos/PedidoCreateDto.js";
import PedidoResponseDto from "../utils/DTO's/Response/Pedidos/PedidoResponseDto.js";

// ── Adaptador: traduce el objeto de Prisma al formato que espera PedidoResponseDto
const toResponseDto = (data) =>
  new PedidoResponseDto({
    id: data.id,
    estado: data.status ?? data.estado ?? "",
    fechaPedido: data.createdAt ?? data.fechaPedido ?? new Date(),
    clienteNombre: data.name ?? "",
    transportista: data.deliveryMethod ?? "",
    numeroFactura: data.id?.toString() ?? "",
    productos: (data.items ?? []).map((i) => `${i.name} x${i.quantity}`),
    subtotal: data.subtotal ?? 0,
    flete: data.shippingCost ?? 0,
    iva: 0,
    total: data.total ?? 0,
  });

// Dirección estática de la sucursal
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

// ═══════════════════════════════════════════════════════════
//  SERVICIO - Lógica de negocio del carrito
// ═══════════════════════════════════════════════════════════

class CartService {
  // ─── READ - Obtener el carrito completo del usuario ───
  // backend/src/services/cart.service.js

  // ─── READ - Obtener el carrito completo del usuario ───
  async getCart(userId, userName) {
    let cart = await prisma.cart.findUnique({
      where: {
        userId: parseInt(userId),
      },
      include: { items: true },
    });

    // Inicializar carrito en DB si el usuario entra por primera vez
    if (!cart) {
      cart = await prisma.cart.create({
        data: {
          userId: parseInt(userId),
          paymentType: "Transferencia",
          subtotal: 0,
          total: 0,
        },
        include: { items: true },
      });
    }

    const updated = await this.recalculateCartTotals(cart.id);

    // 🔥 MODIFICAR: Construir respuesta con productId incluido
    const items = updated.items || [];

    // Crear array de strings con formato "Nombre xCantidad" (para compatibilidad)
    const productosStrings = items.map((i) => `${i.name} x${i.quantity}`);

    // 🔥 CREAR NUEVO ARRAY CON PRODUCTOS DETALLADOS (incluye productId)
    const productosDetallados = items.map((i) => ({
      productId: i.productId,
      sku: i.sku,
      name: i.name,
      price: i.price,
      quantity: i.quantity,
      totalPrice: i.totalPrice,
    }));

    // Devolver respuesta con ambos formatos
    return {
      id: updated.id,
      estado: updated.status ?? updated.estado ?? "",
      fechaPedido: updated.createdAt ?? updated.fechaPedido ?? new Date(),
      clienteNombre: userName || `Usuario ${userId}`,
      transportista: updated.deliveryMethod ?? "",
      numeroFactura: updated.id?.toString() ?? "",
      // 🔥 FORMATO ORIGINAL (strings) - para compatibilidad
      productos: productosStrings,
      // 🔥 NUEVO FORMATO (objetos con productId)
      items: productosDetallados,
      subtotal: updated.subtotal ?? 0,
      flete: updated.shippingCost ?? 0,
      iva: 0,
      total: updated.total ?? 0,
    };
  }

  // ─── READ - Obtener un ítem específico del carrito por productId ───
  async getCartItem(userId, productId) {
    const cart = await prisma.cart.findUnique({
      where: { userId: parseInt(userId) }, // ← userId ya es número, no objeto
      include: { items: true },
    });

    if (!cart) throw new Error("El carrito del usuario no existe.");

    const item = cart.items.find((i) => i.productId === productId);
    if (!item) throw new Error("El producto no se encuentra en el carrito.");

    return item;
  }

  // ─── CREATE - Agregar un producto nuevo al carrito ───

  async addItem(userId, productId, quantity) {
    if (!productId || String(productId).trim() === "") {
      throw new Error("El productId es obligatorio.");
    }
    if (!Number.isInteger(quantity) || quantity <= 0) {
      throw new Error("La cantidad debe ser un número entero mayor a 0.");
    }

    let cart = await prisma.cart.findUnique({
      where: { userId: parseInt(userId) },
      include: { items: true },
    });

    if (!cart) {
      cart = await prisma.cart.create({
        data: {
          userId: parseInt(userId),
          paymentType: "Transferencia",
          subtotal: 0,
          total: 0,
        },
        include: { items: true },
      });
    }

    // Verificar si el producto ya está en el carrito
    const alreadyInCart = cart.items.find((i) => i.productId === productId);
    if (alreadyInCart) {
      throw new Error(
        "El producto ya está en el carrito. Use updateItemQuantity para modificar la cantidad.",
      );
    }

    // Obtener producto completo (incluyendo precio y stock)
    const product = await ExelService.getProductByReference(productId);
    if (!product) {
      throw new Error("El producto seleccionado no existe en el catálogo.");
    }

    // Verificar que tenemos precio y stock
    if (product.stock === undefined || product.precio === undefined) {
      console.error(`Producto ${productId} no tiene precio/stock en Redis`);
      throw new Error("El producto no tiene precio o stock disponible.");
    }

    const availableStock = parseInt(product.stock) || 0;
    if (quantity > availableStock) {
      throw new Error(
        `Stock insuficiente. Solo quedan ${availableStock} unidades disponibles.`,
      );
    }

    const price = parseFloat(product.precio) || 0;
    if (price <= 0) {
      throw new Error("El producto no tiene un precio válido.");
    }

    console.log(
      `🛒 Agregando producto: ${product.nombre}, Precio: ${price}, Stock: ${availableStock}`,
    );

    // Crear el item en el carrito
    await prisma.cartItem.create({
      data: {
        cartId: cart.id,
        productId: productId,
        sku: product.sku || productId,
        name: product.nombre || "Producto",
        price: price,
        quantity: quantity,
        totalPrice: price * quantity,
      },
    });

    const updated = await this.recalculateCartTotals(cart.id);
    return toResponseDto(updated);
  }

  // ─── UPDATE - Modificar la cantidad de un ítem ya existente ───
  async updateItemQuantity(userId, productId, quantity) {
    if (!productId || String(productId).trim() === "") {
      throw new Error("El productId es obligatorio.");
    }
    if (!Number.isInteger(quantity)) {
      throw new Error("La cantidad debe ser un número entero válido.");
    }

    const cart = await prisma.cart.findUnique({
      where: { userId: parseInt(userId) },
      include: { items: true },
    });

    if (!cart) throw new Error("El carrito del usuario no existe.");

    if (quantity <= 0) {
      return await this.removeItem(userId, productId);
    }

    const externalProduct = await ExelService.getProductByReference(productId);

    if (!externalProduct)
      throw new Error("El producto seleccionado ya no existe en el catálogo.");

    const availableStock = parseInt(externalProduct.stock || 0);
    if (quantity > availableStock) {
      throw new Error(
        `Stock insuficiente. Solo quedan ${availableStock} unidades disponibles.`,
      );
    }

    const existingItem = cart.items.find(
      (item) => item.productId === productId,
    );

    if (existingItem) {
      await prisma.cartItem.update({
        where: { id: existingItem.id },
        data: {
          quantity,
          totalPrice: parseFloat(externalProduct.precio) * quantity,
        },
      });
    } else {
      return await this.addItem(userId, productId, quantity);
    }

    const updated = await this.recalculateCartTotals(cart.id);
    return toResponseDto(updated);
  }

  // ─── UPDATE - Cambiar el tipo de pago del carrito ───
  async setPaymentType(userId, paymentType) {
    const TIPOS_VALIDOS = ["Efectivo", "Transferencia", "Crédito"];
    if (!paymentType || !TIPOS_VALIDOS.includes(paymentType)) {
      throw new Error(
        "Tipo de pago inválido. Use: " + TIPOS_VALIDOS.join(", "),
      );
    }

    const cart = await prisma.cart.findUnique({
      where: { userId: parseInt(userId) },
    });
    if (!cart) throw new Error("El carrito del usuario no existe.");

    const updated = await prisma.cart.update({
      where: { id: cart.id },
      data: { paymentType },
      include: { items: true },
    });
    return toResponseDto(updated);
  }
  // ─── DELETE - Eliminar un producto específico del carrito ───
  async removeItem(userId, productId) {
    const cart = await prisma.cart.findUnique({
      where: { userId: parseInt(userId) },
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
    const cart = await prisma.cart.findUnique({
      where: { userId: parseInt(userId) },
    });
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
      where: { userId: parseInt(userId) },
      include: { items: true },
    });

    if (!cart) throw new Error("El carrito del usuario no existe.");

    let shippingAddress = null;
    let shippingCost = 0.0;

    if (method === "Sucursal") {
      shippingAddress = CABS_SUCURSAL_ADDRESS;
      shippingCost = 0.0; // Recoger en sucursal es gratis
    } else if (method === "Domicilio") {
      if (!addressId)
        throw new Error(
          "Debe seleccionar una dirección registrada para la entrega a domicilio.",
        );

      const address = await prisma.userAddress.findFirst({
        where: { id: parseInt(addressId), userId: parseInt(userId) },
      });
      if (!address)
        throw new Error("La dirección de entrega seleccionada no existe.");

      shippingAddress = address;
      const subtotalActual = cart.items.reduce(
        (acc, item) => acc + item.totalPrice,
        0,
      );
      shippingCost = subtotalActual < 2000 ? 90.0 : 0.0;
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
      where: { userId: parseInt(userId) },
      include: { items: true },
    });

    if (!cart || !cart.items || cart.items.length === 0)
      throw new Error("El carrito está vacío.");
    if (!cart.deliveryMethod)
      throw new Error("No ha seleccionado el método de entrega.");

    const pedidoDto = new PedidoCreateDto({
      usuarioId: userId,
      productoIds: cart.items.map((i) => i.productId),
      subtotal: cart.subtotal,
      flete: cart.shippingCost,
      iva: 0,
      total: cart.total,
    });

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

      // 2. Limpiar ítems del carrito ya procesado
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
        `Pedido #${order.id} generado exitosamente para el usuario ${userId}`,
      );
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
