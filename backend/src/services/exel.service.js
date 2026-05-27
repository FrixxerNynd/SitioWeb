import axios from 'axios';
import logger from '../utils/logger.js';

class ExelService {
    async fetchExternalProducts(queryParams) {
        try {
            const apiKey = 'API_6196eef5a6a5cc36c14885c19c55c81057e603c4';
            const apiUrl = 'https://api01.exeldelnorte.com.mx/productos';

            // Realiza la petición GET mapeando los Query Params opcionales de la documentación
            const response = await axios.get(apiUrl, {
                headers: {
                    'Authorization': apiKey
                },
                params: queryParams // Pasa automáticamente sin_stock, marca, subcategoria, categoria, familia
            });

            return response.data;
        } catch (error) {
            logger.error(`Error al conectar con la API de Exel del Norte: ${error.message}`);
            throw new Error('No se pudo obtener el catálogo de productos externo.');
        }
    }
}

export default new ExelService();