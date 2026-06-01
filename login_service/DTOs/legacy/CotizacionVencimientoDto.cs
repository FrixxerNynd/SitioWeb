namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para cotizaciones próximas a vencer
    /// Muestra alertas de cotizaciones que requieren atención
    /// </summary>
    public class CotizacionVencimientoDto
    {
        /// <summary>
        /// ID del documento
        /// </summary>
        public int IdDocumento { get; set; }

        /// <summary>
        /// Serie del documento
        /// </summary>
        public string SerieDocumento { get; set; } = string.Empty;

        /// <summary>
        /// Folio del documento
        /// </summary>
        public double Folio { get; set; }

        /// <summary>
        /// Razón social del cliente
        /// </summary>
        public string RazonSocial { get; set; } = string.Empty;

        /// <summary>
        /// Monto total de la cotización
        /// </summary>
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Fecha de vencimiento
        /// </summary>
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Días restantes hasta el vencimiento
        /// </summary>
        public int DiasRestantes { get; set; }

        /// <summary>
        /// Nivel de urgencia (Critico, Alto, Medio, Bajo)
        /// </summary>
        public string NivelUrgencia { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de creación de la cotización
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Estatus de la cotización (Activa/Cancelada)
        /// </summary>
        public string Estatus { get; set; } = string.Empty;
    }
}
