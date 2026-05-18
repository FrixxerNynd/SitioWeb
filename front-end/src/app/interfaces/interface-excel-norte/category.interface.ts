// Interfaz para la respuesta de la API de categorías
export interface ICategoryResponse {
    id_categoria: string;
    nombre_categoria: string;
}

// Interfaz para la respuesta completa de la API
export interface IApiCategoriesResponse {
    resultado: boolean;
    mensaje: string;
    datos: ICategoryResponse[];
}