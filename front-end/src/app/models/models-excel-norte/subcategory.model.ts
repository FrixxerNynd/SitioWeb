import { ISubcategoryResponse } from '../../interfaces/interface-excel-norte/subcategory.interface'; 
import { Category } from './category.model';

export class Subcategory {
    public id: string;
    public nombre: string;
    public categoriaId?: string;
    public categoriaNombre?: string;
    public nombreNormalizado: string;
    public slug: string;

    constructor(data: ISubcategoryResponse) {
        this.id = data.id_subcategoria;
        this.nombre = data.nombre_subcategoria;
        this.categoriaId = data.id_categoria;
        this.categoriaNombre = data.nombre_categoria;
        this.nombreNormalizado = this.normalizarTexto(data.nombre_subcategoria);
        this.slug = this.generarSlug(data.nombre_subcategoria);
    }

    private normalizarTexto(texto: string): string {
        return texto
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .trim();
    }

    private generarSlug(texto: string): string {
        return texto
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-|-$/g, '');
    }

    // Obtener nombre completo incluyendo categoría
    getNombreCompleto(): string {
        return this.categoriaNombre 
            ? `${this.categoriaNombre} > ${this.nombre}`
            : this.nombre;
    }

    // Verificar si pertenece a una categoría
    perteneceACategoria(categoriaId: string): boolean {
        return this.categoriaId === categoriaId;
    }

    // Para usar en selects de HTML
    toSelectOption(): { value: string; label: string; group?: string } {
        return {
            value: this.id,
            label: this.nombre,
            group: this.categoriaNombre
        };
    }

    // Crear desde JSON
    static fromJSON(json: any): Subcategory {
        return new Subcategory({
            id_subcategoria: json.id,
            nombre_subcategoria: json.nombre,
            id_categoria: json.categoriaId,
            nombre_categoria: json.categoriaNombre
        });
    }
}