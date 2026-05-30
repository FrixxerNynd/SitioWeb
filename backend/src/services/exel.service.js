import axios from 'axios';
import logger from '../utils/logger.js';
import redisClient from '../config/redis.js';  // 🔥 RUTA CORRECTA (sube un nivel a config)
import dotenv from "dotenv";
import ProductosCreateDto from "../utils/DTO's/Request/ProductosCreateDto.js";
import ProductosResponseDto from "../utils/DTO's/Response/ProductosResponseDto.js";

dotenv.config();

const apiKey = process.env.API_KEY;
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
      return ProductosResponseDto.fromArray(items);
    } catch (error) {
      logger.error(
        `Error al conectar con la API de Exel del Norte: ${error.message}`,
      );
      throw new Error("No se pudo obtener el catálogo de productos externo.");
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
  

  async getCategories() {
    try {
      const apiUrl = "https://api01.exeldelnorte.com.mx/categorias";
      const response = await axios.get(apiUrl, {
        headers: {
          Authorization: apiKey,
        },
      });
      return response.data;
    } catch (error) {
      logger.error(
        `Error al obtener las categorias de Exel del Norte: ${error.message}`
      );
      throw new Error("No se pudo obtener el catálogo de categorias externo");
    }
  }

  async getImagenes(query) {
    try {
      const apiUrl = "https://api01.exeldelnorte.com.mx/imagenes";
      //Verificar si query trae informacion
      if (query.length > 0) {
        const response = await axios.get(apriUrl, {
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

      return response.data;
    } catch (error) {
      logger.error(
        `Error al obtener las imagenes de Exel del Norte: ${error.message}`,
      );
      throw new Error("No se pudo obtener el catálogo de imagenes externo");
    }
  }
}


export default new ExelService();
