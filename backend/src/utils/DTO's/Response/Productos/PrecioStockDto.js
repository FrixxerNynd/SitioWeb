class PrecioStockResponseDto {
    /**
     * @param {Object} data
     * @param {string} [data.referencia]
     * @param {number} [data.existencia]
     * @param {number} [data.precio]
     * @param {number} [data.precioOferta]
     * @param {number} [data.precioSinOferta]
     * @param {boolean} [data.oferta]
     */
    constructor(data = {}) {
        this.referencia = data.referencia ?? '';
        this.existencia = data.existencia ?? 0;
        this.precio = data.precio ?? 0;
        this.precioOferta = data.precioOferta ?? 0;
        this.precioSinOferta = data.precioSinOferta ?? 0;
        this.oferta = data.oferta ?? false;
    }
}

export default PrecioStockResponseDto;
