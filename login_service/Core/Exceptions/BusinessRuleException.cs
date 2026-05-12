// =====================================================================================
// EXCEPCIÓN REGLA DE NEGOCIO - BusinessRuleException.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define una excepción para violaciones de reglas de negocio específicas.
//
// CUÁNDO USARLO:
// - Cuando se viola una regla de negocio
// - Cuando una operación no es válida según las políticas del negocio
// - Para validaciones complejas que no son de formato
//
// EJEMPLO:
// if (orden.Total < 0)
//     throw new BusinessRuleException("El total de la orden no puede ser negativo");
//
// =====================================================================================

using back_cabs.CRM.Middleware;

namespace back_cabs.CRM.Core.Exceptions;

/// <summary>
/// Excepción lanzada cuando se viola una regla de negocio
/// Retorna HTTP 422 - Unprocessable Entity
/// </summary>
public class BusinessRuleException : ApplicationException
{
    /// <summary>
    /// Constructor con mensaje de regla de negocio violada
    /// </summary>
    /// <param name="mensaje">Descripción de la regla de negocio violada</param>
    public BusinessRuleException(string mensaje)
        : base(mensaje, TipoError.ErrorValidacion, "BUSINESS_RULE_VIOLATION")
    {
    }

    /// <summary>
    /// Constructor con detalles adicionales
    /// </summary>
    /// <param name="mensaje">Descripción de la regla violada</param>
    /// <param name="detalles">Objeto con información adicional del contexto</param>
    public BusinessRuleException(string mensaje, object detalles)
        : base(mensaje, TipoError.ErrorValidacion, "BUSINESS_RULE_VIOLATION", detalles)
    {
    }

    /// <summary>
    /// Constructor para reglas relacionadas con estados
    /// </summary>
    /// <param name="entidad">Nombre de la entidad</param>
    /// <param name="regla">Nombre de la regla violada</param>
    /// <param name="motivo">Explicación del motivo</param>
    public BusinessRuleException(string entidad, string regla, string motivo)
        : base(
            $"Regla de negocio '{regla}' violada para {entidad}: {motivo}",
            TipoError.ErrorValidacion,
            "BUSINESS_RULE_VIOLATION",
            new { Entidad = entidad, Regla = regla, Motivo = motivo })
    {
    }
}
