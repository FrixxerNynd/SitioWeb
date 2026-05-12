// =====================================================================================
// EXCEPCIÓN SERVICIO EXTERNO - ExternalServiceException.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define una excepción para errores de comunicación con servicios externos (521).
//
// CUÁNDO USARLO:
// - Cuando falla una API de terceros
// - Cuando hay timeouts en servicios externos
// - Cuando servicios de pago, email, SMS, etc. no responden
//
// EJEMPLO:
// try {
//     await _emailService.EnviarAsync(email);
// } catch (Exception ex) {
//     throw new ExternalServiceException("EmailService", "Error al enviar email", ex);
// }
//
// =====================================================================================

using back_cabs.CRM.Middleware;

namespace back_cabs.CRM.Core.Exceptions;

/// <summary>
/// Excepción para errores de servicios externos (521 External Service Error)
/// </summary>
public class ExternalServiceException : ApplicationException
{
    public string NombreServicio { get; }

    public ExternalServiceException(string nombreServicio)
        : base($"Error al comunicarse con el servicio externo: {nombreServicio}", TipoError.ErrorServicioExterno, "EXTERNAL_SERVICE_ERROR")
    {
        NombreServicio = nombreServicio;
    }

    public ExternalServiceException(string nombreServicio, string mensaje)
        : base(mensaje, TipoError.ErrorServicioExterno, "EXTERNAL_SERVICE_ERROR")
    {
        NombreServicio = nombreServicio;
    }

    public ExternalServiceException(string nombreServicio, string mensaje, Exception innerException)
        : base(mensaje, innerException, TipoError.ErrorServicioExterno)
    {
        NombreServicio = nombreServicio;
        CodigoError = "EXTERNAL_SERVICE_ERROR";
    }
}
