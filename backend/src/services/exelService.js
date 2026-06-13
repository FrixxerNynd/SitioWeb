import axios from 'axios';
import logger from '../utils/Helpers/logger.js';
import redisClient from '../config/redis.js';
import RedisHelper from '../utils/Helpers/redisHelper.js';
import dotenv from "dotenv";
import { responseDto } from "../utils/DTO's/Response/DTO-Index.js"

dotenv.config();

const apiKey = process.env.API_TOKEN;
class ExelService {

  // Método original - se mantiene igual
  async fetchExternalProducts(queryParams) {
    try {
      const apiUrl = 'https://api01.exeldelnorte.com.mx/productos';
      const response = await axios.get(apiUrl, {
        headers: {
          'Authorization': apiKey
        },
        params: queryParams
      });

      const items =
        response.data?.datos ??
        (Array.isArray(response.data) ? response.data : []);

      const productos = items.map(item => new responseDto.ProductosResponseDto({
        id: item.id,
        nombre: item.nombre,
        precio: parseFloat(item.precio ?? 0),
        precioOriginal: parseFloat(item.precio_sin_oferta ?? 0),
        stock: parseInt(item.stock ?? 0),
        descripcion: item.descripcion_extendida,
        sku: item.sku,
        codigoSAT: item.codigo_sat,
        codigoBarras: item.codigo_barras,
        marca: item.marca_id,
        subcategoria: item.subcategoria_id,
        referencia: item.referencia,
        categoria: item.categoria_id,
      }));

      await this.saveProductsRedis(productos);

      return productos;
    } catch (error) {
      logger.error(
        `Error al conectar con la API de Exel del Norte: ${error.message}`,
      );
      throw new Error("No se pudo obtener el catálogo de productos externo.");
    }
  }



  /**
   * Consulta productos desde Redis aplicando filtros por índice.
   *
   * Estrategia de filtrado:
   *  - Sin filtros        → devuelve todas las referencias de `catalogo:referencias`
   *  - Con un filtro      → usa el índice directo (p.ej. `indice:categoria:5`)
   *  - Múltiples filtros  → intersección (SINTER) de los índices involucrados,
   *                         lo que devuelve solo las referencias que cumplen TODOS los filtros.
   *
   * @param {Object} filtros
   * @param {string|number} [filtros.categoria]    - ID de categoría
   * @param {string|number} [filtros.subcategoria] - ID de subcategoría
   * @param {string|number} [filtros.marca]        - ID de marca
   * @param {number}        [filtros.limite=50]    - Máximo de resultados
   * @param {number}        [filtros.offset=0]     - Índice de inicio (paginación)
   * @returns {Promise<{productos: Object[], total: number}|null>}
   *   null si Redis no está disponible (el llamador debe hacer fallback a la API externa).
   */
  async getProductsRedis({ categoria, subcategoria, marca, limite = 50, offset = 0 } = {}) {
    const client = redisClient.getClient();

    if (!client) {
      logger.warn('Redis no disponible, se omite caché para la consulta');
      return null;
    }

    try {
      // ── 1. Determinar el conjunto de referencias a devolver ──────────────
      let referencias = [];

      const indicesActivos = [
        categoria ? `indice:categoria:${categoria}` : null,
        subcategoria ? `indice:subcategoria:${subcategoria}` : null,
        marca ? `indice:marca:${marca}` : null,
      ].filter(Boolean);

      if (indicesActivos.length === 0) {
        // Sin filtros → catálogo completo
        referencias = await client.sMembers('catalogo:referencias');
        logger.info(`Cache HIT catálogo completo: ${referencias.length} referencias`);

      } else if (indicesActivos.length === 1) {
        // Un solo filtro → lectura directa del índice
        referencias = await client.sMembers(indicesActivos[0]);
        logger.info(`Cache HIT índice [${indicesActivos[0]}]: ${referencias.length} referencias`);

      } else {
        // Múltiples filtros → intersección de índices (AND lógico)
        referencias = await client.sInter(indicesActivos);
        logger.info(
          `Cache HIT SINTER [${indicesActivos.join(' ∩ ')}]: ${referencias.length} referencias`
        );
      }

      // ── 2. Validar que existan resultados ────────────────────────────────
      if (referencias.length === 0) {
        return { productos: [], total: 0 };
      }

      const total = referencias.length;

      // ── 3. Aplicar paginación sobre las referencias ──────────────────────
      const pagina = referencias.slice(offset, offset + limite);

      // ── 4. Obtener los hashes de cada referencia en paralelo ─────────────
      const hgets = pagina.map(ref => client.hGetAll(`producto:${ref}`));
      const resultados = await Promise.all(hgets);

      // Filtrar hashes vacíos (referencias huérfanas sin hash correspondiente)
      const productos = resultados.filter(p => p && Object.keys(p).length > 0);

      logger.info(`Devolviendo ${productos.length}/${total} productos desde Redis`);
      return { productos, total };

    } catch (error) {
      logger.error(`Error al consultar productos en Redis: ${error.message}`);
      return null;
    }
  }

  /**
   * Obtiene las categorias desde la API externa y las guarda en Redis.
   */
  async getExternalCategories() {
    try {
      const apiUrl = "https://api01.exeldelnorte.com.mx/categorias";
      const response = await axios.get(apiUrl, {
        headers: {
          Authorization: apiKey,
        },
      });

      //Mapear la respuesta a CategoriaResponseDto
      const categorias = response.data.datos.map(categoria => new responseDto.categoria({
        id: categoria.id_categoria,
        nombre_categoria: categoria.nombre_categoria,
      }));

      await this.saveCategoriesRedis(categorias);
      return categorias

    } catch (error) {
      logger.error(
        `Error al obtener las categorias de Exel del Norte: ${error.message}`
      );
      throw new Error("No se pudo obtener el catálogo de categorias externo");
    }
  }

  async getCategoriesRedis() {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);

    const categorias = await helper.getAll({
      keyPrefix: 'categoria',
      idsSetKey: 'catalogo:categorias'
    });
    return categorias
  }

  async getExternalImagenes(query) {
    try {
      const apiUrl = "https://api01.exeldelnorte.com.mx/imagenes";
      //Verificar si query trae informacion
      if (query.length > 0) {
        const response = await axios.get(apiUrl, {
          headers: {
            Authorization: apiKey,
          },
          params: query,
        });
        return response.data;
      }

      const response = await axios.get(apiUrl, {
        headers: {
          Authorization: apiKey,
        },
      });

      //Mapear la respuesta a ImagenResponseDto
      const imagenes = response.data.datos.map(imagen => new responseDto.imagenes({
        referencia: imagen.referencia,
        imagenes: imagen.imagenes,
      }));

      await this.saveImagesRedis(imagenes);
      return imagenes;

    } catch (error) {
      logger.error(
        `Error al obtener las imagenes de Exel del Norte: ${error.message}`,
      );
      throw new Error("No se pudo obtener el catálogo de imagenes externo");
    }
  }

  async getImagenesBatch({ idsSetKey = 'catalogo:imagenes', page = 1, limit = 50 } = {}) {
    const client = await redisClient.getClient();

    if (!client) {
      logger.warn('Redis no disponible, no se pudieron leer imágenes');
      return { total: 0, page, limit, totalPages: 0, datos: [] };
    }

    const allIds = await client.sMembers(idsSetKey);
    const total = allIds.length;

    const start = (page - 1) * limit;
    const idsPagina = allIds.slice(start, start + limit);

    const datos = await Promise.all(
      idsPagina.map(async (referencia) => {
        const raw = await client.get(`imagenes:${referencia}`);
        return new responseDto.imagenes({
          referencia,
          imagenes: raw ? JSON.parse(raw) : [],
        });
      })
    );

    return {
      total,
      page,
      limit,
      totalPages: Math.ceil(total / limit),
      datos
    };
  }

  //#region Metodos Guardado en Redis
  async saveProductsRedis(productos) {
    const client = await redisClient.getClient();

    if (!client) {
      logger.warn("Redis no disponible, productos no guardados en redis")
      return;
    }

    try {
      const multi = client.multi();

      for (const p of productos) {
        if (!p.referencia) continue;

        multi.hSet(`producto:${p.referencia}`, {
          referencia: parseInt(p.referencia ?? 0),
          sku: String(p.sku ?? 0),
          nombre: p.nombre ?? '',
          marca: parseInt(p.marca ?? 0),
          categoria: parseInt(p.categoria ?? 0),
          subcategoria: parseInt(p.subcategoria ?? 0),
          codigoBarras: p.codigoBarras ?? '',
          codigoSAT: p.codigoSAT ?? '',
        });

        multi.sAdd('catalogo:referencias', p.referencia);
        if (p.categoria) {
          multi.sAdd(`indice:categoria:${p.categoria}`, p.referencia);
        }
        if (p.subcategoria) {
          multi.sAdd(`indice:subcategoria:${p.subcategoria}`, p.referencia);
        }
        if (p.marca) {
          multi.sAdd(`indice:marca:${p.marca}`, p.referencia);
        }
      }

      await multi.exec();
      logger.info(`Se guardaron ${productos.length} productos en Redis`);
    } catch (error) {
      logger.error("Error al guardar productos en redis: " + error.message);
    }
  }

  async saveCategoriesRedis(categorias) {
    const client = await redisClient.getClient();
    const helper = new RedisHelper(client);
    try {
      const { saved, skipped } = await helper.saveBatch(categorias, {
        keyPrefix: 'categoria',
        idField: 'id',
        allKeysSet: 'catalogo:categorias',
        toHash: (c) => ({
          id: String(c.id_categoria ?? ''),
          nombre_categoria: String(c.nombre_categoria ?? ''),
        }),
        indices: [],
      });

      logger.info(`Categorías: ${saved} guardadas, ${skipped} saltadas`);
    }
    catch (error) {
      logger.error("Error al guardar categorias en redis: " + error.message);
      throw new Error("Error al guardar categorias en redis: " + error.message)
    }
  }

  async saveImagesRedis(imagenes) {
    const client = await redisClient.getClient();

    if (!client) {
      logger.warn("Redis no disponible, imagenes no guardadas en redis");
      return { saved: 0, skipped: imagenes.length };
    }

    try {
      const multi = client.multi();
      let saved = 0;
      let skipped = 0;

      for (const img of imagenes) {
        if (!img.referencia || !img.imagenes || img.imagenes.length === 0) {
          skipped++;
          continue;
        }

        multi.set(`imagenes:${img.referencia}`, JSON.stringify(img.imagenes));
        multi.sAdd('catalogo:imagenes', img.referencia);

        saved++;
      }

      await multi.exec();
      logger.info(`Se guardaron ${saved} productos con imágenes en Redis (${skipped} saltados)`);
      return { saved, skipped };
    } catch (error) {
      logger.error("Error al guardar imagenes en redis: " + error.message);
      return { saved: 0, skipped: imagenes.length };
    }
  }


  //#endregion
}


export default new ExelService();
