class CategoriaResponseDto {
    /**
       * @param {Object} data
       * @param {number} data.id
       * @param {string} [data.nombre_categoria]
       */

    constructor(data = {}) {
        this.id = data.id ?? 0;
        this.nombre_categoria = data.nombre_categoria ?? '';
    }
}

export default CategoriaResponseDto;