import { createClient } from 'redis';
import logger from '../utils/logger.js';

class RedisClient {
  /**
   * @type {import('redis').RedisClientType | null}
   */
  #client = null;

  /**
   * @type {boolean}
   */
  #isConnected = false;

  async connect() {
    try {
      this.#client = createClient({
        url: `redis://${process.env.REDIS_HOST || 'localhost'}:${process.env.REDIS_PORT || 6379}`,
        socket: {
          connectTimeout: 5000,      // falla rápido si Redis no responde
          reconnectStrategy: (retries) => Math.min(retries * 100, 3000)
        }
      });

      this.#client.on('connect',      () => { this.#isConnected = true;  logger.info('✅ Conectado a Redis'); });
      this.#client.on('disconnect',   () => { this.#isConnected = false; logger.warn('⚠️ Redis desconectado'); });
      this.#client.on('error', (err)  => { this.#isConnected = false;    logger.error(`❌ Redis error: ${err.message}`); });

      await this.#client.connect();
      return this.#client;
    } catch (error) {
      logger.error(`No se pudo conectar a Redis: ${error.message}`);
      this.#isConnected = false;
      return null;
    }
  }

  /**
   * @returns {import('redis').RedisClientType | null}
   */
  getClient() {
    if (!this.#client || !this.#isConnected) {
      logger.warn('Redis no disponible, operando sin caché');
      return null;
    }
    return this.#client;
  }

  /**
   * @param {string}   cacheKey
   * @param {Function} fetchCallback
   * @param {number}   [ttlSeconds=300]
   * @returns {Promise<any>}
   */
  async getOrSet(cacheKey, fetchCallback, ttlSeconds = 300) {
    const client = this.getClient();
    if (!client) return fetchCallback();

    try {
      const cached = await client.get(cacheKey);
      if (cached) {
        logger.info(`📦 Cache HIT: ${cacheKey}`);
        return JSON.parse(cached);
      }

      logger.info(`🔍 Cache MISS: ${cacheKey}`);
      const freshData = await fetchCallback();

      if (freshData != null) {
        // pipeline: guarda sin bloquear la respuesta
        client.setEx(cacheKey, ttlSeconds, JSON.stringify(freshData)).catch((err) =>
          logger.error(`Error guardando caché: ${err.message}`)
        );
        logger.info(`💾 Guardado en caché: ${cacheKey} (TTL ${ttlSeconds}s)`);
      }

      return freshData;
    } catch (error) {
      logger.error(`Error en caché: ${error.message}`);
      return fetchCallback();
    }
  }

  /**
   * @param {string} pattern
   */
  async deletePattern(pattern) {
    const client = this.getClient();
    if (!client) return;

    const keys = await client.keys(pattern);
    if (keys.length > 0) {
      await client.del(keys);
      logger.info(`🗑️ Eliminados ${keys.length} keys: ${pattern}`);
    }
  }

  async disconnect() {
    if (this.#client && this.#isConnected) {
      await this.#client.quit();
      logger.info('Redis desconectado');
    }
  }
}

export default new RedisClient();