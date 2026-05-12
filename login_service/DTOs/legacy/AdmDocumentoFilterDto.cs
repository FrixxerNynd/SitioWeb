namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para filtros de búsqueda de documentos
    /// </summary>
    public class AdmDocumentoFilterDto
    {
        /// <summary>
        /// Fecha inicial (inclusiva)
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Fecha final (inclusiva)
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Folio del documento
        /// </summary>
        public string? Folio { get; set; }

        /// <summary>
        /// Serie del documento
        /// </summary>
        public string? SerieDocumento { get; set; }

        /// <summary>
        /// Razón social del cliente (búsqueda parcial)
        /// </summary>
        public string? RazonSocial { get; set; }

        /// <summary>
        /// Fecha de vencimiento inicial (inclusiva)
        /// </summary>
        public DateTime? FechaVencimientoInicio { get; set; }

        /// <summary>
        /// Fecha de vencimiento final (inclusiva)
        /// </summary>
        public DateTime? FechaVencimientoFin { get; set; }

        /// <summary>
        /// ID del concepto (opcional)
        /// </summary>
        public int? IdConcepto { get; set; }

        /// <summary>
        /// ID del agente (opcional)
        /// </summary>
        public int? IdAgente { get; set; }

        /// <summary>
        /// ID del documento de origen (opcional)
        /// </summary>
        public int? IdDocumentoDe { get; set; }

        /// <summary>
        /// Número de página (default: 1)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Registros por página (default: 50, max: 100)
        /// </summary>
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// Incluir movimientos en la respuesta
        /// </summary>
        public bool IncluirMovimientos { get; set; } = false;
    }
}
