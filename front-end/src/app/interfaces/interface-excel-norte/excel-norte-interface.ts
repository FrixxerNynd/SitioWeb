export interface IProduct {
    id: string;
    referencia: string;
    sku: string;
    nombre: string;
    stock: string;
    precio: string;
    precio_oferta: string | null;
    precio_sin_oferta: string;
    oferta: boolean;
    moneda: string;
    marca_id: string;
    marca_nombre: string;
    subcategoria_id: string;
    subcategoria_nombre: string;
    categoria_id: string;
    categoria_nombre: string;
}

export interface ICategory {
    id_categoria: string;
    nombre_categoria: string;
}

export interface ISubcategory {
    id_subcategoria: string;
    nombre_subcategoria: string;
}

export interface IBrand {
    id: string;
    nombre: string;
}

export interface IApiResponse<T> {
    resultado: boolean;
    mensaje: string;
    datos: T;
}