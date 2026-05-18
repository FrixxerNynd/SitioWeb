import { IBrandResponse } from "../../interfaces/interface-excel-norte/brand.interface"; 

export class Brand {
    public id: string;
    public nombre: string;
    public nombreNormalizado: string;
    public slug: string;
    public logoUrl?: string;
    public descripcion?: string;

    constructor(data: IBrandResponse) {
        this.id = data.id;
        this.nombre = data.nombre;
        this.nombreNormalizado = this.normalizarTexto(data.nombre);
        this.slug = this.generarSlug(data.nombre);
        this.logoUrl = data.logo_url;
        this.descripcion = data.descripcion;
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

    // Obtener iniciales para avatar (ej: "AS" para ASUS)
    getIniciales(): string {
        return this.nombre
            .split(' ')
            .map(palabra => palabra.charAt(0))
            .join('')
            .toUpperCase()
            .substring(0, 2);
    }

    // Verificar si la marca tiene productos (requiere servicio de productos)
    async tieneProductos(productService: any): Promise<boolean> {
        // Esta función necesitaría inyección del servicio de productos
        // Implementación pendiente
        return true;
    }

    // Para usar en selects de HTML
    toSelectOption(): { value: string; label: string } {
        return {
            value: this.id,
            label: this.nombre
        };
    }

    // Crear desde JSON
    static fromJSON(json: any): Brand {
        return new Brand({
            id: json.id,
            nombre: json.nombre,
            logo_url: json.logoUrl,
            descripcion: json.descripcion
        });
    }
}