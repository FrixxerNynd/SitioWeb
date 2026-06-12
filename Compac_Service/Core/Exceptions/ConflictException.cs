// =====================================================================================
// EXCEPCIÓN DE CONFLICTO - ConflictException.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define una excepción para conflictos como duplicados o estados inválidos.
//
// CUÁNDO USARLO:
// - Cuando se intenta crear un registro con clave única duplicada
// - Cuando hay conflictos de concurrencia
// - Cuando un recurso ya existe
//
// EJEMPLO:
// if (await _repo.ExistsByPlacasAsync(dto.Placas))
//     throw new ConflictException("Vehículo", "placas", dto.Placas);
//
// =====================================================================================

using back_cabs.CRM.Middleware;

namespace back_cabs.CRM.Core.Exceptions;

/// <summary>
/// Excepción lanzada cuando hay un conflicto (duplicado, concurrencia, etc.)
/// Retorna HTTP 409 - Conflict
/// </summary>
public class ConflictException : ApplicationException
{
    /// <summary>
    /// Constructor con mensaje personalizado
    /// </summary>
    /// <param name="mensaje">Mensaje descriptivo del conflicto</param>
    public ConflictException(string mensaje)
        : base(mensaje, TipoError.ErrorConflicto, "CONFLICT")
    {
    }

    /// <summary>
    /// Constructor para duplicados (clave única violada)
    /// </summary>
    /// <param name="entidad">Nombre de la entidad</param>
    /// <param name="campo">Campo con valor duplicado</param>
    /// <param name="valor">Valor que está duplicado</param>
    public ConflictException(string entidad, string campo, object valor)
        : base(
            $"{entidad} con {campo} '{valor}' ya existe.",
            TipoError.ErrorConflicto,
            "DUPLICATE_RESOURCE",
            new { Entidad = entidad, Campo = campo, Valor = valor })
    {
    }

    /// <summary>
    /// Constructor para conflictos de estado
    /// </summary>
    /// <param name="entidad">Nombre de la entidad</param>
    /// <param name="estadoActual">Estado actual del recurso</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    public ConflictException(string entidad, string estadoActual, string operacion)
        : base(
            $"No se puede realizar '{operacion}' en {entidad} con estado '{estadoActual}'.",
            TipoError.ErrorConflicto,
            "INVALID_STATE_TRANSITION",
            new { Entidad = entidad, EstadoActual = estadoActual, Operacion = operacion })
    {
    }
}
