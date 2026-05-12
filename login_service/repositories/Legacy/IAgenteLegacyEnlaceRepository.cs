// =====================================================================================
// INTERFAZ REPOSITORIO ENLACE AGENTE LEGACY
// =====================================================================================
// Define el contrato para acceso a datos de enlaces Usuario-Agente Legacy
// =====================================================================================

using back_cabs.CRM.models.Auth;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Repositorio para gestión de datos de enlaces Usuario-Agente Legacy
    /// Proporciona acceso a datos de usuarios y agentes para el servicio de enlaces
    /// </summary>
    public interface IAgenteLegacyEnlaceRepository
    {
        // ═══════════════════════════════════════════════════════════════
        // CONSULTAS DE AGENTES LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los agentes del sistema legacy
        /// </summary>
        Task<IEnumerable<AdmAgente>> ObtenerTodosAgentesAsync();

        /// <summary>
        /// Obtiene un agente específico por su ID
        /// </summary>
        Task<AdmAgente?> ObtenerAgentePorIdAsync(int agenteId);

        /// <summary>
        /// Verifica si existe un agente en el sistema legacy
        /// </summary>
        Task<bool> ExisteAgenteAsync(int agenteId);

        // ═══════════════════════════════════════════════════════════════
        // CONSULTAS DE USUARIOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene un usuario por su ID para lectura
        /// </summary>
        Task<UsuarioAuth?> ObtenerUsuarioPorIdAsync(int usuarioId);

        /// <summary>
        /// Obtiene todos los usuarios que tienen agente enlazado
        /// </summary>
        Task<IEnumerable<UsuarioAuth>> ObtenerUsuariosConAgenteEnlazadoAsync();

        /// <summary>
        /// Obtiene todos los usuarios activos sin agente enlazado
        /// </summary>
        Task<IEnumerable<UsuarioAuth>> ObtenerUsuariosSinAgenteEnlazadoAsync();

        /// <summary>
        /// Obtiene el usuario que tiene enlazado un agente específico
        /// </summary>
        Task<UsuarioAuth?> ObtenerUsuarioPorAgenteAsync(int agenteId);

        /// <summary>
        /// Verifica si un usuario tiene agente enlazado
        /// </summary>
        Task<bool> UsuarioTieneAgenteAsync(int usuarioId);

        // ═══════════════════════════════════════════════════════════════
        // ESTADÍSTICAS Y CONTEOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Cuenta total de usuarios activos
        /// </summary>
        Task<int> ContarUsuariosActivosAsync();

        /// <summary>
        /// Cuenta usuarios activos con agente enlazado
        /// </summary>
        Task<int> ContarUsuariosEnlazadosAsync();

        /// <summary>
        /// Cuenta total de agentes en el sistema legacy
        /// </summary>
        Task<int> ContarAgentesLegacyAsync();

        // ═══════════════════════════════════════════════════════════════
        // OPERACIONES DE ESCRITURA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Enlaza un usuario con un agente legacy
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="agenteId">ID del agente</param>
        /// <param name="codigoAgente">Código del agente</param>
        /// <param name="nombreAgente">Nombre del agente</param>
        Task<bool> EnlazarUsuarioConAgenteAsync(
            int usuarioId, 
            int agenteId, 
            string codigoAgente, 
            string nombreAgente);

        /// <summary>
        /// Remueve el enlace de un usuario con su agente
        /// </summary>
        Task<bool> DesenlazarUsuarioAsync(int usuarioId);

        /// <summary>
        /// Obtiene el ID del agente enlazado a un usuario (para documentos)
        /// Retorna 0 si no tiene agente enlazado
        /// </summary>
        Task<int> ObtenerIdAgenteDeUsuarioAsync(int usuarioId);
    }
}
