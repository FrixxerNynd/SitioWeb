namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de respuesta simplificado para AdmDocumentos (Cotizaciones/Documentos)
    /// Solo campos esenciales: Serie, Folio, Fecha, RazonSocial, FechaVencimiento, Agente, FechaProntoPago, FechaEntregaRecepcion, Productos
    /// </summary>
    public class AdmDocumentoResponseDto
    {
        //ID necesarios
        public int IdDocumento { get; set; }
        public int IdCliente { get; set; }
        public int IdAgente { get; set; }
        public string SerieDocumento { get; set; } = string.Empty;
        public double Folio { get; set; }
        public DateTime Fecha { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaProntoPago { get; set; }
        public DateTime FechaEntregaRecepcion { get; set; }

        // Totales
        public double Subtotal { get; set; }
        public double IVA { get; set; }
        public double Total { get; set; }

        // Descuentos
        public double DescuentoDoc1 { get; set; }
        public double DescuentoDoc2 { get; set; }
        public double DescuentoDoc3 { get; set; }

        // Estado
        public string Estado { get; set; } = "Activa";
        public string Afectado { get; set; } = "Si";
        public string Impreso { get; set; } = "Si";
        public string Devuelto { get; set; } = "No";

        // Agente de ventas (nombre completo)
        public string? Agente { get; set; }

        // Detalle de movimientos (productos cotizados)
        public List<AdmMovimientoResponseDto> Movimientos { get; set; } = new();

        public string? Observaciones { get; set; } = string.Empty;
        public bool Facturado { get; set; }
    }
}
