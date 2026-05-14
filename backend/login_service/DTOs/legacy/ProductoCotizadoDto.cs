namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para análisis de productos más cotizados
    /// Muestra ranking de productos por frecuencia y volumen de cotizaciones
    /// </summary>
    public class ProductoCotizadoDto
    {
        /// <summary>
        /// ID del producto en el sistema legacy
        /// </summary>
        public int IdProducto { get; set; }

        /// <summary>
        /// Código del producto
        /// </summary>
        public string CodigoProducto { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// Número total de cotizaciones que incluyen este producto
        /// </summary>
        public int TotalCotizaciones { get; set; }

        /// <summary>
        /// Cantidad total cotizada del producto
        /// </summary>
        public decimal CantidadTotal { get; set; }

        /// <summary>
        /// Monto total cotizado del producto
        /// </summary>
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Precio promedio por unidad
        /// </summary>
        public decimal PrecioPromedio { get; set; }

        /// <summary>
        /// Número de clientes únicos que han cotizado este producto
        /// </summary>
        public int ClientesUnicos { get; set; }

        /// <summary>
        /// Posición en el ranking de productos más cotizados
        /// </summary>
        public int Ranking { get; set; }
    }
}