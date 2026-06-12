class CotizacionResponseDto {
  constructor(data = {}) {
    this.IdDocumento = data.CIdDocumento ?? 0;
    this.IdCliente = data.CIdClienteProveedor ?? 0;
    this.IdAgente = data.CIdAgente ?? 0;
    this.SerieDocumento = data.CSerieDocumento ?? '';
    this.Folio = data.CFolio ?? 0;
    this.Fecha = data.CFecha ? new Date(data.CFecha) : null;
    this.RazonSocial = data.CRazonSocial ?? '';
    this.FechaVencimiento = data.CFechaVencimiento ? new Date(data.CFechaVencimiento) : null;
    this.FechaProntoPago = data.CFechaProntoPago ? new Date(data.CFechaProntoPago) : null;
    this.FechaEntregaRecepcion = data.CFechaEntregaRecepcion ? new Date(data.CFechaEntregaRecepcion) : null;
    this.Subtotal = data.CNeto ?? 0;
    this.IVA = data.CImpuesto1 ?? 0;
    this.Total = data.CTotal ?? 0;
    this.DescuentoDoc1 = data.CDescuentoDoc1 ?? 0;
    this.DescuentoDoc2 = data.CDescuentoDoc2 ?? 0;
    this.DescuentoDoc3 = data.CGasto1 ?? 0;
    this.Estado = data.CCancelado === 1 ? 'Cancelada' : 'Activa';
    this.Afectado = data.CAfectado === 1 ? 'Sí' : 'No';
    this.Impreso = data.CImpreso === 1 ? 'Sí' : 'No';
    this.Devuelto = data.CDevuelto === 1 ? 'Sí' : 'No';
    this.Observaciones = data.CObservaciones ?? '';
    this.Agente = data.NombreAgente ?? '';
    this.Facturado = data.Facturado ?? false;
    this.Movimientos = data.Movimientos ?? [];
  }
}

export default CotizacionResponseDto;
