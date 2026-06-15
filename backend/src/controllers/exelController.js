import exelService from '../services/exelService.js';
import redisClient from '../config/redis.js';

// ═══════════════════════════════════════════════════════════
//  CONTROLADOR - Catálogo de productos Exel del Norte
// ═══════════════════════════════════════════════════════════

class ExelController {

    /**
     * GET /api/productos
     * Devuelve productos desde Redis si están disponibles.
     * Acepta filtros opcionales: ?categoria=X&subcategoria=Y&marca=Z
     * y paginación: ?limite=50&offset=0
     * Si Redis no está disponible hace fallback a la API externa.
     */
    async getProductos(req, res, next) {
        try {
            const {
                categoria,
                subcategoria,
                marca,
                pageSize = 50,
                page
            } = req.query;

            // ── Intento desde Redis ───────────────────────────────
            const redisResult = await exelService.getProductsRedis({
                categoria,
                subcategoria,
                marca,
                pageSize: parseInt(pageSize),
                page: parseInt(page),
            });

            if (redisResult) {
                return res.status(200).json({
                    success: true,
                    source: 'redis',
                    total: redisResult.total,
                    data: redisResult.productos,
                    pageSize: parseInt(redisResult.pageSize),
                    Pagina: parseInt(redisResult.page),
                    Paginas: redisResult.totalPages,
                });
            }

            // ── Fallback a API externa ────────────────────────────
            const productos = await exelService.fetchExternalProducts(req.query);
            return res.status(200).json({
                success: true,
                source: 'api',
                total: productos.length,
                data: productos
            });

        } catch (error) {
            next(error);
        }
    }

    /**
     * GET /api/productos/:referencia
     * Devuelve un producto por referencia desde Redis.
     */
    async getProductByReference(req, res, next) {
        try {
            const { referencia } = req.params;
            const producto = await exelService.getProductByReference(referencia);
            if (!producto) {
                return res.status(404).json({ success: false, message: 'Producto no encontrado' });
            }
            res.status(200).json({ success: true, data: producto });
        } catch (error) {
            next(error);
        }
    }

    /**
     * GET /api/productos/categorias
     * Devuelve el listado de categorías desde la API externa.
     */
    async getCategorias(req, res, next) {
        try {
            const data = await exelService.getCategoriesRedis();
            res.status(200).json({ success: true, data });
        } catch (error) {
            next(error);
        }
    }

    /**
     * GET /api/productos/imagenes
     * Devuelve imágenes de productos.
     * Acepta query params opcionales: ?page=1&limit=50
     */
    async getImagenes(req, res, next) {
        try {
            const { page = 1, limit = 50, referencias, referencia } = req.query;
            // Acepta tanto ?referencias=A,B,C como ?referencia=A
            const refsRaw = referencias || referencia;
            const refs = refsRaw ? refsRaw.split(',') : null;

            console.log('refs parseadas:', refs);

            const data = await exelService.getImagenesBatch({ page, limit, refs });
            res.status(200).json({ success: true, data });
        } catch (error) {
            next(error);
        }
    }

    // #region Metodos de sincronizacion Redis

    /**
     * POST /api/productos/sync
     * Fuerza una sincronización del catálogo completo desde la API a Redis.
     * Útil para refrescar manualmente el caché.
     */
    async syncCatalogo(req, res, next) {
        try {
            const productos = await exelService.fetchExternalProducts({ "sin_stock": "false" });
            res.status(200).json({
                success: true,
                message: `Catálogo sincronizado: ${productos.length} productos guardados en Redis`,
                total: productos.length,
            });
        } catch (error) {
            next(error);
        }
    }
    /**
     * POST /api/productos/sync-categorias
     * Fuerza una sincronización de las categorías desde la API a Redis.
     * Útil para refrescar manualmente el caché.
     */
    async syncCategorias(req, res, next) {
        try {
            const categorias = await exelService.getExternalCategories();
            res.status(200).json({
                success: true,
                message: `Categorías sincronizadas: ${categorias.length} categorías guardadas en Redis`,
                total: categorias.length,
            });
        } catch (error) {
            next(error);
        }
    }
    /**
     * POST /api/productos/sync-imagenes
     * Fuerza una sincronización del catálogo de imágenes desde la API a Redis.
     * Útil para refrescar manualmente el caché de imágenes.
     */
    async syncImagenes(req, res, next) {
        try {
            const imagenes = await exelService.getExternalImagenes([]);
            res.status(200).json({
                success: true,
                message: `Imágenes sincronizadas: ${imagenes.length} productos con imágenes guardados en Redis`,
                total: imagenes.length,
            });
        } catch (error) {
            next(error);
        }
    }
    /**
     * GET /api/productos/redis-stats
     * Devuelve un resumen del estado del catálogo en Redis para diagnóstico.
     */
    async getRedisStats(req, res, next) {
        try {
            const client = redisClient.getClient();

            if (!client) {
                return res.status(503).json({
                    success: false,
                    message: 'Redis no está disponible',
                });
            }

            // Total de productos en el set maestro
            const totalProductos = await client.sCard('catalogo:referencias');

            // Muestra de las primeras 5 referencias
            const muestraReferencias = await client.sRandMember('catalogo:referencias', 5);

            // Vista previa del hash del primer producto de la muestra
            let muestraProducto = null;
            if (muestraReferencias.length > 0) {
                muestraProducto = await client.hGetAll(`producto:${muestraReferencias[0]}`);
            }

            // Contar las claves de cada tipo de índice
            const keysCategoria = await client.keys('indice:categoria:*');
            const keysSubcategoria = await client.keys('indice:subcategoria:*');
            const keysMarca = await client.keys('indice:marca:*');

            // Contar cuántas referencias tiene cada índice de categoría
            const statsCategoria = await Promise.all(
                keysCategoria.map(async key => ({
                    indice: key,
                    total: await client.sCard(key),
                }))
            );

            return res.status(200).json({
                success: true,
                redis: {
                    conectado: true,
                    totalProductos,
                    indices: {
                        categorias: keysCategoria.length,
                        subcategorias: keysSubcategoria.length,
                        marcas: keysMarca.length,
                    },
                    detalleCategorias: statsCategoria,
                    muestraReferencias,
                    muestraProducto,
                },
            });
        } catch (error) {
            next(error);
        }
    }

    // ── Nuevos handlers ─────────────────────────────────────

    /**
     * GET /api/productos/subcategorias
     * Devuelve el listado de subcategorías desde Redis.
     */
    async getSubcategorias(req, res, next) {
        try {
            const data = await exelService.getSubcategoriasRedis();
            res.status(200).json({ success: true, data });
        } catch (error) {
            next(error);
        }
    }

    /**
     * GET /api/productos/medidas
     * Devuelve medidas de productos paginadas desde Redis.
     * Acepta: ?page=1&limit=50
     */
    async getMedidas(req, res, next) {
        try {
            const { page = 1, limit = 50 } = req.query;
            const data = await exelService.getMedidasRedis(parseInt(page), parseInt(limit));
            res.status(200).json({ success: true, data });
        } catch (error) {
            next(error);
        }
    }

    /**
     * GET /api/productos/fichas-tecnicas
     * Devuelve fichas técnicas paginadas desde Redis.
     * Acepta: ?page=1&limit=50
     */
    async getFichasTecnicas(req, res, next) {
        try {
            const { page = 1, limit = 50 } = req.query;
            const data = await exelService.getFichasTecnicasRedis(parseInt(page), parseInt(limit));
            res.status(200).json({ success: true, data });
        } catch (error) {
            next(error);
        }
    }

    /**
     * GET /api/productos/ofertas
     * Devuelve productos en oferta paginados desde Redis.
     * Acepta: ?page=1&limit=50
     */
    async getProductosEnOferta(req, res, next) {
        try {
            const { page = 1, limit = 50 } = req.query;
            const data = await exelService.getProductosEnOferta(parseInt(page), parseInt(limit));
            res.status(200).json({ success: true, data });
        } catch (error) {
            next(error);
        }
    }

    /**
     * GET /api/productos/precios
     * Devuelve todos los precios y stock paginados desde Redis,
     * independientemente de si están en oferta o no.
     * Acepta: ?page=1&limit=50
     */
    async getPrecios(req, res, next) {
        try {
            const { page = 1, limit = 50 } = req.query;
            const data = await exelService.getPreciosStockRedis(parseInt(page), parseInt(limit));
            res.status(200).json({ success: true, data });
        } catch (error) {
            next(error);
        }
    }

    // #region Sync handlers nuevos

    /**
     * POST /api/productos/sync-medidas
     * Fuerza una descarga de las medidas de productos desde la API y las guarda en Redis.
     */
    async syncMedidas(req, res, next) {
        try {
            const result = await exelService.getSaveExternalSizeProducts(req.query);
            res.status(200).json({
                success: true,
                message: `Medidas sincronizadas: ${result?.saved ?? 0} guardadas, ${result?.skipped ?? 0} omitidas`,
                ...result,
            });
        } catch (error) {
            next(error);
        }
    }

    /**
     * POST /api/productos/sync-fichas
     * Fuerza una descarga de las fichas técnicas desde la API y las guarda en Redis.
     */
    async syncFichas(req, res, next) {
        try {
            const result = await exelService.getSaveFichaProducts();
            res.status(200).json({
                success: true,
                message: `Fichas técnicas sincronizadas: ${result.total} obtenidas, ${result.saved} guardadas, ${result.skipped} omitidas`,
                ...result,
            });
        } catch (error) {
            next(error);
        }
    }

    /**
     * POST /api/productos/sync-subcategorias
     * Fuerza una descarga de las subcategorías desde la API y las guarda en Redis.
     */
    async syncSubcategorias(req, res, next) {
        try {
            const subcategorias = await exelService.getExternalSubcategorias();
            res.status(200).json({
                success: true,
                message: `Subcategorías sincronizadas: ${subcategorias.length} guardadas en Redis`,
                total: subcategorias.length,
            });
        } catch (error) {
            next(error);
        }
    }

    //#endregion
}

export default new ExelController();
