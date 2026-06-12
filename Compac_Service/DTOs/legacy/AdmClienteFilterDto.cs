namespace back_cabs.CRM.DTOs.Legacy
{
    /// <summary>
    /// DTO de filtros para búsqueda de clientes
    /// </summary>
    public class AdmClienteFilterDto
    {
        /// <summary>
        /// Código del cliente (búsqueda parcial)
        /// </summary>
        public string? CodigoCliente { get; set; }

        /// <summary>
        /// Razón social (búsqueda parcial)
        /// </summary>
        public string? RazonSocial { get; set; }

        /// <summary>
        /// RFC (búsqueda parcial)
        /// </summary>
        public string? RFC { get; set; }

        /// <summary>
        /// Email (búsqueda parcial)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Teléfono (búsqueda parcial)
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Estado (búsqueda parcial en domicilio)
        /// </summary>
        public string? Estado { get; set; }

        /// <summary>
        /// Ciudad (búsqueda parcial en domicilio)
        /// </summary>
        public string? Ciudad { get; set; }

        /// <summary>
        /// Estatus del cliente (1 = Activo, 0 = Inactivo, null = Todos)
        /// </summary>
        public int? Estatus { get; set; } = 1; // Por defecto solo activos

        /// <summary>
        /// Tipo de dirección (0 = Predeterminado, 1 = Fiscal, 2 = Envío, null = Todas)
        /// </summary>
        public int? TipoDireccion { get; set; } = null; // Por defecto todas

        /// <summary>
        /// Incluir detalle completo de ubicación
        /// </summary>
        public bool IncluirDetalleUbicacion { get; set; } = true;

        /// <summary>
        /// Número de página (inicia en 1)
        /// </summary>
        public int NumeroPagina { get; set; } = 1;

        /// <summary>
        /// Tamaño de página (máximo 100)
        /// </summary>
        public int TamanoPagina { get; set; } = 50;
    }
}
