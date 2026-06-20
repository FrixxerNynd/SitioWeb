import { Router } from "express";
import addressController from "../controllers/address.controller.js";

// Nota: NO se aplica validateToken aquí porque este router se monta
// como sub-router dentro de cartRouter, que ya corre validateToken
// en su middleware raíz (cartRouter.use(validateToken)).
const addressRouter = Router();

// ═══════════════════════════════════════════════════════════
//  RUTAS - Direcciones del usuario, anidadas en /api/cart/addresses
// ═══════════════════════════════════════════════════════════

/**
 * @swagger
 * tags:
 *   name: Direcciones
 *   description: Gestión de direcciones de entrega del carrito
 */

/**
 * @swagger
 * /api/cart/addresses:
 *   get:
 *     summary: Listar las direcciones del usuario autenticado
 *     tags: [Direcciones]
 *     security:
 *       - cokieAuth: []
 *     responses:
 *       200:
 *         description: Direcciones obtenidas exitosamente
 *       500:
 *         description: Error interno del servidor
 */
addressRouter.get("/", addressController.getAddresses.bind(addressController));

/**
 * @swagger
 * /api/cart/addresses/{addressId}:
 *   get:
 *     summary: Obtener una dirección específica del usuario
 *     tags: [Direcciones]
 *     security:
 *       - cokieAuth: []
 *     parameters:
 *       - in: path
 *         name: addressId
 *         required: true
 *         schema:
 *           type: integer
 *     responses:
 *       200:
 *         description: Dirección encontrada
 *       404:
 *         description: La dirección no existe
 *       500:
 *         description: Error interno del servidor
 */
addressRouter.get("/:addressId", addressController.getAddressById.bind(addressController));

/**
 * @swagger
 * /api/cart/addresses:
 *   post:
 *     summary: Agregar una nueva dirección de entrega
 *     tags: [Direcciones]
 *     security:
 *       - cokieAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [pais, estado, ciudad, codigoPostal, colonia, calle, nExterior]
 *             properties:
 *               pais:
 *                 type: string
 *                 example: México
 *               estado:
 *                 type: string
 *                 example: Durango
 *               ciudad:
 *                 type: string
 *                 example: Victoria de Durango
 *               codigoPostal:
 *                 type: string
 *                 example: "34167"
 *               colonia:
 *                 type: string
 *                 example: Barcelona
 *               calle:
 *                 type: string
 *                 example: eduarado de la pena lares
 *               nExterior:
 *                 type: string
 *                 example: "154"
 *               nInterior:
 *                 type: string
 *                 example: "652"
 *     responses:
 *       201:
 *         description: Dirección creada exitosamente
 *       400:
 *         description: Faltan campos obligatorios
 *       500:
 *         description: Error interno del servidor
 */
addressRouter.post("/", addressController.createAddress.bind(addressController));

/**
 * @swagger
 * /api/cart/addresses/{addressId}:
 *   put:
 *     summary: Modificar una dirección existente
 *     tags: [Direcciones]
 *     security:
 *       - cokieAuth: []
 *     parameters:
 *       - in: path
 *         name: addressId
 *         required: true
 *         schema:
 *           type: integer
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               pais:
 *                 type: string
 *                 example: México
 *               estado:
 *                 type: string
 *                 example: Durango
 *               ciudad:
 *                 type: string
 *                 example: Victoria de Durango
 *               codigoPostal:
 *                 type: string
 *                 example: "34167"
 *               colonia:
 *                 type: string
 *                 example: Barcelona
 *               calle:
 *                 type: string
 *                 example: eduarado de la pena lares
 *               nExterior:
 *                 type: string
 *                 example: "154"
 *               nInterior:
 *                 type: string
 *                 example: "652"
 *     responses:
 *       200:
 *         description: Dirección actualizada exitosamente
 *       404:
 *         description: La dirección no existe
 *       500:
 *         description: Error interno del servidor
 */
addressRouter.put("/:addressId", addressController.updateAddress.bind(addressController));

/**
 * @swagger
 * /api/cart/addresses/{addressId}:
 *   delete:
 *     summary: Eliminar una dirección
 *     tags: [Direcciones]
 *     security:
 *       - cokieAuth: []
 *     parameters:
 *       - in: path
 *         name: addressId
 *         required: true
 *         schema:
 *           type: integer
 *     responses:
 *       200:
 *         description: Dirección eliminada exitosamente
 *       404:
 *         description: La dirección no existe
 *       500:
 *         description: Error interno del servidor
 */
addressRouter.delete("/:addressId", addressController.deleteAddress.bind(addressController));

export default addressRouter;