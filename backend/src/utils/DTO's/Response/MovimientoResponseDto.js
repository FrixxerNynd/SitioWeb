class MovimientoResponseDto {
  constructor(data = {}) {
    this.IdMovimiento = data.CIdMovimiento ?? 0;
    this.NumeroMovimiento = data.CNumeroMovimiento ?? 0;
    this.IdProducto = data.CIdProducto ?? 0;
    this.CodigoProducto = data.CodigoProducto ?? '';
    this.NombreProducto = data.NombreProducto ?? '';
    this.DescripcionProducto = data.DescripcionProducto ?? '';
    this.IdAlmacen = data.CIdAlmacen ?? 0;
    this.CodigoAlmacen = data.CodigoAlmacen ?? '';
    this.NombreAlmacen = data.NombreAlmacen ?? '';
    this.Unidades = data.CUnidades ?? 0;
    this.UnidadesCapturadas = data.CUnidadesCapturadas ?? 0;
    this.IdUnidad = data.CIdUnidad ?? 0;
    this.Precio = data.CPrecio ?? 0;
    this.PrecioCapturado = data.CPrecioCapturado ?? 0;
    this.CostoCapturado = data.CCostoCapturado ?? 0;
    this.PorcentajeDescuento = data.CPorcentajeDescuento1 ?? 0;
    this.DescuentoLinea = data.CDescuento1 ?? 0;
    this.Impuesto1 = data.CImpuesto1 ?? 0;
    this.Impuesto2 = data.CImpuesto2 ?? 0;
    this.Impuesto3 = data.CImpuesto3 ?? 0;
    this.Retencion1 = data.CRetencion1 ?? 0;
    this.Retencion2 = data.CRetencion2 ?? 0;
    this.Neto = data.CNeto ?? 0;
    this.Total = data.CTotal ?? 0;
    this.Referencia = data.CReferencia ?? '';
    this.Observaciones = data.CObservaMov ?? '';
    this.Afectado = data.CAfectaExistencia ?? 0;
    this.Venta = 0;
  }
}

export default MovimientoResponseDto;
