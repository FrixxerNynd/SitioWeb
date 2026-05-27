namespace back_cabs.CRM.Core.Exceptions;

/// <summary>
/// Excepción base para errores relacionados con cotizaciones
/// </summary>
public class CotizacionException : Exception
{
    public CotizacionException(string message) : base(message) { }
    public CotizacionException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Excepción cuando una llave foránea no existe en la base de datos
/// </summary>
public class ForeignKeyNotFoundException : CotizacionException
{
    public string Campo { get; }
    public int ValorId { get; }

    public ForeignKeyNotFoundException(string campo, int valorId) 
        : base($"El {campo} con ID {valorId} no existe en la base de datos.")
    {
        Campo = campo;
        ValorId = valorId;
    }
}

/// <summary>
/// Excepción cuando se intenta crear un duplicado
/// </summary>
public class DuplicateRecordException : CotizacionException
{
    public string Campo { get; }
    public object Valor { get; }

    public DuplicateRecordException(string campo, object valor) 
        : base($"Ya existe un registro con {campo} = '{valor}'.")
    {
        Campo = campo;
        Valor = valor;
    }
}

/// <summary>
/// Excepción cuando no se puede acceder a un recurso por problemas de autorización o estado
/// </summary>
public class ResourceAccessDeniedException : CotizacionException
{
    public ResourceAccessDeniedException(string message) : base(message) { }
}

/// <summary>
/// Excepción cuando los datos de validación fallan
/// </summary>
public class ValidationException : CotizacionException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message)
    {
        Errors = errors;
    }
}
