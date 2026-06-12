class CotizacionCancelarDto {
  constructor(data = {}) {
    this.IdDocumento = data.IdDocumento ?? 0;
    this.Motivo = data.Motivo ?? '';
    this.UsuarioCancela = data.UsuarioCancela ?? 'SISTEMA';
  }
}

export default CotizacionCancelarDto;
