/**
 * id_subcategoria: subcategoria.id_subcategoria,
 * nombre_subcategoria: subcategoria.nombre_subcategoria,
 * id_categoria: subcategoria.id_categoria
 */
export default class SubCategoriaDto {
    constructor(data = {}) {
        this.id_subcategoria = data.id_subcategoria ?? '',
            this.nombre_subcategoria = data.nombre_subcategoria ?? '',
            this.id_categoria = data.id_categoria ?? 0
    }
}