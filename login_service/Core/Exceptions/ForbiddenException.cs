// =====================================================================================
// EXCEPCIÓN PROHIBIDO - ForbiddenException.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define una excepción para errores de permisos insuficientes (403).
//
// CUÁNDO USARLO:
// - Cuando el usuario está autenticado pero no tiene permisos
// - Cuando se intenta acceder a recursos de otro usuario
// - Cuando falta un rol o permiso específico
//
// EJEMPLO:
// if (orden.ClienteId != usuarioActual.ClienteId)
//     throw new ForbiddenException("No tiene permisos para ver esta orden");
//
// =====================================================================================

using back_cabs.CRM.Middleware;

namespace back_cabs.CRM.Core.Exceptions;

/// <summary>
/// Excepción para errores de permisos insuficientes (403 Forbidden)
/// </summary>
public class ForbiddenException : ApplicationException
{
    public ForbiddenException()
        : base("No tiene permisos para realizar esta operación", TipoError.ErrorProhibido, "FORBIDDEN")
    {
    }

    public ForbiddenException(string mensaje)
        : base(mensaje, TipoError.ErrorProhibido, "FORBIDDEN")
    {
    }

    public ForbiddenException(string recurso, string accion)
        : base($"No tiene permisos para {accion} el recurso '{recurso}'", TipoError.ErrorProhibido, "FORBIDDEN")
    {
    }

    public ForbiddenException(string mensaje, object detalles)
        : base(mensaje, TipoError.ErrorProhibido, "FORBIDDEN", detalles)
    {
    }
}
