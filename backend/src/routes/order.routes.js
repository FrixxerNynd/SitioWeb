import { Router } from 'express';
import orderController from '../controllers/order.controller.js';
import { validateToken } from '../middlewares/JWTValidator.js';

const orderRouter = Router();

// Todas las rutas de órdenes requieren autenticación
orderRouter.use(validateToken);

// ═══════════════════════════════════════════════════════════
//  RUTAS - Endpoints de Órdenes de Venta con JSDoc Swagger
// ═══════════════════════════════════════════════════════════

/**
 * @swagger
 * tags:
 *   name: Órdenes
 *   description: Gestión de órdenes de venta CABS
 */

/**
 * @swagger
 * components:
 *   schemas:
 *     OrderItemResponse:
 *       type: object
 *       properties:
 *         sku:
 *           type: string
 *           example: "EXEL-001"
 *           description: SKU del producto en el catálogo Exel
 *         referencia:
 *           type: string
 *           example: "12345"
 *           description: ID del producto como referencia de catálogo
 *         nombre:
 *           type: string
 *           example: "Laptop HP EliteBook 840"
 *         precio:
 *           type: number
 *           example: 18999.99
 *         cantidad:
 *           type: integer
 *           example: 2
 *         totalProducto:
 *           type: number
 *           example: 37999.98
 *
 *     OrderResponse:
 *       type: object
 *       properties:
 *         id:
 *           type: integer
 *           example: 1
 *         estado:
 *           type: string
 *           enum: [PAGADO_PENDIENTE_SURTIDO, EN_PREPARACION, EN_CAMINO, ENTREGADO, CANCELADO]
 *           example: "PAGADO_PENDIENTE_SURTIDO"
 *         fechaPedido:
 *           type: string
 *           format: date-time
 *         metodoPago:
 *           type: string
 *           example: "Transferencia"
 *         metodoEntrega:
 *           type: string
 *           example: "Domicilio"
 *         direccionEntrega:
 *           type: object
 *           nullable: true
 *         subtotal:
 *           type: number
 *           example: 37999.98
 *         flete:
 *           type: number
 *           example: 0
 *         total:
 *           type: number
 *           example: 37999.98
 *         productos:
 *           type: array
 *           items:
 *             $ref: '#/components/schemas/OrderItemResponse'
 */

/**
 * @swagger
 * /api/orders:
 *   get:
 *     summary: Listar todos los pedidos del usuario autenticado
 *     tags: [Órdenes]
 *     security:
 *       - cokieAuth: []
 *     responses:
 *       200:
 *         description: Lista de órdenes obtenida exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/OrderResponse'
 *       401:
 *         description: Token inválido o no proporcionado
 *       500:
 *         description: Error interno del servidor
 */
orderRouter.get('/', orderController.getOrders.bind(orderController));

/**
 * @swagger
 * /api/orders/{id}:
 *   get:
 *     summary: Obtener un pedido específico por ID
 *     tags: [Órdenes]
 *     security:
 *       - cokieAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: integer
 *         description: ID del pedido
 *     responses:
 *       200:
 *         description: Pedido encontrado exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   $ref: '#/components/schemas/OrderResponse'
 *       403:
 *         description: El pedido no pertenece al usuario autenticado
 *       404:
 *         description: Pedido no encontrado
 *       500:
 *         description: Error interno del servidor
 */
orderRouter.get('/:id', orderController.getOrderById.bind(orderController));

/**
 * @swagger
 * /api/orders/{id}/status:
 *   patch:
 *     summary: Actualizar el estado de un pedido
 *     tags: [Órdenes]
 *     security:
 *       - cokieAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: integer
 *         description: ID del pedido a actualizar
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [status]
 *             properties:
 *               status:
 *                 type: string
 *                 enum: [PAGADO_PENDIENTE_SURTIDO, EN_PREPARACION, EN_CAMINO, ENTREGADO, CANCELADO]
 *                 example: "EN_PREPARACION"
 *     responses:
 *       200:
 *         description: Estado actualizado exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   $ref: '#/components/schemas/OrderResponse'
 *       400:
 *         description: Estado inválido
 *       403:
 *         description: El pedido no pertenece al usuario autenticado
 *       404:
 *         description: Pedido no encontrado
 *       500:
 *         description: Error interno del servidor
 */
orderRouter.patch('/:id/status', orderController.updateOrderStatus.bind(orderController));

export default orderRouter;
