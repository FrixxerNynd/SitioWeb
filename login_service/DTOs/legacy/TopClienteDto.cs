namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para análisis de top clientes por cotizaciones
    /// Muestra ranking de clientes más activos y rentables
    /// </summary>
    public class TopClienteDto
    {
        /// <summary>
        /// ID del cliente en el sistema legacy
        /// </summary>
        public int IdCliente { get; set; }

        /// <summary>
        /// Código del cliente
        /// </summary>
        public string CodigoCliente { get; set; } = string.Empty;

        /// <summary>
        /// Razón social del cliente
        /// </summary>
        public string RazonSocial { get; set; } = string.Empty;

        /// <summary>
        /// RFC del cliente
        /// </summary>
        public string Rfc { get; set; } = string.Empty;

        /// <summary>
        /// Número total de cotizaciones del cliente
        /// </summary>
        public int TotalCotizaciones { get; set; }

        /// <summary>
        /// Suma total de montos de cotizaciones del cliente
        /// </summary>
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Promedio de monto por cotización del cliente
        /// </summary>
        public decimal MontoPromedio { get; set; }

        /// <summary>
        /// Cotizaciones activas del cliente
        /// </summary>
        public int CotizacionesActivas { get; set; }

        /// <summary>
        /// Fecha de última cotización
        /// </summary>
        public DateTime? UltimaCotizacion { get; set; }

        /// <summary>
        /// Posición en el ranking
        /// </summary>
        public int Ranking { get; set; }
    }
}
