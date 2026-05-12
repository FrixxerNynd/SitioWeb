namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para análisis de cotizaciones por rango de monto
    /// Agrupa cotizaciones en rangos de valor para análisis de distribución
    /// </summary>
    public class CotizacionPorRangoDto
    {
        /// <summary>
        /// Rango de monto (ej: "0-1000", "1000-5000", "5000-10000", "10000+")
        /// </summary>
        public string RangoMonto { get; set; } = string.Empty;

        /// <summary>
        /// Monto mínimo del rango
        /// </summary>
        public decimal MontoMinimo { get; set; }

        /// <summary>
        /// Monto máximo del rango (null para rangos abiertos)
        /// </summary>
        public decimal? MontoMaximo { get; set; }

        /// <summary>
        /// Número total de cotizaciones en este rango
        /// </summary>
        public int TotalCotizaciones { get; set; }

        /// <summary>
        /// Monto total acumulado de todas las cotizaciones en el rango
        /// </summary>
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Monto promedio de las cotizaciones en el rango
        /// </summary>
        public decimal MontoPromedio { get; set; }

        /// <summary>
        /// Número de cotizaciones activas en el rango
        /// </summary>
        public int CotizacionesActivas { get; set; }

        /// <summary>
        /// Número de cotizaciones canceladas en el rango
        /// </summary>
        public int CotizacionesCanceladas { get; set; }

        /// <summary>
        /// Porcentaje que representa este rango del total
        /// </summary>
        public decimal PorcentajeDelTotal { get; set; }
    }
}