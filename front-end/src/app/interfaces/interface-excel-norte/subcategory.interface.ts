// Interfaz para la respuesta de la API de subcategorías
export interface ISubcategoryResponse {
    id_subcategoria: string;
    nombre_subcategoria: string;
    id_categoria?: string;
    nombre_categoria?: string;
}

// Interfaz para la respuesta completa de la API
export interface IApiSubcategoriesResponse {
    resultado: boolean;
    mensaje: string;
    datos: ISubcategoryResponse[];
}