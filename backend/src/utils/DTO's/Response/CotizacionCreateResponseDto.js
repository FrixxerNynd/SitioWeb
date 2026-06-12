class CotizacionCreateResponseDto {
  constructor(data = {}) {
    this.IdDocumento = data.IdDocumento ?? 0;
    this.Serie = data.Serie ?? 'CA';
    this.Folio = data.Folio ?? 0;
    this.Fecha = data.Fecha ? new Date(data.Fecha) : new Date();
    this.RazonSocial = data.RazonSocial ?? '';
    this.Total = data.Total ?? 0;
    this.Pendiente = data.Pendiente ?? 0;
    this.Neto = data.Neto ?? 0;
    this.Impuesto = data.Impuesto ?? 0;
    this.CantidadProductos = data.CantidadProductos ?? 0;
    this.Mensaje = data.Mensaje ?? '';
  }
}

export default CotizacionCreateResponseDto;
