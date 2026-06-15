import axios from 'axios';
import logger from '../utils/Helpers/logger.js';
import redisClient from '../config/redis.js';
import RedisHelper from '../utils/Helpers/redisHelper.js';
import dotenv from "dotenv";
import { responseDto } from "../utils/DTO's/Response/DTO-Index.js"
import { stringify } from 'node:querystring';
import redis from '../config/redis.js';

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

      const productos = items.map(item => new responseDto.productos({
        nombre: item.nombre ?? '',
        descripcion: item.descripcion_extendida ?? '',
        sku: item.sku ?? '',
        codigoSAT: item.codigo_sat ?? '',
        codigoBarras: item.codigo_barras ?? '',
        marca: item.marca_id ?? '',
        subcategoria: item.subcategoria_id ?? '',
        referencia: item.referencia ?? '',
        categoria: item.categoria_id ?? '',
        stock: item.stock ?? 0,
      }));
      const precio_existencia = items.map(item => new responseDto.precio_stock({
        referencia: item.referencia,
        existencia: item.stock,
        precio: parseFloat(item.precio ?? 0),
        precioOferta: parseFloat(item.precio_oferta ?? 0),
        precioSinOferta: parseFloat(item.precio_sin_oferta ?? 0),
        oferta: item.oferta ?? false
      }))


      await this.saveProductsRedis(productos);
      await this.savePreciosExistenciaProductosRedis(precio_existencia);

      return productos;
    } catch (error) {
      logger.error(
        `Error al conectar con la API de Exel del Norte: ${error.message}`,
      );
      throw new Error("No se pudo obtener el catálogo de productos externo.");
    }
  }

  async getSaveExternalSizeProducts(queryParams) {
    try {
      const apiUrl = "https://api01.exeldelnorte.com.mx/productos_medidas";

      const response = await axios.get(apiUrl, {
        headers: {
          Authorization: apiKey,
        },
        params: queryParams,
      });

      const items = response.data?.datos ?? (Array.isArray(response.data) ? response.data : []);

      const medidas = items.map(item => new responseDto.medidas({
        referencia: item.referencia,
        altura: parseFloat(item.altura ?? 0),
        ancho: parseFloat(item.ancho ?? 0),
        largo: parseFloat(item.largo ?? 0),
        peso: parseFloat(item.peso ?? 0),
        medida_peso: item.medida_peso,
        volumen: parseFloat(item.volumen ?? 0),
        medida_volumen: item.medida_volumen,
      }));
      const { saved, skipped } = await this.saveMedidasRedis(medidas);
      logger.info(`Medidas guardadas: ${saved}, Medidas omitidas: ${skipped}`);

      return { total: items.length, saved, skipped }
    } catch (error) {
      logger.error(`Error al obtener y guardar medidas: ${error.message}`);
      throw new Error('No se pudo obtener las medidas de los productos.');
    }
  }

  async getSaveFichaProducts() {
    try {
      const apiUrl = "https://api01.exeldelnorte.com.mx/productos_fichatecnica";
      const response = await axios.get(apiUrl, {
        headers: { 'Authorization': apiKey }
      });

      const items = response.data?.datos ?? (Array.isArray(response.data) ? response.data : []);

      const fichasTecnicas = items.map(item => new responseDto.fichaTecnica({
        referencia: item.referencia,
        fichaTecnica: item.ficha_tecnica,
      }));

      const { saved, skipped } = await this.saveFichasTecnicasRedis(fichasTecnicas);

      return { total: fichasTecnicas.length, saved, skipped };
    } catch (error) {
      logger.error("Error al obtener y guardar ficha técnica de productos: " + error.message);
      throw new Error('No se pudo obtener las fichas técnicas de los productos.');
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
   *   null si Redis no está disponible (el llamador debe hacer fallback a la API externa).
   */
  async getProductsRedis({ categoria, subcategoria, marca, stock, pageSize = 100, page = 1 } = {}) {
    const client = redisClient.getClient();

    if (!client) {
      logger.warn('Redis no disponible, se omite caché para la consulta');
      return null;
    }

    try {
      // ── 1. Determinar el conjunto de referencias ──────────────────────────
      let referencias = [];

      const indicesActivos = [
        categoria ? `indice:categoria:${categoria}` : null,
        subcategoria ? `indice:subcategoria:${subcategoria}` : null,
        marca ? `indice:marca:${marca}` : null,
        stock !== undefined ? `indice:stock:${stock}` : null,
      ].filter(Boolean);

      if (indicesActivos.length === 0) {
        referencias = await client.sMembers('catalogo:referencias');
        logger.info(`Cache HIT catálogo completo: ${referencias.length} referencias`);
      } else if (indicesActivos.length === 1) {
        referencias = await client.sMembers(indicesActivos[0]);
        logger.info(`Cache HIT índice [${indicesActivos[0]}]: ${referencias.length} referencias`);
      } else {
        referencias = await client.sInter(indicesActivos);
        logger.info(`Cache HIT SINTER [${indicesActivos.join(' ∩ ')}]: ${referencias.length} referencias`);
      }

      // ── 2. Validar resultados ─────────────────────────────────────────────
      if (referencias.length === 0) {
        return { productos: [], total: 0, page, pageSize, totalPages: 0 };
      }

      const total = referencias.length;
      const totalPages = Math.ceil(total / pageSize);

      // Clamp: si piden una página fuera de rango, devolver vacío
      if (page < 1 || page > totalPages) {
        return { productos: [], total, page, pageSize, totalPages };
      }

      // ── 3. Paginación ─────────────────────────────────────────────────────
      const offset = (page - 1) * pageSize;
      const pagina = referencias.slice(offset, offset + pageSize);

      // ── 4. Obtener hashes en paralelo ─────────────────────────────────────
      const resultados = await Promise.all(
        pagina.map(ref => client.hGetAll(`producto:${ref}`))
      );

      const productosBrutos = resultados.filter(p => p && Object.keys(p).length > 0);

      // ── 5. Enriquecer con precio/stock ────────────────────────────────────
      const productos = await Promise.all(
        productosBrutos.map(async (prod) => {
          const ref = prod.referencia;
          if (!ref) return prod;
          try {
            const precioHash = await client.hGetAll(`precio_existencia:${ref}`);
            if (precioHash && Object.keys(precioHash).length > 0) {
              return {
                ...prod,
                precio: parseFloat(precioHash.precio ?? 0),
                precio_oferta: precioHash.precio_oferta ? parseFloat(precioHash.precio_oferta) : null,
                precio_sin_oferta: parseFloat(precioHash.precio_sin_oferta ?? precioHash.precio ?? 0),
                oferta: precioHash.oferta === 'true',
                stock: parseInt(precioHash.existencia ?? 0),
              };
            }
          } catch (_) { /* devolver producto sin precio si falla */ }
          return prod;
        })
      );

      logger.info(`Devolviendo página ${page}/${totalPages} — ${productos.length}/${total} productos desde Redis`);

      return { productos, total, page, pageSize, totalPages };

    } catch (error) {
      logger.error(`Error al consultar productos en Redis: ${error.message}`);
      return null;
    }
  }

  async getProductByReference(reference) {
    const client = redisClient.getClient();
    if (!client) {
      logger.warn('Redis no disponible, se omite caché para la consulta');
      return null;
    }
    try {
      const producto = await client.hGetAll(`producto:${reference}`);
      if (!producto || Object.keys(producto).length === 0) return null;
      
      try {
        const precioHash = await client.hGetAll(`precio_existencia:${reference}`);
        if (precioHash && Object.keys(precioHash).length > 0) {
          producto.precio = parseFloat(precioHash.precio ?? 0);
          producto.precio_oferta = precioHash.precio_oferta ? parseFloat(precioHash.precio_oferta) : null;
          producto.precio_sin_oferta = parseFloat(precioHash.precio_sin_oferta ?? precioHash.precio ?? 0);
          producto.oferta = precioHash.oferta === 'true';
          producto.stock = parseInt(precioHash.existencia ?? 0);
        }
      } catch (_) { /* si falla obtener precio, devolver producto base */ }
      
      return producto;
    } catch (error) {
      logger.error(`Error al consultar producto por referencia en Redis: ${error.message}`);
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
        id: parseInt(categoria.id_categoria),
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

  /**
   * Obtiene las marcas desde la API externa y las guarda en Redis.
   */
  async getSaveExternalBrand() {
    try {
      const apiUrl = 'https://api01.exeldelnorte.com.mx/marcas';
      const response = await axios.get(apiUrl, {
        headers: {
          Authorization: apiKey
        }
      });

      const marcas = response.data.datos.map(marca => new responseDto.marca({
        id_marca: marca.id,
        nombre_marca: marca.nombre,
      }))
      //Guardar las marcas en Redis
      const saved = await this.saveBrandsRedis(marcas);

      return saved;
    } catch(error) {
      logger.error(`Error al obtener las marcas de Exel del Norte: ${error.message}`);
      throw new Error("No se pudo obtener el catálogo de marcas externo");
    }
  }

   /**
   * Obtiene las marcas desde Redis.
   */
  async getBrandsRedis() {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);

    const marcas = await helper.getAll({
      keyPrefix: 'marca',
      idsSetKey: 'catalogo:marcas'
    });
    return marcas
  }


  /**
   * Obtiene las categorias desde Redis.
   */
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

      // La API devuelve la clave raíz en mayúsculas "DATA"
      const items = response.data?.DATA ?? response.data?.datos ?? [];

      //Mapear la respuesta a ImagenResponseDto
      const imagenes = items.map(imagen => new responseDto.imagenes({
        referencia: imagen.referencia,
        imagenes: imagen.imagenes ?? [],
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

  async getExternalSubcategorias() {
    try {
      const apiUrl = "https://api01.exeldelnorte.com.mx/subcategorias";
      const response = await axios.get(apiUrl, {
        headers: {
          Authorization: apiKey,
        },
      });

      const subcategorias = response.data.datos.map(subcategoria => new responseDto.subcategoria({
        id_subcategoria: subcategoria.id_subcategoria,
        nombre_subcategoria: subcategoria.nombre_subcategoria,
        id_categoria: subcategoria.id_categoria,
      }));
      await this.saveSubcategoriasRedis(subcategorias);
      return subcategorias;
    } catch (error) {
      logger.error(
        `Error al obtener las subcategorias de Exel del Norte: ${error.message}`,
      );
      throw new Error("No se pudo obtener el catálogo de subcategorias externo");
    }
  }

  async getImagenesBatch({ idsSetKey = 'catalogo:imagenes', page = 1, limit = 50, refs = null } = {}) {
    const client = await redisClient.getClient();

    if (!client) return { total: 0, page, limit, totalPages: 0, datos: [] };

    let idsPagina;

    if (refs && refs.length > 0) {
      // Referencias específicas — sin paginar
      idsPagina = refs;
    } else {
      // Sin filtro — paginar el catálogo completo
      const allIds = await client.sMembers(idsSetKey);
      const start = (page - 1) * limit;
      idsPagina = allIds.slice(start, start + limit);
    }

    const datos = await Promise.all(
      idsPagina.map(async (referencia) => {
        const raw = await client.get(`imagenes:${referencia}`);
        return new responseDto.imagenes({
          referencia,
          imagenes: raw ? JSON.parse(raw) : [],
        });
      })
    );

    return { datos };
  }



  async getPrecio(referencia) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);

    const precio = await helper.getOne({
      keyPrefix: 'precio',
      id: referencia,
      fromHash: (hash) => ({
        referencia: hash.referencia,
        precio: parseFloat(hash.precio),
        precioOferta: parseFloat(hash.precio_oferta),
        precioSinOferta: parseFloat(hash.precio_sin_oferta),
        oferta: hash.oferta === 'true',
      }),
    });

    if (!precio) {
      return { found: false, message: `Precio de ${referencia} no encontrado` };
    }

    return { found: true, data: precio };
  }

  //Todos los productos en oferta
  async getProductosEnOferta(page = 1, limit = 50) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);

    const result = await helper.getBatchByFilters({
      keyPrefix: 'precio',
      indexKeys: [`indice:oferta:true`],
      page,
      limit,
      fromHash: (hash) => ({
        referencia: hash.referencia,
        precio: parseFloat(hash.precio),
        precioOferta: parseFloat(hash.precio_oferta),
        precioSinOferta: parseFloat(hash.precio_sin_oferta),
        oferta: hash.oferta === 'true',
      }),
    });

    return result;
  }

  /**
   * Lee todos los precios/stock paginados desde Redis, sin importar si están en oferta o no.
   */
  async getPreciosStockRedis(page = 1, limit = 50) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);

    return helper.getBatch({
      keyPrefix: 'precio_existencia',
      idsSetKey: 'catalogo:precios_existencias',
      page,
      limit,
      fromHash: (hash) => ({
        referencia: hash.referencia,
        precio: parseFloat(hash.precio ?? 0),
        precioOferta: parseFloat(hash.precio_oferta ?? 0),
        precioSinOferta: parseFloat(hash.precio_sin_oferta ?? 0),
        oferta: hash.oferta === 'true',
        existencia: parseInt(hash.existencia ?? 0),
        fechaActualizacion: hash.fecha_actualizacion ?? null,
      }),
    });
  }

  //#region Metodos Guardado en Redis
  async saveProductsRedis(productos) {
    const client = await redisClient.getClient();

    if (!client) {
        logger.warn("Redis no disponible, productos no guardados en redis");
        return;
    }

    try {
        const multi = client.multi();

        for (const p of productos) {
            if (!p.referencia) continue;

            multi.hSet(`producto:${p.referencia}`, {
                referencia:   p.referencia ?? '',
                sku:          String(p.sku ?? ''),
                nombre:       p.nombre ?? '',
                marca:        p.marca ?? '',
                categoria:    p.categoria ?? '',
                subcategoria: p.subcategoria ?? '',
                codigoBarras: p.codigoBarras ?? '',
                codigoSAT:    p.codigoSAT ?? '',
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

            // ← índice de stock
            if (p.stock > 0) {
                multi.sAdd('indice:stock:true', p.referencia);
                multi.sRem('indice:stock:false', p.referencia); // limpia el contrario
            } else {
                multi.sAdd('indice:stock:false', p.referencia);
                multi.sRem('indice:stock:true', p.referencia); // limpia el contrario
            }
        }

        await multi.exec();
        logger.info(`Se guardaron ${productos.length} productos en Redis`);
    } catch (error) {
        logger.error("Error al guardar productos en redis: " + error.message);
    }
  }

  async saveMedidasRedis(medidas) {
    const client = await redisClient.getClient();
    const helper = new RedisHelper(client);
    try {
      const { saved, skipped } = await helper.saveBatch(medidas, {
        keyPrefix: 'medida',
        idField: 'referencia',
        allKeysSet: 'catalogo:medidas',
        toHash: (m) => ({
          referencia: String(m.referencia ?? ''),
          altura: String(parseFloat(m.altura ?? 0)),
          ancho: String(parseFloat(m.ancho ?? 0)),
          largo: String(parseFloat(m.largo ?? 0)),
          peso: String(parseFloat(m.peso ?? 0)),
          medida_peso: String(m.medida_peso ?? ''),
          volumen: String(parseFloat(m.volumen ?? 0)),
          medida_volumen: String(m.medida_volumen ?? ''),
        })
      });

      logger.info(`Medidas: ${saved} guardadas, ${skipped} saltadas`);
      return { saved, skipped };
    } catch (error) {
      logger.error("Error al guardar medidas en redis: " + error.message);
      throw new Error("Error al guardar medidas en redis: " + error.message);
    }
  }

  async savePreciosExistenciaProductosRedis(precioExistencia) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);
    try {

      const { saved, skipped } = await helper.saveBatch(precioExistencia, {
        keyPrefix: 'precio_existencia',
        idField: 'referencia',
        allKeysSet: 'catalogo:precios_existencias',
        toHash: (p) => ({
          referencia: String(p.referencia ?? ''),
          precio: String(p.precio ?? 0),
          precio_oferta: String(p.precioOferta ?? 0),
          precio_sin_oferta: String(p.precioSinOferta ?? 0),
          oferta: String(p.oferta ?? false),
          existencia: String(p.existencia ?? 0),
          fecha_actualizacion: new Date().toISOString(),
        })
      })
      logger.info(`Precios y existencias: ${saved} guardados, ${skipped} saltados`);
    } catch (error) {
      logger.error("Error al guardar precios y existencias en redis: " + error.message);
      throw new Error("Error al guardar precios y existencias en redis: " + error.message)
    }
  }

  async saveFichasTecnicasRedis(fichas) {
    const client = await redisClient.getClient();
    const helper = new RedisHelper(client);

    const { saved, skipped } = await helper.saveJsonBatch(fichas, {
      keyPrefix: 'ficha_tecnica',
      idField: 'referencia',
      allKeysSet: 'catalogo:fichas_tecnicas',
      toJson: (f) => ({
        caracteristicasGenerales: f.caracteristicasGenerales,
        detalleTecnico: f.detalleTecnico,
        especificacionesGenerales: f.especificacionesGenerales,
      }),
      indices: [], // no necesarios con solo 3 grupos fijos
    });

    logger.info(`Fichas técnicas: ${saved} guardadas, ${skipped} saltadas`);
    return { saved, skipped };
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
          id: String(c.id ?? ''),
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

  async saveBrandsRedis(marcas) {
    const client = await redisClient.getClient();
    const helper = new RedisHelper(client);

    try {
      const { saved, skipped } = await helper.saveBatch(marcas, {
        keyPrefix: 'marca',
        idField: 'id_marca',
        allKeysSet: 'catalogo:marcas',
        toHash: (m) => ({
          id_marca: String(m.id_marca ?? ''),
          nombre_marca: String(m.nombre?? ''),
        }),
        indices: [],
      })
      logger.info(`Marcas: ${saved} guardadas, ${skipped} saltadas`);
      return { saved, skipped }
    } catch (error) {
      logger.error("Error al guardar marcas en redis: " + error.message);
      throw new Error("Error al guardar marcas en redis: " + error.message)
    }
  }

  async saveSubcategoriasRedis(subcategorias) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);
    try {
      const { saved, skipped } = await helper.saveBatch(subcategorias, {
        keyPrefix: 'subcategoria',
        idField: 'id_subcategoria',
        allKeysSet: 'catalogo:subcategorias',
        toHash: (s) => ({
          id_subcategoria: String(s.id_subcategoria ?? ''),
          nombre_subcategoria: String(s.nombre_subcategoria ?? ''),
          id_categoria: String(s.id_categoria ?? ''),
        }),
        indices: [{ name: 'id_categoria', keyPrefix: 'indice:categoria' }],
      });
      logger.info(`Subcategorias: ${saved} guardadas, ${skipped} saltadas`);
    }
    catch (error) {
      logger.error("Error al guardar subcategorias en redis: " + error.message);
      throw new Error("Error al guardar subcategorias en redis: " + error.message)
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
  // ----------------------------------------------------------

  /**
   * Lee subcategorías desde Redis (todas, sin paginar).
   */
  async getSubcategoriasRedis() {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);
    return helper.getAll({
      keyPrefix: 'subcategoria',
      idsSetKey: 'catalogo:subcategorias',
    });
  }

  
  /**
   * Lee medidas paginadas desde Redis.
   */
  async getMedidasRedis(page = 1, limit = 50) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);
    return helper.getBatch({
      keyPrefix: 'medida',
      idsSetKey: 'catalogo:medidas',
      page,
      limit,
      fromHash: (hash) => ({
        referencia: hash.referencia,
        altura: parseFloat(hash.altura ?? 0),
        ancho: parseFloat(hash.ancho ?? 0),
        largo: parseFloat(hash.largo ?? 0),
        peso: parseFloat(hash.peso ?? 0),
        medida_peso: hash.medida_peso ?? '',
        volumen: parseFloat(hash.volumen ?? 0),
        medida_volumen: hash.medida_volumen ?? '',
      }),
    });
  }

  /**
   * Obtener medidas de un producto por referencia desde redis
   */
  async getMedidaRedisRef(referencia) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);
    const data = await helper.getOne({
      keyPrefix: 'medida',
      id: referencia,
      fromHash: (hash) => ({
        referencia: hash.referencia,
        altura: parseFloat(hash.altura ?? 0),
        ancho: parseFloat(hash.ancho ?? 0),
        largo: parseFloat(hash.largo ?? 0),
        peso: parseFloat(hash.peso ?? 0),
        medida_peso: hash.medida_peso ?? '',
        volumen: parseFloat(hash.volumen ?? 0),
        medida_volumen: hash.medida_volumen ?? '',
      }),
    });
    if (!data) {
      return null;
    }
    return data;
  }

  /**
   * Lee fichas técnicas paginadas desde Redis.
   */
  async getFichasTecnicasRedis(page = 1, limit = 50) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);
    return helper.getJsonBatch({
      keyPrefix: 'ficha_tecnica',
      idsSetKey: 'catalogo:fichas_tecnicas',
      page,
      limit,
      fromJson: (parsed, id) => ({ referencia: id, fichaTecnica: parsed }),
    });
  }

  /**
   * Obtener ficha tecnica por referencia desde redis
   */
  async getFichaTecnicaRedisRef(referencia) {
    const client = redisClient.getClient();
    const helper = new RedisHelper(client);
    const data = await helper.getJson({
      keyPrefix: 'ficha_tecnica',
      id: referencia,
      fromJson: (parsed, id) => ({ referencia: id, fichaTecnica: parsed }),
   })
    if (!data) {
      return null;
    }
    return data;
  }

  //#endregion
}


export default new ExelService();
