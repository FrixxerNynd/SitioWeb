class PedidoCreateDto {
  /**
   * @param {Object} data
   * @param {number} data.usuarioId
   * @param {string} [data.transportista]
   * @param {number[]} [data.productoIds]
   * @param {number} data.subtotal
   * @param {number} data.flete
   * @param {number} data.iva
   * @param {number} data.total
   */
  constructor(data = {}) {
    this.usuarioId = data.usuarioId ?? 0;
    this.transportista = data.transportista ?? '';
    this.productoIds = data.productoIds ?? [];
    this.subtotal = data.subtotal ?? 0;
    this.flete = data.flete ?? 0;
    this.iva = data.iva ?? 0;
    this.total = data.total ?? 0;
  }
}

export default PedidoCreateDto;
