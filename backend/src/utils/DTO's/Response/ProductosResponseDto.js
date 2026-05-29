class ProductosResponseDto {
  /**
   * @param {Object} data
   * @param {number} data.id
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
   * @param {Date|string} [data.fechaCreacion]
   */
  constructor(data = {}) {
    this.id = data.id ?? 0;
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
    this.fechaCreacion = data.fechaCreacion ? new Date(data.fechaCreacion) : new Date();
  }
}

module.exports = ProductosResponseDto;
