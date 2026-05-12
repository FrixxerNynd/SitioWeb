namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de respuesta para unidades de medida y peso
    /// </summary>
    public class AdmUnidadMedidaPesoResponseDto
    {
        /// <summary>
        /// ID de la unidad
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre completo de la unidad
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Abreviatura (ej: KG, PZA, LT)
        /// </summary>
        public string Abreviatura { get; set; } = string.Empty;

        /// <summary>
        /// Clave de despliegue
        /// </summary>
        public string Despliegue { get; set; } = string.Empty;

        /// <summary>
        /// Clave interna del sistema
        /// </summary>
        public string ClaveInterna { get; set; } = string.Empty;

        /// <summary>
        /// Clave SAT para facturación electrónica
        /// </summary>
        public string ClaveSat { get; set; } = string.Empty;
    }
}
