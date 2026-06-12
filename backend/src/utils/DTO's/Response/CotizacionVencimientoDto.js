class CotizacionVencimientoDto {
  constructor(data = {}) {
    this.IdDocumento = data.CIdDocumento ?? 0;
    this.Serie = data.CSerieDocumento ?? '';
    this.Folio = data.CFolio ?? 0;
    this.RazonSocial = data.CRazonSocial ?? '';
    this.FechaVencimiento = data.CFechaVencimiento ? new Date(data.CFechaVencimiento) : null;
    this.Total = data.CTotal ?? 0;
    this.DiasRestantes = data.DiasRestantes ?? 0;
  }
}

export default CotizacionVencimientoDto;
