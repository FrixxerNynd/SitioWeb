import { Router } from 'express';
import exelController from '../controllers/exelController.js';

const exelRouter = Router();

// ═══════════════════════════════════════════════════════════
//  RUTAS - Catálogo de productos Exel del Norte
// ═══════════════════════════════════════════════════════════

/**
 * @swagger
 * tags:
 *   name: Productos
 *   description: Catálogo de productos Exel del Norte
 */

/**
 * @swagger
 * /api/productos:
 *   get:
 *     summary: Listar productos del catálogo
 *     description: |
 *       Devuelve productos desde Redis cuando están disponibles.
 *       Si Redis no está activo hace fallback a la API externa de Exel del Norte.
 *       Soporta filtrado por categoría, subcategoría y marca, y paginación.
 *     tags: [Productos]
 *     parameters:
 *       - in: query
 *         name: categoria
 *         schema:
 *           type: string
 *         description: ID de la categoría para filtrar
 *       - in: query
 *         name: subcategoria
 *         schema:
 *           type: string
 *         description: ID de la subcategoría para filtrar
 *       - in: query
 *         name: marca
 *         schema:
 *           type: string
 *         description: ID de la marca para filtrar
 *       - in: query
 *         name: limite
 *         schema:
 *           type: integer
 *           default: 50
 *         description: Número máximo de resultados (paginación)
 *       - in: query
 *         name: offset
 *         schema:
 *           type: integer
 *           default: 0
 *         description: Índice de inicio (paginación)
 *     responses:
 *       200:
 *         description: Productos obtenidos exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 source:
 *                   type: string
 *                   enum: [redis, api]
 *                   description: Indica si los datos vienen de Redis o de la API externa
 *                 total:
 *                   type: integer
 *                   description: Total de productos que coinciden con el filtro
 *                 data:
 *                   type: array
 *                   items:
 *                     type: object
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/', exelController.getProductos.bind(exelController));

/**
 * @swagger
 * /api/productos/categorias:
 *   get:
 *     summary: Listar categorías del catálogo
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Categorías obtenidas exitosamente
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/categorias', exelController.getCategorias.bind(exelController));

/**
 * @swagger
 * /api/productos/imagenes:
 *   get:
 *     summary: Obtener imágenes de productos
 *     tags: [Productos]
 *     parameters:
 *       - in: query
 *         name: referencia
 *         schema:
 *           type: string
 *         description: Referencia del producto para filtrar imágenes
 *     responses:
 *       200:
 *         description: Imágenes obtenidas exitosamente
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/imagenes', exelController.getImagenes.bind(exelController));

//#region Sincronizacion de datos con Redis
/**
 * @swagger
 * /api/productos/sync:
 *   post:
 *     summary: Sincronizar catálogo completo en Redis
 *     description: Fuerza una descarga de todos los productos desde la API externa y los guarda en Redis.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Catálogo sincronizado exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 message:
 *                   type: string
 *                 total:
 *                   type: integer
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.post('/sync', exelController.syncCatalogo.bind(exelController));

/**
 * @swagger
 * /api/productos/sync-categorias:
 *   post:
 *     summary: Sincronizar categorías del catálogo en Redis
 *     description: Fuerza una descarga de todas las categorías desde la API externa y los guarda en Redis.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Categorías sincronizadas exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 message:
 *                   type: string
 *                 total:
 *                   type: integer
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.post('/sync-categorias', exelController.syncCategorias.bind(exelController));
/**
 * @swagger
 * /api/productos/redis-stats:
 *   get:
 *     summary: Diagnóstico del catálogo en Redis
 *     description: |
 *       Devuelve un resumen del estado actual del caché Redis:
 *       total de productos, conteo de índices por categoría/subcategoría/marca
 *       y una muestra aleatoria de 5 referencias con el hash de la primera.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Estadísticas obtenidas exitosamente
 *       503:
 *         description: Redis no está disponible
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/redis-stats', exelController.getRedisStats.bind(exelController));


//#endregion
export default exelRouter;
