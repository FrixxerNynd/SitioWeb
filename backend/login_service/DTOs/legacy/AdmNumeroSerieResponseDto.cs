namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de respuesta para AdmNumerosSerie
    /// </summary>
    public class AdmNumeroSerieResponseDto
    {
        public int IdSerie { get; set; }
        public int IdProducto { get; set; }
        public string NumeroSerie { get; set; } = string.Empty;
        public int IdAlmacen { get; set; }
        public int Estado { get; set; }
        public int EstadoAnterior { get; set; }
        public string NumeroLote { get; set; } = string.Empty;
        public DateTime FechaCaducidad { get; set; }
        public DateTime FechaFabricacion { get; set; }
        public string Pedimento { get; set; } = string.Empty;
        public string Aduana { get; set; } = string.Empty;
        public DateTime FechaPedimento { get; set; }
        public double TipoCambio { get; set; }
        public double Costo { get; set; }
        public string Timestamp { get; set; } = string.Empty;
        public int NumAduana { get; set; }
        public string ClaveSat { get; set; } = string.Empty;

        // Información relacionada
        public string? CodigoProducto { get; set; }
        public string? NombreProducto { get; set; }
        public string? CodigoAlmacen { get; set; }
        public string? NombreAlmacen { get; set; }
    }
}
