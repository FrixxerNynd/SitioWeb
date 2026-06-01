// =====================================================================================
// SERVICIO ENLACE AGENTE LEGACY - AgenteLegacyEnlaceService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa la lógica de negocio para gestionar enlaces entre usuarios CRM
// (tabla auth_usuarios en cabs_pruebas) y agentes del sistema legacy
// (tabla admAgentes en adCABS2016).
//
// PROPÓSITO:
// - Permitir a administradores enlazar usuarios con agentes legacy
// - Al crear documentos (cotizaciones), usar el agente enlazado
// - Monitorear el estado de enlaces para el administrador
//
// BASES DE DATOS:
// - WriteContext: cabs_pruebas (para actualizar auth_usuarios)
// - ReadOnlyContext: cabs_pruebas (para consultas de usuarios)
// - LegacyCompacReadOnlyContext: adCABS2016 (para consultar admAgentes)
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.Auth;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio para gestión de enlaces Usuario-Agente Legacy
    /// Conecta usuarios del CRM con agentes de Adminpaq para documentos
    /// </summary>
    public class AgenteLegacyEnlaceService : IAgenteLegacyEnlaceService
    {
        private readonly WriteContext _writeContext;                        // cabs_pruebas - escritura
        private readonly ReadOnlyContext _readContext;                      // cabs_pruebas - lectura
        private readonly LegacyCompacReadOnlyContext _legacyContext;        // adCABS2016 - lectura
        private readonly ILogger<AgenteLegacyEnlaceService> _logger;

        public AgenteLegacyEnlaceService(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            LegacyCompacReadOnlyContext legacyContext,
            ILogger<AgenteLegacyEnlaceService> logger)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ═══════════════════════════════════════════════════════════════
        // CONSULTAS DE AGENTES LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<IEnumerable<AgenteLegacyConEnlaceDto>> ObtenerAgentesLegacyAsync()
        {
            try
            {
                // Obtener todos los agentes del sistema legacy
                var agentes = await _legacyContext.AdmAgentes
                    .AsNoTracking()
                    .OrderBy(a => a.CNombreAgente)
                    .ToListAsync();

                // Obtener usuarios que tienen agente enlazado
                var usuariosConEnlace = await _readContext.UsuariosAuth
                    .AsNoTracking()
                    .Where(u => u.IdAgenteLegacy != null)
                    .Select(u => new
                    {
                        u.IdAgenteLegacy,
                        u.Id,
                        u.Nombre,
                        u.Apellido,
                        u.Email,
                        u.FechaEnlaceAgente
                    })
                    .ToListAsync();

                // Crear diccionario de enlaces para acceso rápido
                var enlacesDict = usuariosConEnlace.ToDictionary(
                    e => e.IdAgenteLegacy!.Value,
                    e => new
                    {
                        e.Id,
                        NombreCompleto = $"{e.Nombre} {e.Apellido}".Trim(),
                        e.Email,
                        e.FechaEnlaceAgente
                    });

                // Mapear agentes con información de enlace
                return agentes.Select(a => new AgenteLegacyConEnlaceDto
                {
                    Id = a.CIdAgente,
                    Codigo = a.CCodigoAgente,
                    Nombre = a.CNombreAgente,
                    Tipo = a.CTipoAgente,
                    TipoDescripcion = ObtenerDescripcionTipoAgente(a.CTipoAgente),
                    ComisionVenta = a.CComisionVentaAgente,
                    ComisionCobro = a.CComisionCobroAgente,
                    FechaAlta = a.CFechaAltaAgente,
                    EstaEnlazado = enlacesDict.ContainsKey(a.CIdAgente),
                    UsuarioEnlazadoId = enlacesDict.TryGetValue(a.CIdAgente, out var enlace) ? enlace.Id : null,
                    UsuarioEnlazadoNombre = enlace?.NombreCompleto,
                    UsuarioEnlazadoEmail = enlace?.Email,
                    FechaEnlace = enlace?.FechaEnlaceAgente
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agentes legacy con enlaces");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AgenteLegacyConEnlaceDto>> ObtenerAgentesDisponiblesAsync()
        {
            var todosAgentes = await ObtenerAgentesLegacyAsync();
            return todosAgentes.Where(a => !a.EstaEnlazado);
        }

        /// <inheritdoc/>
        public async Task<AgenteLegacyConEnlaceDto?> ObtenerAgentePorIdAsync(int agenteId)
        {
            var agentes = await ObtenerAgentesLegacyAsync();
            return agentes.FirstOrDefault(a => a.Id == agenteId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AgenteLegacyConEnlaceDto>> BuscarAgentesAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerAgentesLegacyAsync();

            var terminoLower = termino.ToLower();
            var agentes = await ObtenerAgentesLegacyAsync();

            return agentes.Where(a =>
                a.Codigo.ToLower().Contains(terminoLower) ||
                a.Nombre.ToLower().Contains(terminoLower));
        }

        // ═══════════════════════════════════════════════════════════════
        // OPERACIONES DE ENLACE
        // ═══════════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<EnlaceAgenteResponseDto> EnlazarUsuarioConAgenteAsync(EnlazarAgenteRequestDto request)
        {
            try
            {
                _logger.LogInformation(
                    "Iniciando enlace: Usuario {UsuarioId} → Agente {AgenteId}",
                    request.UsuarioId, request.AgenteId);

                // Validar el enlace primero
                var validacion = await ValidarEnlaceAsync(request.UsuarioId, request.AgenteId);
                if (!validacion.EsValido)
                {
                    _logger.LogWarning("Validación de enlace fallida: {Mensaje}", validacion.Mensaje);
                    return new EnlaceAgenteResponseDto
                    {
                        Exitoso = false,
                        Mensaje = validacion.Mensaje
                    };
                }

                // Obtener usuario del contexto de escritura
                var usuario = await _writeContext.UsuariosAuth.FindAsync(request.UsuarioId);
                if (usuario == null)
                {
                    return new EnlaceAgenteResponseDto
                    {
                        Exitoso = false,
                        Mensaje = "Usuario no encontrado"
                    };
                }

                // Obtener agente del sistema legacy
                var agente = await _legacyContext.AdmAgentes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.CIdAgente == request.AgenteId);

                if (agente == null)
                {
                    return new EnlaceAgenteResponseDto
                    {
                        Exitoso = false,
                        Mensaje = "Agente no encontrado en el sistema legacy"
                    };
                }

                // Guardar info del agente anterior si existe (para log)
                var agenteAnteriorCodigo = usuario.CodigoAgenteLegacy;

                // Realizar el enlace
                usuario.IdAgenteLegacy = agente.CIdAgente;
                usuario.CodigoAgenteLegacy = agente.CCodigoAgente;
                usuario.NombreAgenteLegacy = agente.CNombreAgente;
                usuario.FechaEnlaceAgente = DateTime.UtcNow;
                usuario.ActualizarFechaModificacion();

                await _writeContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Enlace exitoso: Usuario {Usuario} ({Email}) → Agente {Codigo} ({Nombre}). Anterior: {Anterior}",
                    usuario.NombreCompleto, usuario.Email,
                    agente.CCodigoAgente, agente.CNombreAgente,
                    agenteAnteriorCodigo ?? "Ninguno");

                return new EnlaceAgenteResponseDto
                {
                    Exitoso = true,
                    Mensaje = validacion.EsCambioAgente
                        ? $"Agente actualizado exitosamente. Anterior: {agenteAnteriorCodigo}"
                        : "Enlace creado exitosamente",
                    Detalle = new EnlaceDetalleDto
                    {
                        UsuarioId = usuario.Id,
                        UsuarioNombre = usuario.NombreCompleto,
                        UsuarioEmail = usuario.Email,
                        AgenteId = agente.CIdAgente,
                        AgenteCodigo = agente.CCodigoAgente,
                        AgenteNombre = agente.CNombreAgente,
                        FechaEnlace = usuario.FechaEnlaceAgente!.Value
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al enlazar usuario {UsuarioId} con agente {AgenteId}",
                    request.UsuarioId, request.AgenteId);

                return new EnlaceAgenteResponseDto
                {
                    Exitoso = false,
                    Mensaje = "Error interno al crear el enlace"
                };
            }
        }

        /// <inheritdoc/>
        public async Task<EnlaceAgenteResponseDto> DesenlazarUsuarioAsync(int usuarioId)
        {
            try
            {
                var usuario = await _writeContext.UsuariosAuth.FindAsync(usuarioId);

                if (usuario == null)
                {
                    return new EnlaceAgenteResponseDto
                    {
                        Exitoso = false,
                        Mensaje = "Usuario no encontrado"
                    };
                }

                if (!usuario.TieneAgenteLegacy)
                {
                    return new EnlaceAgenteResponseDto
                    {
                        Exitoso = false,
                        Mensaje = "El usuario no tiene un agente enlazado"
                    };
                }

                // Guardar info para el log y respuesta
                var agenteAnterior = new
                {
                    Id = usuario.IdAgenteLegacy,
                    Codigo = usuario.CodigoAgenteLegacy,
                    Nombre = usuario.NombreAgenteLegacy
                };

                // Remover enlace
                usuario.IdAgenteLegacy = null;
                usuario.CodigoAgenteLegacy = null;
                usuario.NombreAgenteLegacy = null;
                usuario.FechaEnlaceAgente = null;
                usuario.ActualizarFechaModificacion();

                await _writeContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Desenlace exitoso: Usuario {Usuario} ({Email}) ← Agente {Codigo} ({Nombre})",
                    usuario.NombreCompleto, usuario.Email,
                    agenteAnterior.Codigo, agenteAnterior.Nombre);

                return new EnlaceAgenteResponseDto
                {
                    Exitoso = true,
                    Mensaje = $"Enlace con agente {agenteAnterior.Codigo} removido exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desenlazar usuario {UsuarioId}", usuarioId);

                return new EnlaceAgenteResponseDto
                {
                    Exitoso = false,
                    Mensaje = "Error interno al remover el enlace"
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ValidacionEnlaceDto> ValidarEnlaceAsync(int usuarioId, int agenteId)
        {
            // Verificar que el usuario existe y está activo
            var usuario = await _readContext.UsuariosAuth
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return new ValidacionEnlaceDto
                {
                    EsValido = false,
                    Mensaje = "El usuario no existe"
                };
            }

            if (!usuario.Activo)
            {
                return new ValidacionEnlaceDto
                {
                    EsValido = false,
                    Mensaje = "El usuario está inactivo"
                };
            }

            // Verificar que el agente existe en el sistema legacy
            var agenteExiste = await _legacyContext.AdmAgentes
                .AsNoTracking()
                .AnyAsync(a => a.CIdAgente == agenteId);

            if (!agenteExiste)
            {
                return new ValidacionEnlaceDto
                {
                    EsValido = false,
                    Mensaje = "El agente no existe en el sistema legacy"
                };
            }

            // Verificar que el agente no esté enlazado a otro usuario
            var usuarioConEsteAgente = await _readContext.UsuariosAuth
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdAgenteLegacy == agenteId && u.Id != usuarioId);

            if (usuarioConEsteAgente != null)
            {
                return new ValidacionEnlaceDto
                {
                    EsValido = false,
                    Mensaje = $"El agente ya está enlazado al usuario {usuarioConEsteAgente.NombreCompleto} ({usuarioConEsteAgente.Email})"
                };
            }

            // Verificar si es un cambio de agente
            var esCambio = usuario.TieneAgenteLegacy && usuario.IdAgenteLegacy != agenteId;

            return new ValidacionEnlaceDto
            {
                EsValido = true,
                Mensaje = esCambio ? "Se reemplazará el agente actual" : "Enlace válido",
                EsCambioAgente = esCambio,
                AgenteAnterior = esCambio ? $"{usuario.CodigoAgenteLegacy} - {usuario.NombreAgenteLegacy}" : null
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // CONSULTAS DE USUARIOS
        // ═══════════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<IEnumerable<UsuarioConEnlaceDto>> ObtenerUsuariosConEnlaceAsync()
        {
            return await _readContext.UsuariosAuth
                .AsNoTracking()
                .Where(u => u.IdAgenteLegacy != null)
                .OrderBy(u => u.Nombre)
                .Select(u => new UsuarioConEnlaceDto
                {
                    Id = u.Id,
                    NombreCompleto = u.Nombre + " " + u.Apellido,
                    Email = u.Email,
                    Rol = u.Rol,
                    Activo = u.Activo,
                    TieneAgenteLegacy = true,
                    IdAgenteLegacy = u.IdAgenteLegacy,
                    CodigoAgenteLegacy = u.CodigoAgenteLegacy,
                    NombreAgenteLegacy = u.NombreAgenteLegacy,
                    FechaEnlaceAgente = u.FechaEnlaceAgente
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UsuarioSinEnlaceDto>> ObtenerUsuariosSinEnlazarAsync()
        {
            return await _readContext.UsuariosAuth
                .AsNoTracking()
                .Where(u => u.IdAgenteLegacy == null && u.Activo)
                .OrderBy(u => u.Nombre)
                .Select(u => new UsuarioSinEnlaceDto
                {
                    Id = u.Id,
                    NombreCompleto = u.Nombre + " " + u.Apellido,
                    Email = u.Email,
                    Rol = u.Rol,
                    CreadoEn = u.CreadoEn
                })
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════════════════
        // DASHBOARD Y MONITOREO
        // ═══════════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<EnlacesDashboardDto> ObtenerDashboardAsync()
        {
            try
            {
                // Estadísticas de usuarios
                var totalUsuarios = await _readContext.UsuariosAuth
                    .CountAsync(u => u.Activo);

                var usuariosEnlazados = await _readContext.UsuariosAuth
                    .CountAsync(u => u.IdAgenteLegacy != null && u.Activo);

                // Estadísticas de agentes
                var totalAgentes = await _legacyContext.AdmAgentes.CountAsync();

                // Obtener listas
                var usuariosConEnlace = await ObtenerUsuariosConEnlaceAsync();
                var usuariosPendientes = await ObtenerUsuariosSinEnlazarAsync();
                var agentesDisponibles = (await ObtenerAgentesDisponiblesAsync()).ToList();

                return new EnlacesDashboardDto
                {
                    TotalUsuarios = totalUsuarios,
                    UsuariosEnlazados = usuariosEnlazados,
                    UsuariosSinEnlazar = totalUsuarios - usuariosEnlazados,
                    TotalAgentesLegacy = totalAgentes,
                    AgentesDisponibles = agentesDisponibles.Count,
                    PorcentajeEnlazados = totalUsuarios > 0
                        ? Math.Round((double)usuariosEnlazados / totalUsuarios * 100, 2)
                        : 0,
                    UsuariosConEnlace = usuariosConEnlace.ToList(),
                    UsuariosPendientes = usuariosPendientes.ToList(),
                    AgentesDisponiblesList = agentesDisponibles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard de enlaces");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // OPERACIONES PARA DOCUMENTOS LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<int> ObtenerIdAgenteParaDocumentoAsync(int usuarioId)
        {
            var usuario = await _readContext.UsuariosAuth
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            // Retornar 0 si no tiene agente (valor por defecto en Adminpaq)
            return usuario?.IdAgenteLegacy ?? 0;
        }

        /// <inheritdoc/>
        public async Task<bool> UsuarioPuedeCrearDocumentosAsync(int usuarioId)
        {
            return await _readContext.UsuariosAuth
                .AnyAsync(u => u.Id == usuarioId && u.IdAgenteLegacy != null && u.Activo);
        }

        /// <inheritdoc/>
        public async Task<AdmAgente?> ObtenerAgentePorUsuarioAsync(int usuarioId)
        {
            var usuario = await _readContext.UsuariosAuth
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario?.IdAgenteLegacy == null)
                return null;

            return await _legacyContext.AdmAgentes
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.CIdAgente == usuario.IdAgenteLegacy);
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS AUXILIARES PRIVADOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene la descripción del tipo de agente
        /// </summary>
        private static string ObtenerDescripcionTipoAgente(int tipo)
        {
            return tipo switch
            {
                1 => "Vendedor",
                2 => "Cobrador",
                3 => "Vendedor/Cobrador",
                _ => "Otro"
            };
        }
    }
}
