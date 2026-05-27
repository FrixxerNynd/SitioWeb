using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para actualización (edición) de Cotizaciones.
    /// Permite valores nulos/opcionales para indicar "no cambiar".
    /// </summary>
    public class AdmCotizacionUpdateDto
    {
        [Required]
        public int IdDocumento { get; set; }
        public int? IdCliente { get; set; }
        public string? RazonSocial { get; set; }
        public int? IdAgente { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaProntoPago { get; set; }
        public DateTime? FechaEntregaRecepcion { get; set; }

        public List<CotizacionMovimientoUpdateDto>? Productos { get; set; }

        public double? DescuentoDoc1 { get; set; }
        public double? DescuentoDoc2 { get; set; }
        public double? DescuentoDoc3 { get; set; }

        // El CTotal NO debería ser obligatorio si se recalcula, 
        // pero lo dejamos opcional por si el front quiere enviarlo.
        public double? CTotal { get; set; }
        public double? MontoPagado { get; set; }
        public string? Observaciones { get; set; }
        public double? TipoCambio { get; set; }
    }

    public class CotizacionMovimientoUpdateDto
    {
        // ID del movimiento original. Si es null/0, es NUEVO.
        public int? IdMovimiento { get; set; }
        // Si es 0/null, se mantiene el original (si IdMovimiento existe)
        public int? IdProducto { get; set; }
        public int? IdAlmacen { get; set; }
        public int? IdUnidad { get; set; }
        // Cantidades monetarias opcionales
        public double? Unidades { get; set; }
        public double? Precio { get; set; }
        public double? PorcentajeDescuento { get; set; }
        public double? DescuentoImporte { get; set; }
        public string? Observaciones { get; set; }
    }
}
