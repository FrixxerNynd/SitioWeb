import axios from 'axios';
import logger from '../utils/logger.js';
import redisClient from '../config/redis.js';  // 🔥 RUTA CORRECTA (sube un nivel a config)

class ExelService {
    
    // Método original - se mantiene igual
    async fetchExternalProducts(queryParams) {
        try {
            const apiKey = 'API_6196eef5a6a5cc36c14885c19c55c81057e603c4';
            const apiUrl = 'https://api01.exeldelnorte.com.mx/productos';

            const response = await axios.get(apiUrl, {
                headers: {
                    'Authorization': apiKey
                },
                params: queryParams
            });

            return response.data;
        } catch (error) {
            logger.error(`Error al conectar con la API de Exel del Norte: ${error.message}`);
            throw new Error('No se pudo obtener el catálogo de productos externo.');
        }
    }

    // 🔥 NUEVO MÉTODO CON CACHÉ
    async fetchExternalProductsWithCache(queryParams) {
        const cacheKey = `exel:products:${JSON.stringify(queryParams)}`;
        const TTL_SECONDS = 600; // 10 minutos
        
        const fetchFreshData = async () => {
            return await this.fetchExternalProducts(queryParams);
        };
        
        return await redisClient.getOrSet(cacheKey, fetchFreshData, TTL_SECONDS);
    }
}

export default new ExelService();