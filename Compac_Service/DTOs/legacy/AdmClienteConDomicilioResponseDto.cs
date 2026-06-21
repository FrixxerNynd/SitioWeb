namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de respuesta para clientes con domicilio
    /// </summary>
    public class AdmClienteConDomicilioResponseDto
    {
        public int Id { get; set; }
        public string CodigoCliente { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string RFC { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Email2 { get; set; } = string.Empty;
        public string Email3 { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public UbicacionDetalleDto? UbicacionDetalle { get; set; }
    }

    /// <summary>
    /// Detalle completo de ubicación
    /// </summary>
    public class UbicacionDetalleDto
    {
        public string Calle { get; set; } = string.Empty;
        public string NumeroExterior { get; set; } = string.Empty;
        public string NumeroInterior { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Telefono1 { get; set; } = string.Empty;
        public string Telefono2 { get; set; } = string.Empty;
        public string TelefonoCompleto { get; set; } = string.Empty;
    }

    public class CreditClientDto
    {
        public double LimiteCredito { get; set; } = 0;
        public int LimiteDocs { get; set; }
        public int DiasCredito { get; set; }
        public int ExcederCredito {get; set; }
    }
}
