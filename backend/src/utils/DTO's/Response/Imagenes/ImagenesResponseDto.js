class ImagenesResponseDto {
    /**
       * @param {Object} data
       * @param {number} data.referencia
       * @param {string[]} [data.imagenes]
       */

    constructor(data = {}) {
        this.referencia = data.referencia ?? 0;
        this.imagenes = Array.isArray(data.imagenes) ? data.imagenes : [];
    }
}

export default ImagenesResponseDto;