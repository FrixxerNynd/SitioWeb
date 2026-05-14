// =====================================================================================
// INTERFAZ SERVICE ENLACE AGENTE LEGACY - IAgenteLegacyEnlaceService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de servicios para gestión de enlaces entre usuarios CRM
// y agentes del sistema legacy (adCABS2016.admAgentes).
//
// PROPÓSITO:
// - Enlazar/desenlazar usuarios con agentes legacy
// - Consultas de usuarios y agentes con estado de enlace
// - Dashboard de monitoreo para administradores
// - Operaciones para documentos (obtener ID de agente para cotizaciones)
//
// USO:
// Este servicio es inyectado en AgentesLegacyEnlaceController
// y puede ser usado por otros servicios que necesiten el ID de agente
// para crear documentos en el sistema legacy.
//
// =====================================================================================

using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Servicio para gestión de enlaces Usuario-Agente Legacy
    /// Permite vincular usuarios del CRM con agentes de Adminpaq
    /// </summary>
    public interface IAgenteLegacyEnlaceService
    {
        // ═══════════════════════════════════════════════════════════════
        // CONSULTAS DE AGENTES LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los agentes del sistema legacy con su estado de enlace
        /// </summary>
        /// <returns>Lista de agentes con información de enlace</returns>
        Task<IEnumerable<AgenteLegacyConEnlaceDto>> ObtenerAgentesLegacyAsync();

        /// <summary>
        /// Obtiene agentes legacy disponibles (sin enlazar a ningún usuario)
        /// </summary>
        /// <returns>Lista de agentes disponibles</returns>
        Task<IEnumerable<AgenteLegacyConEnlaceDto>> ObtenerAgentesDisponiblesAsync();

        /// <summary>
        /// Obtiene un agente legacy por su ID con información de enlace
        /// </summary>
        /// <param name="agenteId">ID del agente en el sistema legacy</param>
        /// <returns>Agente con información de enlace o null si no existe</returns>
        Task<AgenteLegacyConEnlaceDto?> ObtenerAgentePorIdAsync(int agenteId);

        /// <summary>
        /// Busca agentes por código o nombre
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <returns>Lista de agentes que coinciden con la búsqueda</returns>
        Task<IEnumerable<AgenteLegacyConEnlaceDto>> BuscarAgentesAsync(string termino);

        // ═══════════════════════════════════════════════════════════════
        // OPERACIONES DE ENLACE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Enlaza un usuario con un agente legacy
        /// Almacena el ID, código y nombre del agente en el usuario
        /// </summary>
        /// <param name="request">Datos del enlace (usuarioId, agenteId)</param>
        /// <returns>Resultado de la operación con detalles del enlace</returns>
        Task<EnlaceAgenteResponseDto> EnlazarUsuarioConAgenteAsync(EnlazarAgenteRequestDto request);

        /// <summary>
        /// Desenlaza un usuario de su agente legacy
        /// Limpia los campos de agente en el usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario a desenlazar</param>
        /// <returns>Resultado de la operación</returns>
        Task<EnlaceAgenteResponseDto> DesenlazarUsuarioAsync(int usuarioId);

        /// <summary>
        /// Valida si un enlace es posible antes de realizarlo
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="agenteId">ID del agente</param>
        /// <returns>Resultado de validación con mensaje</returns>
        Task<ValidacionEnlaceDto> ValidarEnlaceAsync(int usuarioId, int agenteId);

        // ═══════════════════════════════════════════════════════════════
        // CONSULTAS DE USUARIOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene usuarios con su información de enlace a agente
        /// </summary>
        /// <returns>Lista de usuarios con enlace</returns>
        Task<IEnumerable<UsuarioConEnlaceDto>> ObtenerUsuariosConEnlaceAsync();

        /// <summary>
        /// Obtiene usuarios activos sin enlazar a ningún agente
        /// </summary>
        /// <returns>Lista de usuarios pendientes de enlazar</returns>
        Task<IEnumerable<UsuarioSinEnlaceDto>> ObtenerUsuariosSinEnlazarAsync();

        // ═══════════════════════════════════════════════════════════════
        // DASHBOARD Y MONITOREO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene datos completos para el dashboard de monitoreo
        /// Incluye estadísticas, usuarios y agentes
        /// </summary>
        /// <returns>Dashboard con toda la información de enlaces</returns>
        Task<EnlacesDashboardDto> ObtenerDashboardAsync();

        // ═══════════════════════════════════════════════════════════════
        // OPERACIONES PARA DOCUMENTOS LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene el ID de agente legacy para usar en documentos
        /// Este método es clave para crear cotizaciones/documentos en Adminpaq
        /// </summary>
        /// <param name="usuarioId">ID del usuario logueado</param>
        /// <returns>ID del agente legacy, 0 si no tiene enlace</returns>
        Task<int> ObtenerIdAgenteParaDocumentoAsync(int usuarioId);

        /// <summary>
        /// Verifica si el usuario puede crear documentos en el sistema legacy
        /// Requiere tener un agente enlazado
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si puede crear documentos, false si no</returns>
        Task<bool> UsuarioPuedeCrearDocumentosAsync(int usuarioId);

        /// <summary>
        /// Obtiene la entidad completa del agente legacy asociado a un usuario
        /// Útil cuando se necesita información completa del agente
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Entidad AdmAgente o null si no tiene enlace</returns>
        Task<AdmAgente?> ObtenerAgentePorUsuarioAsync(int usuarioId);
    }
}
