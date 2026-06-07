import { Router } from "express";
import cartController from "../controllers/cart.controller.js";
import { validateToken } from "../middlewares/JWTValidator.js";  // ← nuevo

const cartRouter = Router();

// ← Si USE_AUTH=true usa el middleware real, si no usa uno falso
// const authMiddleware = process.env.USE_AUTH === "true"
//   ? validateToken
//   : (req, res, next) => {
//       req.user = { id: 1 };
//       next();
//     };

cartRouter.use(validateToken);   

// ═══════════════════════════════════════════════════════════
//  RUTAS - Endpoints del carrito con JSDoc Swagger
// ═══════════════════════════════════════════════════════════

/**
 * @swagger
 * tags:
 *   name: Carrito
 *   description: Gestión del carrito de compras CABS
 */

/**
 * @swagger
 * /api/cart:
 *   get:
 *     summary: Obtener el carrito del usuario autenticado
 *     tags: [Carrito]
 *     security:
 *       - cokieAuth: []
 *     responses:
 *       200:
 *         description: Carrito obtenido exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/PedidoResponse'
 *       500:
 *         description: Error interno del servidor
 */
cartRouter.get("/", cartController.getCart.bind(cartController));

/**
 * @swagger
 * /api/cart/item/{productId}:
 *   get:
 *     summary: Obtener un ítem específico del carrito
 *     tags: [Carrito]
 *     security:
 *       - cokieAuth: []
 *     parameters:
 *       - in: path
 *         name: productId
 *         required: true
 *         schema:
 *           type: integer
 *         description: ID del producto a co nsultar
 *     responses:
 *       200:
 *         description: Ítem encontrado
 *       404:
 *         description: El producto no se encuentra en el carrito
 *       500:
 *         description: Error interno del servidor
 */
cartRouter.get("/item/:productId", cartController.getCartItem.bind(cartController));

/**
 * @swagger
 * /api/cart/item:
 *   post:
 *     summary: Agregar un producto nuevo al carrito
 *     tags: [Carrito]
 *     security:
 *       - cokieAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [productId, quantity]
 *             properties:
 *               productId:
 *                 type: integer
 *                 example: 101
 *               quantity:
 *                 type: integer
 *                 example: 2
 *     responses:
 *       201:
 *         description: Producto agregado al carrito
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/PedidoResponse'
 *       400:
 *         description: Producto ya en carrito o stock insuficiente
 *       500:
 *         description: Error interno del servidor
 */
cartRouter.post("/item", cartController.addItem.bind(cartController));

/**
 * @swagger
 * /api/cart/item/{productId}:
 *   put:
 *     summary: Modificar la cantidad de un ítem (quantity=0 lo elimina)
 *     tags: [Carrito]
 *     security:
 *       - cokieAuth: []
 *     parameters:
 *       - in: path
 *         name: productId
 *         required: true
 *         schema:
 *           type: integer
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [quantity]
 *             properties:
 *               quantity:
 *                 type: integer
 *                 example: 3
 *     responses:
 *       200:
 *         description: Cantidad actualizada exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/PedidoResponse'
 *       400:
 *         description: Stock insuficiente
 *       500:
 *         description: Error interno del servidor
 */
cartRouter.put("/item/:productId", cartController.updateItemQuantity.bind(cartController));

/**
 * @swagger
 * /api/cart/item/{productId}:
 *   delete:
 *     summary: Eliminar un producto del carrito
 *     tags: [Carrito]
 *     security:
 *       - cokieAuth: []
 *     parameters:
 *       - in: path
 *         name: productId
 *         required: true
 *         schema:
 *           type: integer
 *         description: ID del producto a eliminar
 *     responses:
 *       200:
 *         description: Producto eliminado del carrito
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/PedidoResponse'
 *       500:
 *         description: Error interno del servidor
 */
cartRouter.delete("/item/:productId", cartController.removeItem.bind(cartController));

/**
 * @swagger
 * /api/cart/payment:
 *   patch:
 *     summary: Cambiar el tipo de pago del carrito
 *     tags: [Carrito]
 *     security:
 *       - cokieAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [paymentType]
 *             properties:
 *               paymentType:
 *                 type: string
 *                 enum: [Efectivo, Transferencia, Crédito]
 *                 example: Efectivo
 *     responses:
 *       200:
 *         description: Tipo de pago actualizado
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/PedidoResponse'
 *       500:
 *         description: Error interno del servidor
 */
cartRouter.patch("/payment", cartController.setPaymentType.bind(cartController));

/**
 * @swagger
 * /api/cart/delivery:
 *   patch:
 *     summary: Seleccionar método de entrega y calcular flete
 *     tags: [Carrito]
 *     security:
 *       - cokieAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [method]
 *             properties:
 *               method:
 *                 type: string
 *                 enum: [Sucursal, Domicilio]
 *                 example: Domicilio
 *               addressId:
 *                 type: integer
 *                 example: 5
 *                 description: Requerido solo si method es "Domicilio"
 *     responses:
 *       200:
 *         description: Método de entrega asignado y flete calculado
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/PedidoResponse'
 *       400:
 *         description: Dirección no encontrada o método inválido
 *       500:
 *         description: Error interno del servidor
 */
cartRouter.patch("/delivery", cartController.setDeliveryMethod.bind(cartController));

/**
 * @swagger
 * /api/cart/checkout:
 *   post:
 *     summary: Procesar el checkout y generar una Orden de Venta
 *     tags: [Carrito]
 *     security:
 *       - cokieAuth: []
 *     responses:
 *       201:
 *         description: Pedido generado exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/PedidoResponse'
 *       400:
 *         description: Carrito vacío o sin método de entrega
 *       500:
 *         description: Error interno del servidor
 */
cartRouter.post("/checkout", cartController.processCheckout.bind(cartController));

export default cartRouter;
