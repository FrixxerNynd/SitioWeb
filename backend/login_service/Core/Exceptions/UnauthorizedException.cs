// =====================================================================================
// EXCEPCIÓN NO AUTORIZADO - UnauthorizedException.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define una excepción para errores de autenticación (401).
//
// CUÁNDO USARLO:
// - Cuando las credenciales son inválidas
// - Cuando el token JWT es inválido o expiró
// - Cuando se requiere autenticación y no está presente
//
// EJEMPLO:
// if (!await _authService.ValidarCredencialesAsync(email, password))
//     throw new UnauthorizedException("Credenciales inválidas");
//
// =====================================================================================

using back_cabs.CRM.Middleware;

namespace back_cabs.CRM.Core.Exceptions;

/// <summary>
/// Excepción para errores de autenticación (401 Unauthorized)
/// </summary>
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException()
        : base("No está autorizado para realizar esta operación", TipoError.ErrorNoAutorizado, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string mensaje)
        : base(mensaje, TipoError.ErrorNoAutorizado, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string mensaje, object detalles)
        : base(mensaje, TipoError.ErrorNoAutorizado, "UNAUTHORIZED", detalles)
    {
    }
}
