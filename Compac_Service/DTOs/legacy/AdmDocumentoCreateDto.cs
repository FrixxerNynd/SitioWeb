using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para crear un nuevo documento en AdmDocumentos (POST)
    /// Incluye solo los campos requeridos para la inserción
    /// </summary>
    public class AdmDocumentoCreateDto
    {
        // ═══════════════════════════════════════════════════════════════
        // RELACIONES REQUERIDAS (FKs)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del modelo de documento (FK a admDocumentosModelo)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El ID del modelo de documento es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del modelo de documento debe ser mayor a 0")]
        public int IdDocumentoDe { get; set; }

        /// <summary>
        /// ID del concepto del documento (FK a admConceptos)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El ID del concepto es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del concepto debe ser mayor a 0")]
        public int IdConceptoDocumento { get; set; }

        /// <summary>
        /// ID del cliente o proveedor (FK a admClientes o admProveedores)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El ID del cliente/proveedor es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del cliente/proveedor debe ser mayor a 0")]
        public int IdClienteProveedor { get; set; }

        /// <summary>
        /// ID del agente (FK a admAgentes)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El ID del agente es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del agente debe ser mayor a 0")]
        public int IdAgente { get; set; }

        /// <summary>
        /// ID de la moneda (FK a admMonedas)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El ID de la moneda es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la moneda debe ser mayor a 0")]
        public int IdMoneda { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // DATOS DEL DOCUMENTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Serie del documento (ej: "A", "F1", "VEN")
        /// Máximo 11 caracteres, requerido
        /// </summary>
        [Required(ErrorMessage = "La serie del documento es requerida")]
        [StringLength(11, ErrorMessage = "La serie no puede exceder 11 caracteres")]
        public string SerieDocumento { get; set; } = string.Empty;

        /// <summary>
        /// Folio del documento (número consecutivo)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El folio es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El folio debe ser mayor o igual a 0")]
        public double Folio { get; set; }

        /// <summary>
        /// Fecha de vencimiento del documento
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Tipo de cambio de la moneda al momento del documento
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El tipo de cambio es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser mayor o igual a 0")]
        public double TipoCambio { get; set; }

        /// <summary>
        /// Referencia del documento (puede ser número externo, orden de compra, etc.)
        /// Máximo 20 caracteres, requerido
        /// </summary>
        [Required(ErrorMessage = "La referencia es requerida")]
        [StringLength(20, ErrorMessage = "La referencia no puede exceder 20 caracteres")]
        public string Referencia { get; set; } = string.Empty;

        /// <summary>
        /// Observaciones o notas del documento
        /// Opcional
        /// </summary>
        [StringLength(8000, ErrorMessage = "Las observaciones no pueden exceder 8000 caracteres")]
        public string? Observaciones { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // INDICADORES DEL DOCUMENTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Naturaleza del documento (1=Cargo, 2=Abono)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "La naturaleza del documento es requerida")]
        [Range(1, 2, ErrorMessage = "La naturaleza debe ser 1 (Cargo) o 2 (Abono)")]
        public int Naturaleza { get; set; }

        /// <summary>
        /// Indica si usa cliente (1) o proveedor (0)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El indicador de uso de cliente es requerido")]
        [Range(0, 1, ErrorMessage = "UsaCliente debe ser 0 o 1")]
        public int UsaCliente { get; set; }

        /// <summary>
        /// Indica si el documento está afectado (1) o no (0)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El indicador de afectación es requerido")]
        [Range(0, 1, ErrorMessage = "Afectado debe ser 0 o 1")]
        public int Afectado { get; set; }

        /// <summary>
        /// Indica si el documento está impreso (1) o no (0)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El indicador de impresión es requerido")]
        [Range(0, 1, ErrorMessage = "Impreso debe ser 0 o 1")]
        public int Impreso { get; set; }

        /// <summary>
        /// Indica si el documento está cancelado (1) o no (0)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El indicador de cancelación es requerido")]
        [Range(0, 1, ErrorMessage = "Cancelado debe ser 0 o 1")]
        public int Cancelado { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // IMPORTES Y TOTALES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Subtotal (neto) del documento antes de impuestos
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El neto es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El neto debe ser mayor o igual a 0")]
        public double Neto { get; set; }

        /// <summary>
        /// Impuesto 1 aplicado (IVA generalmente)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El impuesto 1 es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El impuesto 1 debe ser mayor o igual a 0")]
        public double Impuesto1 { get; set; }

        /// <summary>
        /// Descuento a nivel movimiento
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El descuento de movimiento es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
        public double DescuentoMov { get; set; }

        /// <summary>
        /// Total del documento (Neto + Impuestos - Descuentos)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El total es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a 0")]
        public double Total { get; set; }

        /// <summary>
        /// Saldo pendiente del documento
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El pendiente es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El pendiente debe ser mayor o igual a 0")]
        public double Pendiente { get; set; }

        /// <summary>
        /// Total de unidades (cantidad de productos/servicios)
        /// Requerido
        /// </summary>
        [Required(ErrorMessage = "El total de unidades es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El total de unidades debe ser mayor o igual a 0")]
        public double TotalUnidades { get; set; }
    }
}
