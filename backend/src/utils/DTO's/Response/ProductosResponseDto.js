class ProductosResponseDto {
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

  // Método estático para convertir un array
  static fromArray(items) {
    if (!Array.isArray(items)) return [];
    return items.map(item => new ProductosResponseDto(item));
  }
}

export default ProductosResponseDto;