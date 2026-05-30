import axios from "axios";
import logger from "../utils/logger.js";
import dotenv from "dotenv";
import ProductosCreateDto from "../dtos/productos-create.dto.js";
import ProductosResponseDto from "../dtos/productos-response.dto.js";

dotenv.config();

const apiKey = process.env.API_KEY;
class ExelService {
  async fetchExternalProducts(queryParams) {
    try {
      const apiUrl = "https://api01.exeldelnorte.com.mx/productos";

      const filtros = new ProductosCreateDto(queryParams);

      // Realiza la petición GET mapeando los Query Params opcionales de la documentación
      const response = await axios.get(apiUrl, {
        headers: {
          Authorization: apiKey,
        },
        params: filtros, // Pasa automáticamente sin_stock, marca, subcategoria, categoria, familia
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
        `Error al obtener las categorias de Exel del Norte: ${error.message}`,
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
