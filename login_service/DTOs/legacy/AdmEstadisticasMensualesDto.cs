namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO para estadísticas mensuales de cotizaciones
    /// </summary>
    public class AdmEstadisticasMensualesDto
    {
        public int Mes { get; set; } // 1-12
        public string NombreMes { get; set; } = string.Empty;
        public int CotizacionesActivas { get; set; }
        public int CotizacionesCanceladas { get; set; }
        public decimal MontoTotal { get; set; }
    }
}
