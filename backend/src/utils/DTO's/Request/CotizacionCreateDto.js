class CotizacionMovimientoDto {
  constructor(data = {}) {
    this.IdProducto = data.IdProducto ?? 0;
    this.IdAlmacen = data.IdAlmacen ?? 1;
    this.IdUnidad = data.IdUnidad ?? null;
    this.Unidades = data.Unidades ?? 1;
    this.Precio = data.Precio ?? 0;
    this.PorcentajeDescuento = data.PorcentajeDescuento ?? null;
    this.DescuentoImporte = data.DescuentoImporte ?? null;
    this.Observaciones = data.Observaciones ?? '';
  }
}

class CotizacionCreateDto {
  constructor(data = {}) {
    this.IdCliente = data.IdCliente ?? 0;
    this.RazonSocial = data.RazonSocial ?? null;
    this.IdAgente = data.IdAgente ?? null;
    this.FechaVencimiento = data.FechaVencimiento ?? null;
    this.FechaProntoPago = data.FechaProntoPago ?? null;
    this.FechaEntregaRecepcion = data.FechaEntregaRecepcion ?? null;
    this.Productos = (data.Productos ?? []).map(p => new CotizacionMovimientoDto(p));
    this.DescuentoDoc1 = data.DescuentoDoc1 ?? null;
    this.DescuentoDoc2 = data.DescuentoDoc2 ?? null;
    this.DescuentoDoc3 = data.DescuentoDoc3 ?? null;
    this.CTotal = data.CTotal ?? 0;
    this.MontoPagado = data.MontoPagado ?? null;
    this.Observaciones = data.Observaciones ?? '';
    this.Referencia = data.Referencia ?? '';
    this.AplicarIVA = data.AplicarIVA ?? true;
    this.TipoCambio = data.TipoCambio ?? 1.0;
    this.PorcentajeIVA = data.PorcentajeIVA ?? 16.0;
  }
}

export { CotizacionCreateDto, CotizacionMovimientoDto };
