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
//#region Gets con info desde redis

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
 *         name: stock
 *         schema:
 *           type: string
 *         description: true si tiene stock, false si no tiene
 *       - in: query
 *         name: pageSize
 *         schema:
 *           type: integer
 *           default: 50
 *         description: Número de resultados por página
 *       - in: query
 *         name: page
 *         schema:
 *           type: integer
 *           default: 1
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
 * /api/productos/marcas:
 *   get:
 *     summary: Listar marcas del catálogo
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Marcas obtenidas exitosamente
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/marcas', exelController.getMarcas.bind(exelController));

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

/**
 * @swagger
 * /api/productos/subcategorias:
 *   get:
 *     summary: Listar subcategorías del catálogo
 *     description: Devuelve todas las subcategorías desde Redis. Cada subcategoría incluye su ID, nombre e ID de categoría padre.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Subcategorías obtenidas exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 data:
 *                   type: array
 *                   items:
 *                     type: object
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/subcategorias', exelController.getSubcategorias.bind(exelController));

/**
 * @swagger
 * /api/productos/medidas:
 *   get:
 *     summary: Listar medidas de productos
 *     description: Devuelve medidas de productos paginadas desde Redis (alto, ancho, largo, peso, volumen).
 *     tags: [Productos]
 *     parameters:
 *       - in: query
 *         name: page
 *         schema:
 *           type: integer
 *           default: 1
 *         description: Número de página
 *       - in: query
 *         name: limit
 *         schema:
 *           type: integer
 *           default: 50
 *         description: Resultados por página
 *     responses:
 *       200:
 *         description: Medidas obtenidas exitosamente
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/medidas', exelController.getMedidas.bind(exelController));

/**
 * @swagger
 * /api/productos/fichas-tecnicas:
 *   get:
 *     summary: Listar fichas técnicas de productos
 *     description: Devuelve fichas técnicas paginadas desde Redis.
 *     tags: [Productos]
 *     parameters:
 *       - in: query
 *         name: page
 *         schema:
 *           type: integer
 *           default: 1
 *         description: Número de página
 *       - in: query
 *         name: limit
 *         schema:
 *           type: integer
 *           default: 50
 *         description: Resultados por página
 *     responses:
 *       200:
 *         description: Fichas técnicas obtenidas exitosamente
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/fichas-tecnicas', exelController.getFichasTecnicas.bind(exelController));

/**
 * @swagger
 * /api/productos/ofertas:
 *   get:
 *     summary: Listar productos en oferta
 *     description: Devuelve productos con precio de oferta activo, paginados desde Redis.
 *     tags: [Productos]
 *     parameters:
 *       - in: query
 *         name: page
 *         schema:
 *           type: integer
 *           default: 1
 *         description: Número de página
 *       - in: query
 *         name: limit
 *         schema:
 *           type: integer
 *           default: 50
 *         description: Resultados por página
 *     responses:
 *       200:
 *         description: Productos en oferta obtenidos exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 data:
 *                   type: object
 *                   properties:
 *                     total:
 *                       type: integer
 *                     page:
 *                       type: integer
 *                     limit:
 *                       type: integer
 *                     totalPages:
 *                       type: integer
 *                     datos:
 *                       type: array
 *                       items:
 *                         type: object
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/ofertas', exelController.getProductosEnOferta.bind(exelController));

/**
 * @swagger
 * /api/productos/precios:
 *   get:
 *     summary: Listar todos los precios y stock
 *     description: |
 *       Devuelve el precio, precio de oferta, precio sin oferta, existencia (stock)
 *       y estado de oferta de todos los productos, paginados desde Redis.
 *       No filtra por oferta — devuelve el catálogo completo de precios.
 *     tags: [Productos]
 *     parameters:
 *       - in: query
 *         name: page
 *         schema:
 *           type: integer
 *           default: 1
 *         description: Número de página
 *       - in: query
 *         name: limit
 *         schema:
 *           type: integer
 *           default: 50
 *         description: Resultados por página
 *     responses:
 *       200:
 *         description: Precios y stock obtenidos exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   type: object
 *                   properties:
 *                     total:
 *                       type: integer
 *                     page:
 *                       type: integer
 *                     limit:
 *                       type: integer
 *                     totalPages:
 *                       type: integer
 *                     datos:
 *                       type: array
 *                       items:
 *                         type: object
 *                         properties:
 *                           referencia:
 *                             type: string
 *                           precio:
 *                             type: number
 *                           precioOferta:
 *                             type: number
 *                           precioSinOferta:
 *                             type: number
 *                           oferta:
 *                             type: boolean
 *                           existencia:
 *                             type: integer
 *                           fechaActualizacion:
 *                             type: string
 *                             format: date-time
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.get('/precios', exelController.getPrecios.bind(exelController));

/**
 * @swagger
 * /api/productos/{referencia}:
 *   get:
 *     summary: Obtener un producto por referencia
 *     tags: [Productos]
 *     parameters:
 *       - in: path
 *         name: referencia
 *         schema:
 *           type: string
 *         description: Referencia del producto a obtener
 *     responses:
 *       200:
 *         description: Producto obtenido exitosamente
 *       404:
 *         description: Producto no encontrado
 *       500:
 *         description: Error interno del servidor
 * */
exelRouter.get('/:referencia', exelController.getProductByReference.bind(exelController));

/**
 * @swagger
 * /api/productos/medida/{referencia}:
 *   get:
 *     summary: Obtener medida por referencia
 *     tags: [Productos]
 *     parameters:
 *       - in: path
 *         name: referencia
 *         schema:
 *           type: string
 *         description: Referencia del producto
 *     responses:
 *       200:
 *         description: Medida obtenida exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   type: object
 *                   properties:
 *                     referencia:
 *                       type: string
 *                     medida:
 *                       type: object
 *       404:
 *         description: Producto no encontrado
 *       500:
 *         description: Error interno del servidor
 * */
exelRouter.get('/medida/:referencia', exelController.getMedidaRedisRef.bind(exelController));

/**
 * @swagger
 * /api/productos/ficha-tecnica/{referencia}:
 *   get:
 *     summary: Obtener ficha técnica por referencia
 *     tags: [Productos]
 *     parameters:
 *       - in: path
 *         name: referencia
 *         schema:
 *           type: string
 *         description: Referencia del producto
 *     responses:
 *       200:
 *         description: Ficha técnica obtenida exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   type: object
 *                   properties:
 *                     referencia:
 *                       type: string
 *                     fichaTecnica:
 *                       type: object
 *       404:
 *         description: Producto no encontrado
 *       500:
 *         description: Error interno del servidor
 * */
exelRouter.get('/ficha-tecnica/:referencia', exelController.getFichaTecnicaRedisRef.bind(exelController));
//#endregion

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
 * /api/productos/sync-marcas:
 *   post:
 *     summary: Sincronizar marcas del catálogo en Redis
 *     description: Fuerza una descarga de todas las marcas desde la API externa y los guarda en Redis.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Marcas sincronizadas exitosamente
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
exelRouter.post('/sync-marcas', exelController.syncMarcas.bind(exelController));

/**
 * @swagger
 * /api/productos/sync-imagenes:
 *   post:
 *     summary: Sincronizar imágenes del catálogo en Redis
 *     description: Fuerza una descarga de todas las imágenes de productos desde la API externa y las guarda en Redis.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Imágenes sincronizadas exitosamente
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
exelRouter.post('/sync-imagenes', exelController.syncImagenes.bind(exelController));

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


/**
 * @swagger
 * /api/productos/sync-medidas:
 *   post:
 *     summary: Sincronizar medidas de productos en Redis
 *     description: Fuerza una descarga de todas las medidas de productos desde la API externa y las guarda en Redis.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Medidas sincronizadas exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 message:
 *                   type: string
 *                 saved:
 *                   type: integer
 *                 skipped:
 *                   type: integer
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.post('/sync-medidas', exelController.syncMedidas.bind(exelController));

/**
 * @swagger
 * /api/productos/sync-fichas:
 *   post:
 *     summary: Sincronizar fichas técnicas en Redis
 *     description: Fuerza una descarga de todas las fichas técnicas de productos desde la API externa y las guarda en Redis.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Fichas técnicas sincronizadas exitosamente
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
 *                 saved:
 *                   type: integer
 *                 skipped:
 *                   type: integer
 *       500:
 *         description: Error interno del servidor
 */
exelRouter.post('/sync-fichas', exelController.syncFichas.bind(exelController));

/**
 * @swagger
 * /api/productos/sync-subcategorias:
 *   post:
 *     summary: Sincronizar subcategorías en Redis
 *     description: Fuerza una descarga de todas las subcategorías desde la API externa y las guarda en Redis.
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Subcategorías sincronizadas exitosamente
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
exelRouter.post('/sync-subcategorias', exelController.syncSubcategorias.bind(exelController));

//#endregion

export default exelRouter;
