class CotizacionMovimientoUpdateDto {
  constructor(data = {}) {
    this.IdMovimiento = data.IdMovimiento ?? null;
    this.IdProducto = data.IdProducto ?? null;
    this.IdAlmacen = data.IdAlmacen ?? null;
    this.IdUnidad = data.IdUnidad ?? null;
    this.Unidades = data.Unidades ?? null;
    this.Precio = data.Precio ?? null;
    this.PorcentajeDescuento = data.PorcentajeDescuento ?? null;
    this.DescuentoImporte = data.DescuentoImporte ?? null;
    this.Observaciones = data.Observaciones ?? null;
  }
}

class CotizacionUpdateDto {
  constructor(data = {}) {
    this.IdDocumento = data.IdDocumento ?? 0;
    this.IdCliente = data.IdCliente ?? null;
    this.RazonSocial = data.RazonSocial ?? null;
    this.IdAgente = data.IdAgente ?? null;
    this.FechaVencimiento = data.FechaVencimiento ?? null;
    this.FechaProntoPago = data.FechaProntoPago ?? null;
    this.FechaEntregaRecepcion = data.FechaEntregaRecepcion ?? null;
    this.Productos = (data.Productos ?? []).map(p => new CotizacionMovimientoUpdateDto(p));
    this.DescuentoDoc1 = data.DescuentoDoc1 ?? null;
    this.DescuentoDoc2 = data.DescuentoDoc2 ?? null;
    this.DescuentoDoc3 = data.DescuentoDoc3 ?? null;
    this.MontoPagado = data.MontoPagado ?? null;
    this.Observaciones = data.Observaciones ?? null;
    this.TipoCambio = data.TipoCambio ?? null;
  }
}

export { CotizacionUpdateDto, CotizacionMovimientoUpdateDto };
