class ProductoCotizadoDto {
  constructor(data = {}) {
    this.IdProducto = data.IdProducto ?? 0;
    this.Codigo = data.Codigo ?? '';
    this.Nombre = data.Nombre ?? '';
    this.TotalCotizaciones = data.TotalCotizaciones ?? 0;
    this.TotalUnidades = data.TotalUnidades ?? 0;
    this.MontoTotal = data.MontoTotal ?? 0;
  }
}

export default ProductoCotizadoDto;
