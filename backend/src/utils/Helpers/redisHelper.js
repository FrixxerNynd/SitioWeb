import logger from "./logger.js";

class RedisHelper {
    constructor(client) {
        this.client = client;
    }
    /**
     * Guarda un batch de entidades como hashes + índices secundarios, en una sola transacción.
     * @param {Array} items - array de objetos a guardar
     * @param {Object} options
     * @param {string} options.keyPrefix - ej. 'producto'
     * @param {string} options.idField - campo que se usa como ID (ej. 'referencia')
     * @param {Function} options.toHash - (item) => objeto plano para hSet
     * @param {Array} [options.indices] - [{ name: 'categoria', field: 'categoria' }, ...]
     * @param {string} [options.allKeysSet] - ej. 'catalogo:referencias'
     */
    async saveBatch(items, { keyPrefix, idField, toHash, indices = [], allKeysSet }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se guardó ${keyPrefix}`)
            return { saved: 0, skipped: items.length };
        }

        const multi = this.client.multi();
        let saved = 0;
        let skipped = 0;

        for (const item of items) {
            const id = item[idField];
            if (!id) {
                skipped++;
                continue;
            }
            //Hash Principal
            const hashData = toHash(item);
            multi.hSet(`${keyPrefix}:${id}`, hashData);

            //Set general de todos los id's
            if (allKeysSet) {
                multi.sAdd(allKeysSet, String(id));
            }

            //Indices secundarios
            for (const index of indices) {
                const value = item[index.field];
                if (value) {
                    multi.sAdd(`indice:${index.name}:${value}`, String(id));
                }
            }
            saved++;
        }
        await multi.exec();
        logger.info(`Se guardaron ${saved} items de ${keyPrefix}, skipped: ${skipped}`);

        return { saved, skipped };
    }

    async saveJsonBatch(items, { keyPrefix, idField, toJson, allKeysSet }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se guardó ${keyPrefix}`);
            return { saved: 0, skipped: items.length };
        }

        const multi = this.client.multi();
        let saved = 0;
        let skipped = 0;

        for (const item of items) {
            const id = item[idField];
            if (!id) { skipped++; continue; }

            const jsonData = toJson(item);
            if (!jsonData || (Array.isArray(jsonData) && jsonData.length === 0)) {
                skipped++;
                continue;
            }

            multi.set(`${keyPrefix}:${id}`, JSON.stringify(jsonData));
            if (allKeysSet) multi.sAdd(allKeysSet, String(id));

            saved++;
        }

        await multi.exec();
        return { saved, skipped };
    }


    /**
     * Obtiene una página de entidades a partir de un Set que contiene los IDs.
     * @param {Object} options
     * @param {string} options.keyPrefix - ej. 'categoria'
     * @param {string} options.idsSetKey - ej. 'catalogo:categorias' o 'indice:categoria:17'
     * @param {number} [options.page=1]
     * @param {number} [options.limit=50]
     * @param {Function} [options.fromHash] - (hash) => objeto transformado (opcional)
     */
    async getBatch({ keyPrefix, idsSetKey, page = 1, limit = 50, fromHash }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se pudo leer ${keyPrefix}`);
            return { total: 0, page, limit, totalPages: 0, datos: [] };
        }

        const allIds = await this.client.sMembers(idsSetKey);
        const total = allIds.length;

        const start = (page - 1) * limit;
        const idsPagina = allIds.slice(start, start + limit);

        const entidades = await Promise.all(
            idsPagina.map(async (id) => {
                const hash = await this.client.hGetAll(`${keyPrefix}:${id}`);
                return fromHash ? fromHash(hash) : hash;
            })
        );
        return {
            total,
            page,
            limit,
            totalPages: Math.ceil(total / limit),
            datos: entidades,
        };
    }

    /**
 * Obtiene UNA entidad por su ID.
 */
    async getOne({ keyPrefix, id, fromHash }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se pudo leer ${keyPrefix}:${id}`);
            return null;
        }
        const hash = await this.client.hGetAll(`${keyPrefix}:${id}`);
        if (Object.keys(hash).length == 0) {
            return null;
        }
        return fromHash ? fromHash(hash) : hash;
    }

    /**
     * Obtiene TODAS las entidades de un Set sin paginar (cuidado con sets grandes).
     */
    async getAll({ keyPrefix, idsSetKey, fromHash }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se pudo leer ${keyPrefix}`);
            return [];
        }

        const allIds = await this.client.sMembers(idsSetKey);

        const entidades = await Promise.all(
            allIds.map(async (id) => {
                const hash = await this.client.hGetAll(`${keyPrefix}:${id}`);
                return fromHash ? fromHash(hash) : hash;
            })
        );
        return entidades;
    }

    /**
    * Intersección de múltiples índices (filtros combinados).
    */
    async getBatchByFilters({ keyPrefix, indexKeys, page = 1, limit = 50, fromHash }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se pudo leer ${keyPrefix}`);
            return { total: 0, page, limit, totalPages: 0, datos: [] };
        }

        const allIds = indexKeys.length === 1
            ? await this.client.sMembers(indexKeys[0])
            : await this.client.sInter(indexKeys);

        const total = allIds.length;
        const start = (page - 1) * limit;
        const idsPagina = allIds.slice(start, start + limit);

        const entidades = await Promise.all(
            idsPagina.map(async (id) => {
                const hash = await this.client.hGetAll(`${keyPrefix}:${id}`);
                return fromHash ? fromHash(hash) : hash;
            })
        );

        return {
            total,
            page,
            limit,
            totalPages: Math.ceil(total / limit),
            datos: entidades,
        };
    }

    /**
    * Lee UN solo valor JSON por ID.
    * @param {string} keyPrefix - ej. 'ficha_tecnica'
    * @param {string} id - ej. '3MCACCAC016'
    * @param {Function} [fromJson] - (parsed) => objeto transformado (opcional)
    */
    async getJson({ keyPrefix, id, fromJson }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se pudo leer ${keyPrefix}:${id}`);
            return null;
        }

        const raw = await this.client.get(`${keyPrefix}:${id}`);
        if (!raw) return null;

        const parsed = JSON.parse(raw);
        return fromJson ? fromJson(parsed) : parsed;
    }

    /**
     * Lee un batch paginado de valores JSON desde un Set de IDs.
     * @param {string} keyPrefix - ej. 'imagenes'
     * @param {string} idsSetKey - ej. 'catalogo:imagenes'
     * @param {number} [page=1]
     * @param {number} [limit=50]
     * @param {Function} [fromJson] - (parsed, id) => objeto transformado (opcional)
     */
    async getJsonBatch({ keyPrefix, idsSetKey, page = 1, limit = 50, fromJson }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se pudo leer ${keyPrefix}`);
            return { total: 0, page, limit, totalPages: 0, datos: [] };
        }

        const allIds = await this.client.sMembers(idsSetKey);
        const total = allIds.length;
        const start = (page - 1) * limit;
        const idsPagina = allIds.slice(start, start + limit);

        const datos = await Promise.all(
            idsPagina.map(async (id) => {
                const raw = await this.client.get(`${keyPrefix}:${id}`);
                if (!raw) return null;

                const parsed = JSON.parse(raw);
                return fromJson ? fromJson(parsed, id) : { id, datos: parsed };
            })
        );

        return {
            total,
            page,
            limit,
            totalPages: Math.ceil(total / limit),
            datos: datos.filter(Boolean), // elimina nulls si alguna key no existía
        };
    }

    /**
     * Lee TODOS los valores JSON de un Set sin paginar (para sets pequeños).
     * @param {string} keyPrefix
     * @param {string} idsSetKey
     * @param {Function} [fromJson]
     */
    async getAllJson({ keyPrefix, idsSetKey, fromJson }) {
        if (!this.client) {
            logger.warn(`Redis no disponible, no se pudo leer ${keyPrefix}`);
            return [];
        }

        const allIds = await this.client.sMembers(idsSetKey);

        const datos = await Promise.all(
            allIds.map(async (id) => {
                const raw = await this.client.get(`${keyPrefix}:${id}`);
                if (!raw) return null;

                const parsed = JSON.parse(raw);
                return fromJson ? fromJson(parsed, id) : { id, datos: parsed };
            })
        );

        return datos.filter(Boolean);
    }
}

export default RedisHelper;