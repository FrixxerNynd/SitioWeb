class ProductosCreateDto {
  /**
   * @param {Object} data
   * @param {string} [data.nombre]
   * @param {number} data.precio
   * @param {number} data.precioOriginal
   * @param {number} data.stock
   * @param {string} [data.descripcion]
   * @param {string} [data.sku]
   * @param {string} [data.codigoSAT]
   * @param {string} [data.codigoBarras]
   * @param {string} [data.marca]
   * @param {string} [data.subcategoria]
   */
  constructor(data = {}) {
    this.nombre = data.nombre ?? '';
    this.precio = data.precio ?? 0;
    this.precioOriginal = data.precioOriginal ?? 0;
    this.stock = data.stock ?? 0;
    this.descripcion = data.descripcion ?? '';
    this.sku = data.sku ?? '';
    this.codigoSAT = data.codigoSAT ?? '';
    this.codigoBarras = data.codigoBarras ?? '';
    this.marca = data.marca ?? '';
    this.subcategoria = data.subcategoria ?? '';
  }
}

module.exports = ProductosCreateDto;
