// Interfaz para la respuesta de la API de marcas
export interface IBrandResponse {
    id: string;        
    nombre: string;    
    logo_url?: string;
    descripcion?: string;
}

// Interfaz para la respuesta completa de la API
export interface IApiBrandsResponse {
    resultado: boolean;
    mensaje: string;
    datos: IBrandResponse[];
}