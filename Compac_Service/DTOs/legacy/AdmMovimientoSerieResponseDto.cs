namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de respuesta para movimientos serie
    /// Relación entre movimientos y números de serie
    /// </summary>
    public class AdmMovimientoSerieResponseDto
    {
        /// <summary>
        /// ID autoincrementable del registro
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del movimiento asociado
        /// </summary>
        public int IdMovimiento { get; set; }

        /// <summary>
        /// ID del número de serie asociado
        /// </summary>
        public int IdSerie { get; set; }

        /// <summary>
        /// Fecha del movimiento
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Número de serie completo (si está disponible)
        /// </summary>
        public string? NumeroSerie { get; set; }

        /// <summary>
        /// Pedimento relacionado al número de serie
        /// </summary>
        public string? Pedimento { get; set; }

        /// <summary>
        /// Aduana relacionada
        /// </summary>
        public string? Aduana { get; set; }

        /// <summary>
        /// Lote del número de serie
        /// </summary>
        public string? Lote { get; set; }
    }
}
