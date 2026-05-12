namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para estadísticas generales del dashboard de cotizaciones
    /// Contiene métricas clave para análisis ejecutivo
    /// </summary>
    public class EstadisticasGeneralesDto
    {
        /// <summary>
        /// Total de cotizaciones en el período
        /// </summary>
        public int TotalCotizaciones { get; set; }

        /// <summary>
        /// Suma total de montos de todas las cotizaciones
        /// </summary>
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Promedio de monto por cotización
        /// </summary>
        public decimal MontoPromedio { get; set; }

        /// <summary>
        /// Cotización con monto más alto
        /// </summary>
        public decimal MontoMaximo { get; set; }

        /// <summary>
        /// Cotización con monto más bajo
        /// </summary>
        public decimal MontoMinimo { get; set; }

        /// <summary>
        /// Total de cotizaciones activas
        /// </summary>
        public int CotizacionesActivas { get; set; }

        /// <summary>
        /// Total de cotizaciones canceladas
        /// </summary>
        public int CotizacionesCanceladas { get; set; }

        /// <summary>
        /// Número de clientes únicos con cotizaciones
        /// </summary>
        public int ClientesUnicos { get; set; }

        /// <summary>
        /// Número de productos únicos cotizados
        /// </summary>
        public int ProductosUnicos { get; set; }

        /// <summary>
        /// Período de análisis - fecha inicial
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Período de análisis - fecha final
        /// </summary>
        public DateTime? FechaFin { get; set; }
    }
}
