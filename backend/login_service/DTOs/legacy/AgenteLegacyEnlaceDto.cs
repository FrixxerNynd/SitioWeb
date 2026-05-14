// =====================================================================================
// DTOs ENLACE AGENTE LEGACY - AgenteLegacyEnlaceDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los DTOs para operaciones de enlace entre usuarios del CRM (cabs_pruebas)
// y agentes del sistema legacy (adCABS2016.admAgentes).
//
// PROPÓSITO:
// - Request/Response para enlazar usuarios con agentes legacy
// - Dashboard de monitoreo de enlaces
// - Listados de usuarios y agentes para administración
//
// USO:
// - POST /api/agentes-legacy-enlace/enlazar
// - DELETE /api/agentes-legacy-enlace/desenlazar/{usuarioId}
// - GET /api/agentes-legacy-enlace/dashboard
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Legacy
{
    // ═══════════════════════════════════════════════════════════════
    // DTOs DE REQUEST
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO para la operación de enlace usuario-agente
    /// </summary>
    public class EnlazarAgenteRequestDto
    {
        /// <summary>
        /// ID del usuario en el sistema CRM (auth_usuarios.id)
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// ID del agente en el sistema legacy (admAgentes.CIDAGENTE)
        /// </summary>
        public int AgenteId { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // DTOs DE RESPONSE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO para respuesta del enlace
    /// </summary>
    public class EnlaceAgenteResponseDto
    {
        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool Exitoso { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Detalle del enlace realizado (null si falló)
        /// </summary>
        public EnlaceDetalleDto? Detalle { get; set; }
    }

    /// <summary>
    /// Detalle del enlace realizado
    /// </summary>
    public class EnlaceDetalleDto
    {
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string UsuarioEmail { get; set; } = string.Empty;
        public int AgenteId { get; set; }
        public string AgenteCodigo { get; set; } = string.Empty;
        public string AgenteNombre { get; set; } = string.Empty;
        public DateTime FechaEnlace { get; set; }
    }

    /// <summary>
    /// DTO extendido de agente legacy con información de enlace
    /// </summary>
    public class AgenteLegacyConEnlaceDto
    {
        // ═══════════════════════════════════════════════════════════════
        // DATOS DEL AGENTE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del agente en el sistema legacy
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Código del agente (ej: "VEN001")
        /// </summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del agente
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de agente (1=Vendedor, 2=Cobrador, 3=Ambos)
        /// </summary>
        public int Tipo { get; set; }

        /// <summary>
        /// Descripción del tipo de agente
        /// </summary>
        public string TipoDescripcion { get; set; } = string.Empty;

        /// <summary>
        /// Porcentaje de comisión por ventas
        /// </summary>
        public double ComisionVenta { get; set; }

        /// <summary>
        /// Porcentaje de comisión por cobros
        /// </summary>
        public double ComisionCobro { get; set; }

        /// <summary>
        /// Fecha de alta en el sistema legacy
        /// </summary>
        public DateTime FechaAlta { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // ESTADO DEL ENLACE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Indica si este agente ya está enlazado a un usuario
        /// </summary>
        public bool EstaEnlazado { get; set; }

        /// <summary>
        /// ID del usuario enlazado (null si no está enlazado)
        /// </summary>
        public int? UsuarioEnlazadoId { get; set; }

        /// <summary>
        /// Nombre del usuario enlazado (null si no está enlazado)
        /// </summary>
        public string? UsuarioEnlazadoNombre { get; set; }

        /// <summary>
        /// Email del usuario enlazado (null si no está enlazado)
        /// </summary>
        public string? UsuarioEnlazadoEmail { get; set; }

        /// <summary>
        /// Fecha del enlace (null si no está enlazado)
        /// </summary>
        public DateTime? FechaEnlace { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // DTOs DE USUARIOS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO de usuario con información de enlace a agente legacy
    /// </summary>
    public class UsuarioConEnlaceDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }

        // Información del enlace
        public bool TieneAgenteLegacy { get; set; }
        public int? IdAgenteLegacy { get; set; }
        public string? CodigoAgenteLegacy { get; set; }
        public string? NombreAgenteLegacy { get; set; }
        public DateTime? FechaEnlaceAgente { get; set; }
    }

    /// <summary>
    /// DTO para listado de usuarios pendientes de enlace
    /// </summary>
    public class UsuarioSinEnlaceDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // DTOs DE DASHBOARD Y MONITOREO
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO para el dashboard de monitoreo de enlaces
    /// </summary>
    public class EnlacesDashboardDto
    {
        // ═══════════════════════════════════════════════════════════════
        // ESTADÍSTICAS GENERALES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Total de usuarios activos en el sistema CRM
        /// </summary>
        public int TotalUsuarios { get; set; }

        /// <summary>
        /// Usuarios que tienen agente legacy enlazado
        /// </summary>
        public int UsuariosEnlazados { get; set; }

        /// <summary>
        /// Usuarios sin agente legacy enlazado
        /// </summary>
        public int UsuariosSinEnlazar { get; set; }

        /// <summary>
        /// Total de agentes en el sistema legacy
        /// </summary>
        public int TotalAgentesLegacy { get; set; }

        /// <summary>
        /// Agentes disponibles (sin enlazar a ningún usuario)
        /// </summary>
        public int AgentesDisponibles { get; set; }

        /// <summary>
        /// Porcentaje de usuarios enlazados
        /// </summary>
        public double PorcentajeEnlazados { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // LISTAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Usuarios que tienen enlace con agente legacy
        /// </summary>
        public List<UsuarioConEnlaceDto> UsuariosConEnlace { get; set; } = new();

        /// <summary>
        /// Usuarios pendientes de enlazar
        /// </summary>
        public List<UsuarioSinEnlaceDto> UsuariosPendientes { get; set; } = new();

        /// <summary>
        /// Agentes disponibles para enlazar
        /// </summary>
        public List<AgenteLegacyConEnlaceDto> AgentesDisponiblesList { get; set; } = new();
    }

    /// <summary>
    /// DTO para validación de enlace
    /// </summary>
    public class ValidacionEnlaceDto
    {
        /// <summary>
        /// Indica si el enlace propuesto es válido
        /// </summary>
        public bool EsValido { get; set; }

        /// <summary>
        /// Mensaje descriptivo de la validación
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Indica si es un cambio de agente (el usuario ya tenía otro)
        /// </summary>
        public bool EsCambioAgente { get; set; }

        /// <summary>
        /// Información del agente anterior si es cambio
        /// </summary>
        public string? AgenteAnterior { get; set; }
    }
}
