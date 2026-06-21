import { Router } from 'express';
import percentageController from '../controllers/percentageController.js';

const percentageRouter = Router();

/**
 * @swagger
 * tags:
 *   name: Porcentajes
 *   description: Configuracion de incrementos de precio por categoria
 */

/**
 * @swagger
 * /api/porcentajes:
 *   get:
 *     summary: Listar todos los porcentajes configurados
 *     description: Devuelve el listado completo de porcentajes de incremento de precio ordenados por id_categoria.
 *     tags: [Porcentajes]
 *     responses:
 *       200:
 *         description: Lista de porcentajes
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
 *                     $ref: '#/components/schemas/Percentage'
 *       500:
 *         description: Error interno del servidor
 */
percentageRouter.get('/', percentageController.getPorcentajes.bind(percentageController));

/**
 * @swagger
 * /api/porcentajes/categoria/{id_categoria}:
 *   get:
 *     summary: Obtener porcentaje por id_categoria
 *     description: Devuelve el porcentaje configurado para una categoria especifica.
 *     tags: [Porcentajes]
 *     parameters:
 *       - in: path
 *         name: id_categoria
 *         required: true
 *         schema:
 *           type: string
 *         description: ID de la categoria (ej. "15")
 *     responses:
 *       200:
 *         description: Porcentaje encontrado
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   $ref: '#/components/schemas/Percentage'
 *       404:
 *         description: No hay porcentaje configurado para esa categoria
 */
percentageRouter.get('/categoria/:id_categoria', percentageController.getPorcentajePorCategoria.bind(percentageController));

/**
 * @swagger
 * /api/porcentajes:
 *   post:
 *     summary: Crear o actualizar un porcentaje
 *     description: |
 *       Crea un nuevo porcentaje o actualiza uno existente (upsert basado en id_categoria).
 *       El porcentaje se aplica automaticamente en la siguiente sincronizacion de precios.
 *     tags: [Porcentajes]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - id_categoria
 *               - porcentaje
 *             properties:
 *               id_categoria:
 *                 type: string
 *                 description: ID de la categoria
 *                 example: "15"
 *               nombre_categoria:
 *                 type: string
 *                 description: Nombre descriptivo de la categoria
 *                 example: "Cemento y derivados"
 *               porcentaje:
 *                 type: number
 *                 description: Porcentaje de incremento (ej. 15.5 = 15.5%)
 *                 example: 15.5
 *     responses:
 *       200:
 *         description: Porcentaje creado o actualizado
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   $ref: '#/components/schemas/Percentage'
 *       400:
 *         description: Datos invalidos (id_categoria o porcentaje faltante)
 */
percentageRouter.post('/', percentageController.crearOActualizarPorcentaje.bind(percentageController));

/**
 * @swagger
 * /api/porcentajes/{id}:
 *   delete:
 *     summary: Eliminar un porcentaje
 *     description: Elimina la configuracion de porcentaje por su ID (PK autoincremental).
 *     tags: [Porcentajes]
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: integer
 *         description: ID autoincremental del registro de porcentaje
 *     responses:
 *       200:
 *         description: Porcentaje eliminado
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 message:
 *                   type: string
 *                   example: "Porcentaje eliminado"
 *       404:
 *         description: Porcentaje no encontrado
 */
percentageRouter.delete('/:id', percentageController.eliminarPorcentaje.bind(percentageController));

export default percentageRouter;
