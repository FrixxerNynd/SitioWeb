class CotizacionPorRangoDto {
  constructor(data = {}) {
    this.Rango = data.Rango ?? '';
    this.Total = data.Total ?? 0;
    this.Monto = data.Monto ?? 0;
  }
}

export default CotizacionPorRangoDto;
