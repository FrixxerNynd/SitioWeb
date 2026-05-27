namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para análisis de rendimiento por agente de ventas
    /// Muestra métricas de desempeño de vendedores
    /// </summary>
    public class RendimientoAgenteDto
    {
        /// <summary>
        /// ID del agente en el sistema legacy
        /// </summary>
        public int IdAgente { get; set; }

        /// <summary>
        /// Código del agente
        /// </summary>
        public string CodigoAgente { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del agente
        /// </summary>
        public string NombreAgente { get; set; } = string.Empty;

        /// <summary>
        /// Número total de cotizaciones del agente
        /// </summary>
        public int TotalCotizaciones { get; set; }

        /// <summary>
        /// Monto total de cotizaciones del agente
        /// </summary>
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Promedio de monto por cotización
        /// </summary>
        public decimal MontoPromedio { get; set; }

        /// <summary>
        /// Número de cotizaciones activas
        /// </summary>
        public int CotizacionesActivas { get; set; }

        /// <summary>
        /// Número de cotizaciones canceladas
        /// </summary>
        public int CotizacionesCanceladas { get; set; }

        /// <summary>
        /// Tasa de conversión (cotizaciones activas / total)
        /// </summary>
        public decimal TasaConversion { get; set; }

        /// <summary>
        /// Fecha de la última cotización del agente
        /// </summary>
        public DateTime? UltimaCotizacion { get; set; }

        /// <summary>
        /// Número de clientes únicos atendidos
        /// </summary>
        public int ClientesUnicos { get; set; }

        /// <summary>
        /// Posición en el ranking de rendimiento
        /// </summary>
        public int Ranking { get; set; }
    }
}
