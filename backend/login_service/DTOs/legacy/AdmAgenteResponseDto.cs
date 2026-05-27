// =====================================================================================
// DTO RESPONSE ADM AGENTE LEGACY - AdmAgenteResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO para respuestas de API de agentes legacy desde adCABS2016 (tabla admAgentes).
// Incluye campos completos de la tabla legacy incluyendo comisiones y clasificaciones.
//
// PROPÓSITO:
// - Comunicación API-Frontend para agentes legacy
// - Exponer datos completos de admAgentes
// - Incluir comisiones, clasificaciones y timestamps
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para agentes legacy de admAgentes (adCABS2016)
    /// Contiene información completa de agentes del sistema Adminpaq
    /// </summary>
    public class AdmAgenteResponseDto
    {
        // ═══════════════════════════════════════════════════════════════
        // IDENTIFICACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del agente en el sistema legacy
        /// </summary>
        public int IdAgente { get; set; }

        /// <summary>
        /// Código del agente (ej: "VEN001", "AGE-01")
        /// </summary>
        public string CodigoAgente { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del agente
        /// </summary>
        public string NombreAgente { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // INFORMACIÓN PRINCIPAL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Fecha de alta en el sistema
        /// </summary>
        public DateTime FechaAlta { get; set; }

        /// <summary>
        /// Tipo de agente
        /// 1 = Vendedor, 2 = Cobrador, 3 = Ambos
        /// </summary>
        public int TipoAgente { get; set; }

        /// <summary>
        /// Descripción del tipo de agente
        /// </summary>
        public string TipoAgenteDescripcion => TipoAgente switch
        {
            1 => "Vendedor",
            2 => "Cobrador",
            3 => "Vendedor/Cobrador",
            _ => "Otro"
        };

        // ═══════════════════════════════════════════════════════════════
        // COMISIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Porcentaje de comisión por ventas
        /// </summary>
        public double ComisionVenta { get; set; }

        /// <summary>
        /// Porcentaje de comisión por cobros
        /// </summary>
        public double ComisionCobro { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del cliente asociado (0 si no tiene)
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// ID del proveedor asociado (0 si no tiene)
        /// </summary>
        public int ProveedorId { get; set; }

        /// <summary>
        /// Indica si el agente tiene cliente asociado
        /// </summary>
        public bool TieneClienteAsociado => ClienteId > 0;

        /// <summary>
        /// Indica si el agente tiene proveedor asociado
        /// </summary>
        public bool TieneProveedorAsociado => ProveedorId > 0;

        // ═══════════════════════════════════════════════════════════════
        // CLASIFICACIONES (Solo las 3 primeras para simplificar)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID de clasificación 1 (0 si no tiene)
        /// </summary>
        public int Clasificacion1 { get; set; }

        /// <summary>
        /// ID de clasificación 2 (0 si no tiene)
        /// </summary>
        public int Clasificacion2 { get; set; }

        /// <summary>
        /// ID de clasificación 3 (0 si no tiene)
        /// </summary>
        public int Clasificacion3 { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CAMPOS ADICIONALES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Campo de texto extra 2 (uso configurable por empresa)
        /// </summary>
        public string? TextoExtra2 { get; set; }

        /// <summary>
        /// Timestamp de última modificación
        /// </summary>
        public string? Timestamp { get; set; }
    }
}
