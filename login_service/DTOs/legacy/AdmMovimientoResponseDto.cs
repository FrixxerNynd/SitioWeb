namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de respuesta para AdmMovimientos (Líneas de detalle de documentos)
    /// </summary>
    public class AdmMovimientoResponseDto
    {
        public int IdMovimiento { get; set; }
        public double NumeroMovimiento { get; set; }
        
        // Información del producto
        public int IdProducto { get; set; }
        public string? CodigoProducto { get; set; }
        public string? NombreProducto { get; set; }
        public string? DescripcionProducto { get; set; }
        
        // Almacén
        public int IdAlmacen { get; set; }
        public string? CodigoAlmacen { get; set; }
        public string? NombreAlmacen { get; set; }
        
        // Unidades
        public double Unidades { get; set; }
        public double UnidadesCapturadas { get; set; }
        public int IdUnidad { get; set; }
        public string? NombreUnidad { get; set; }
        
        // Precios y costos
        public double Precio { get; set; }
        public double PrecioCapturado { get; set; }
        public double CostoCapturado { get; set; }
        public double PorcentajeDescuento { get; set; }
        public double DescuentoLinea { get; set; }
        
        // Impuestos
        public double Impuesto1 { get; set; }
        public double Impuesto2 { get; set; }
        public double Impuesto3 { get; set; }
        public double Retencion1 { get; set; }
        public double Retencion2 { get; set; }
        
        // Totales
        public double Neto { get; set; }
        public double Total { get; set; }
        
        // Referencia y observaciones
        public string Referencia { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        
        // Estado
        public int Afectado { get; set; }
        public int Venta { get; set; }
    }
}
