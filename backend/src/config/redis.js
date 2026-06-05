import { createClient } from 'redis';
import logger from '../utils/logger.js';

class RedisClient {
    constructor() {
        this.client = null;
        this.isConnected = false;
    }

    async connect() {
        try {
            // 🔥 CAMBIAR EL PUERTO A 6379
            this.client = createClient({
                url: 'redis://localhost:6379'  // ← Puerto corregido
            });

            this.client.on('connect', () => {
                this.isConnected = true;
                logger.info('✅ Conectado a Redis correctamente');
            });

            this.client.on('error', (err) => {
                this.isConnected = false;
                logger.error(`❌ Error en Redis: ${err.message}`);
            });

            await this.client.connect();
            return this.client;
        } catch (error) {
            logger.error(`No se pudo conectar a Redis: ${error.message}`);
            this.isConnected = false;
            return null;
        }
    }

    getClient() {
        if (!this.client || !this.isConnected) {
            logger.warn('Redis no está disponible, operando sin caché');
            return null;
        }
        return this.client;
    }

    async getOrSet(cacheKey, fetchCallback, ttlSeconds = 300) {
        const client = this.getClient();
        
        if (!client) {
            return await fetchCallback();
        }

        try {
            const cachedData = await client.get(cacheKey);
            
            if (cachedData) {
                logger.info(`📦 Cache HIT: ${cacheKey}`);
                return JSON.parse(cachedData);
            }

            logger.info(`🔍 Cache MISS: ${cacheKey}`);
            const freshData = await fetchCallback();
            
            if (freshData) {
                await client.setEx(cacheKey, ttlSeconds, JSON.stringify(freshData));
                logger.info(`💾 Guardado en caché: ${cacheKey}`);
            }
            
            return freshData;
        } catch (error) {
            logger.error(`Error en caché: ${error.message}`);
            return await fetchCallback();
        }
    }

    async deletePattern(pattern) {
        const client = this.getClient();
        if (client) {
            const keys = await client.keys(pattern);
            if (keys.length > 0) {
                await client.del(keys);
                logger.info(`🗑️ Eliminados ${keys.length} elementos: ${pattern}`);
            }
        }
    }

    async disconnect() {
        if (this.client && this.isConnected) {
            await this.client.quit();
            logger.info('Redis desconectado');
        }
    }
}

export default new RedisClient();