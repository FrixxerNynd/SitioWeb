using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO simplificado para crear una nueva cotización
    /// Facilita al usuario la creación con valores por defecto automáticos
    /// </summary>
    public class AdmCotizacionCreateDto
    {
        /// <summary>
        /// ID del cliente (obligatorio)
        /// Se obtiene previamente mediante búsqueda por razón social o RFC
        /// </summary>
        [Required(ErrorMessage = "El ID del cliente es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del cliente debe ser mayor a 0")]
        public int IdCliente { get; set; }

        /// <summary>
        /// Razón Social personalizada (opcional)
        /// Usado principalmente para Publico en General (ID 832)
        /// </summary>
        public string? RazonSocial { get; set; }

        /// <summary>
        /// ID del agente de ventas (opcional)
        /// Si no se envía, se usará un agente por defecto
        /// </summary>
        public int? IdAgente { get; set; }

        /// <summary>
        /// Fecha de vencimiento de la cotización (opcional)
        /// Si no se envía, se calcula automáticamente (30 días después de la fecha actual)
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>
        /// Fecha de pronto pago (opcional)
        /// Si no se envía, se asigna la misma fecha de vencimiento
        /// </summary>
        public DateTime? FechaProntoPago { get; set; }

        /// <summary>
        /// Fecha de entrega/recepción (opcional)
        /// Si no se envía, se asigna la fecha actual
        /// </summary>
        public DateTime? FechaEntregaRecepcion { get; set; }

        /// <summary>
        /// Productos a cotizar (obligatorio - al menos 1 producto)
        /// </summary>
        [Required(ErrorMessage = "Debe incluir al menos un producto en la cotización")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<CotizacionMovimientoDto> Productos { get; set; } = new();

        /// <summary>
        /// Descuento a nivel documento 1 (opcional)
        /// Importe de descuento aplicado al total (Valor monetario, NO porcentaje)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
        public double? DescuentoDoc1 { get; set; }

        /// <summary>
        /// Descuento a nivel documento 2 (opcional)
        /// Importe de descuento adicional aplicado al total (Valor monetario, NO porcentaje)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
        public double? DescuentoDoc2 { get; set; }

        /// <summary>
        /// Descuento a nivel documento 3 (opcional)
        /// Tercer descuento adicional aplicado al total
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
        public double? DescuentoDoc3 { get; set; }

        /// <summary>
        /// CTOTAL - Monto total final de la cotización (OBLIGATORIO)
        /// Este es el valor que el usuario proporciona manualmente.
        /// El sistema NO lo calcula automáticamente.
        /// Es el monto neto que el cliente pagará.
        /// </summary>
        [Required(ErrorMessage = "El CTOTAL es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El CTOTAL debe ser mayor a 0")]
        public double CTotal { get; set; }

        /// <summary>
        /// Monto que el cliente pagó o abonó (opcional)
        /// Si no se envía o es 0, CPendiente = CTotal
        /// Si se envía, CPendiente = CTotal - MontoPagado
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El monto pagado no puede ser negativo")]
        public double? MontoPagado { get; set; }

        /// <summary>
        /// Observaciones adicionales (opcional)
        /// </summary>
        [StringLength(254, ErrorMessage = "Las observaciones no pueden exceder 254 caracteres")]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Referencia del documento (opcional)
        /// Si no se envía, se genera automáticamente
        /// </summary>
        [StringLength(20, ErrorMessage = "La referencia no puede exceder 20 caracteres")]
        public string? Referencia { get; set; }

        /// <summary>
        /// Indica si se debe aplicar IVA a los productos (opcional)
        /// Por defecto es true
        /// </summary>
        public bool AplicarIVA { get; set; } = true;

        /// <summary>
        /// Tipo de cambio de la moneda (opcional)
        /// Por defecto es 1
        /// </summary>
        public double TipoCambio { get; set; } = 1.0;

        /// <summary>
        /// Porcentaje de IVA a aplicar (opcional)
        /// Por defecto es 16%
        /// </summary>
        [Range(0, 100, ErrorMessage = "El porcentaje de IVA debe estar entre 0 y 100")]
        public double PorcentajeIVA { get; set; } = 16.0;
    }

    /// <summary>
    /// DTO para los productos (movimientos) de la cotización
    /// </summary>
    public class CotizacionMovimientoDto
    {
        /// <summary>
        /// ID del producto (obligatorio)
        /// FK a admProductos
        /// </summary>
        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor a 0")]
        public int IdProducto { get; set; }

        /// <summary>
        /// ID del almacén (obligatorio)
        /// FK a admAlmacenes
        /// </summary>
        [Required(ErrorMessage = "El ID del almacén es obligatorio")]
        // [Range(1, int.MaxValue, ErrorMessage = "El ID del almacén debe ser mayor a 0")]
        public int IdAlmacen { get; set; }

        /// <summary>
        /// ID del movimiento (opcional)
        /// Solo necesario para edición. Si es null en edición, se considera nuevo.
        /// </summary>
        public int? IdMovimiento { get; set; }

        /// <summary>
        /// ID de la unidad de medida (opcional)
        /// Si no se envía, se usa la unidad por defecto del producto
        /// </summary>
        public int? IdUnidad { get; set; }

        /// <summary>
        /// Cantidad de unidades (obligatorio)
        /// Nota: Se permite 0 porque lo importante es el CTOTAL del documento
        /// </summary>
        [Required(ErrorMessage = "La cantidad de unidades es obligatoria")]
        [Range(0, double.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
        public double Unidades { get; set; }

        /// <summary>
        /// Precio unitario (obligatorio)
        /// El usuario puede modificar el precio que viene del producto
        /// </summary>
        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
        public double Precio { get; set; }

        /// <summary>
        /// Porcentaje de descuento a nivel movimiento (opcional)
        /// Descuento individual del producto
        /// </summary>
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public double? PorcentajeDescuento { get; set; }

        /// <summary>
        /// Importe de descuento a nivel movimiento (opcional)
        /// Descuento en valores absolutos ($) aplicado al producto
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El descuento por importe no puede ser negativo")]
        public double? DescuentoImporte { get; set; }

        /// <summary>
        /// Observaciones del movimiento (opcional)
        /// </summary>
        public string? Observaciones { get; set; }
    }
}
