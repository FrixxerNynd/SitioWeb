export interface IProductosPageResponse {
    productos: IProduct[];
    total: number;
    page: number;       // ← viene de Pagina
    pageSize: number;
    totalPages: number; // ← viene de Paginas
}

export interface IProduct {
    id: string;
    referencia: string;
    sku: string;
    nombre: string;
    descripcion: string;
    codigoSat: string;
    codigoBarras: string;
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
    imagenes?: string[];       
    imagen_principal?: string | null;    
}

export interface IBrand {
    id: string;
    nombre: string;
    logo_url?: string;
    descripcion?: string;
}

export interface ICategory {
    id_categoria: string;
    nombre_categoria: string;
}

export interface ISubcategory {
    id_subcategoria: string;
    nombre_subcategoria: string;
    id_categoria?: string;
    nombre_categoria?: string;
}

