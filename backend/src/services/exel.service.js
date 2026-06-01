import axios from 'axios';
import logger from '../utils/logger.js';
import redisClient from '../config/redis.js';
import ExelProductoResponseDto from "../utils/DTO's/Response/ExelProductoResponseDto.js";
import dotenv from 'dotenv';

dotenv.config();

const apiKey = process.env.API_TOKEN;

class ExelService {

    /**
     * @param {Object} [queryParams]
     * @returns {Promise<ExelProductoResponseDto[]>}
     */
    async fetchExternalProductsWithCache(queryParams) {
        const cacheKey = `exel:products:${JSON.stringify(queryParams ?? {})}`;

        return redisClient.getOrSet(cacheKey, async () => {

            const { data } = await axios.get('https://api01.exeldelnorte.com.mx/productos', {
                headers: { Authorization: apiKey },
                params: queryParams ?? {}
            });

            // Aquí está la máscara:
            // cada producto del arreglo pasa por el DTO antes de guardarse en Redis
            // y antes de llegar al controlador
            return data.datos.map((item) => new ExelProductoResponseDto(item));

        }, 600); // TTL: 10 minutos en Redis
    }

    /**
     * Sin DTO aún — estructura pendiente de definir
     * @returns {Promise<any>}
     */
    async getCategories() {
        try {
            const { data } = await axios.get('https://api01.exeldelnorte.com.mx/categorias', {
                headers: { Authorization: apiKey }
            });
            return data;
        } catch (error) {
            logger.error(`Error al obtener las categorias de Exel del Norte: ${error.message}`);
            throw new Error('No se pudo obtener el catálogo de categorias externo');
        }
    }

    /**
     * Sin DTO aún — estructura pendiente de definir
     * @param {Object} [query]
     * @returns {Promise<any>}
     */
    async getImagenes(query) {
        try {
            const { data } = await axios.get('https://api01.exeldelnorte.com.mx/imagenes', {
                headers: { Authorization: apiKey },
                params: query && Object.keys(query).length > 0 ? query : {}
            });
            return data;
        } catch (error) {
            logger.error(`Error al obtener las imagenes de Exel del Norte: ${error.message}`);
            throw new Error('No se pudo obtener el catálogo de imagenes externo');
        }
    }
}

export default new ExelService();