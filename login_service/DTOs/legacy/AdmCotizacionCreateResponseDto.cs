namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de respuesta al crear una cotización exitosamente
    /// </summary>
    public class AdmCotizacionCreateResponseDto
    {
        /// <summary>
        /// ID del documento creado
        /// </summary>
        public int IdDocumento { get; set; }

        /// <summary>
        /// Serie del documento (ej: "CA")
        /// </summary>
        public string Serie { get; set; } = string.Empty;

        /// <summary>
        /// Folio asignado automáticamente
        /// </summary>
        public double Folio { get; set; }

        /// <summary>
        /// Fecha del documento
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Razón social del cliente
        /// </summary>
        public string RazonSocial { get; set; } = string.Empty;

        /// <summary>
        /// Total de la cotización
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Saldo pendiente
        /// </summary>
        public double Pendiente { get; set; }

        /// <summary>
        /// Subtotal (neto sin impuestos)
        /// </summary>
        public double Neto { get; set; }

        /// <summary>
        /// Impuesto aplicado (IVA)
        /// </summary>
        public double Impuesto { get; set; }

        /// <summary>
        /// Cantidad de productos cotizados
        /// </summary>
        public int CantidadProductos { get; set; }

        /// <summary>
        /// Mensaje informativo
        /// </summary>
        public string Mensaje { get; set; } = "Cotización creada exitosamente";
    }
}
