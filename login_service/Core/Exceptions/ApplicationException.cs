// =====================================================================================
// EXCEPCIÓN BASE DE APLICACIÓN - ApplicationException.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la excepción base para todas las excepciones personalizadas de la aplicación.
// Proporciona propiedades comunes como TipoError, CodigoError y Detalles para 
// estandarizar el manejo de errores en toda la aplicación.
//
// CUÁNDO USARLO:
// - Como clase base para crear excepciones específicas del dominio
// - Para mantener consistencia en el manejo de errores
// - Para facilitar el logging y tracking de errores
//
// EJEMPLO:
// public class MiExcepcionCustom : ApplicationException
// {
//     public MiExcepcionCustom(string mensaje) 
//         : base(mensaje, TipoError.ErrorValidacion, "MI_ERROR")
//     {
//     }
// }
//
// =====================================================================================

using back_cabs.CRM.Middleware;

namespace back_cabs.CRM.Core.Exceptions;

/// <summary>
/// Excepción base para todas las excepciones de la aplicación CABS
/// </summary>
public abstract class ApplicationException : Exception
{
    /// <summary>
    /// Tipo de error según el enum TipoError
    /// </summary>
    public TipoError TipoError { get; protected set; }

    /// <summary>
    /// Código de error para identificación única
    /// </summary>
    public string? CodigoError { get; protected set; }

    /// <summary>
    /// Detalles adicionales del error (objeto serializable)
    /// </summary>
    public object? Detalles { get; protected set; }

    /// <summary>
    /// Constructor con todos los parámetros
    /// </summary>
    protected ApplicationException(
        string mensaje,
        TipoError tipoError,
        string? codigoError = null,
        object? detalles = null)
        : base(mensaje)
    {
        TipoError = tipoError;
        CodigoError = codigoError ?? tipoError.ToString();
        Detalles = detalles;
    }

    /// <summary>
    /// Constructor con excepción interna
    /// </summary>
    protected ApplicationException(
        string mensaje,
        Exception innerException,
        TipoError tipoError)
        : base(mensaje, innerException)
    {
        TipoError = tipoError;
        CodigoError = tipoError.ToString();
    }
}
