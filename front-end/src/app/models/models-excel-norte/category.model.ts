import { ICategoryResponse } from "../../interfaces/interface-excel-norte/category.interface";
import { Subcategory } from "./subcategory.model";

export class Category {
    public id: string;
    public nombre: string;
    public nombreNormalizado: string;
    public slug: string;

    constructor(data: ICategoryResponse) {
        this.id = data.id_categoria;
        this.nombre = data.nombre_categoria;
        this.nombreNormalizado = this.normalizarTexto(data.nombre_categoria);
        this.slug = this.generarSlug(data.nombre_categoria);
    }

    // Normalizar texto para búsquedas (quitar acentos, mayúsculas, etc.)
    private normalizarTexto(texto: string): string {
        return texto
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .trim();
    }

    // Generar slug para URLs amigables
    private generarSlug(texto: string): string {
        return texto
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-|-$/g, '');
    }

    // Método para obtener el nombre formateado (primera letra mayúscula)
    getNombreFormateado(): string {
        return this.nombre
            .split(' ')
            .map(palabra => palabra.charAt(0).toUpperCase() + palabra.slice(1).toLowerCase())
            .join(' ');
    }

    // Verificar si la categoría tiene subcategorías
    async tieneSubcategorias(subcategorias: Subcategory[]): Promise<boolean> {
        return subcategorias.some(sub => sub.categoriaId === this.id);
    }

    // Obtener subcategorías de esta categoría
    obtenerSubcategorias(subcategorias: Subcategory[]): Subcategory[] {
        return subcategorias.filter(sub => sub.categoriaId === this.id);
    }

    // Para usar en selects de HTML
    toSelectOption(): { value: string; label: string } {
        return {
            value: this.id,
            label: this.nombre
        };
    }

    // Crear desde JSON (útil para localStorage)
    static fromJSON(json: any): Category {
        return new Category({
            id_categoria: json.id,
            nombre_categoria: json.nombre
        });
    }
}