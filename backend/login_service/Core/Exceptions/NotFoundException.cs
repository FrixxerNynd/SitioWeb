// =====================================================================================
// EXCEPCIÓN RECURSO NO ENCONTRADO - NotFoundException.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define una excepción específica para cuando un recurso no existe en la base de datos.
//
// CUÁNDO USARLO:
// - Cuando se busca un registro por ID y no existe
// - Cuando se busca por un criterio único y no hay resultados
//
// EJEMPLO:
// if (vehiculo == null)
//     throw new NotFoundException("Vehículo", id);
//
// =====================================================================================

using back_cabs.CRM.Middleware;

namespace back_cabs.CRM.Core.Exceptions;

/// <summary>
/// Excepción lanzada cuando un recurso solicitado no existe
/// Retorna HTTP 404 - Not Found
/// </summary>
public class NotFoundException : ApplicationException
{
    /// <summary>
    /// Constructor con entidad e ID
    /// </summary>
    /// <param name="entidad">Nombre de la entidad (ej: "Vehículo", "Cliente")</param>
    /// <param name="id">ID del recurso no encontrado</param>
    public NotFoundException(string entidad, object id)
        : base(
            $"{entidad} con ID '{id}' no fue encontrado.",
            TipoError.ErrorNoEncontrado,
            "RESOURCE_NOT_FOUND",
            new { Entidad = entidad, Id = id })
    {
    }

    /// <summary>
    /// Constructor con mensaje personalizado
    /// </summary>
    /// <param name="mensaje">Mensaje descriptivo del error</param>
    public NotFoundException(string mensaje)
        : base(mensaje, TipoError.ErrorNoEncontrado, "RESOURCE_NOT_FOUND")
    {
    }

    /// <summary>
    /// Constructor con entidad, campo y valor
    /// </summary>
    /// <param name="entidad">Nombre de la entidad</param>
    /// <param name="campo">Campo por el que se buscó</param>
    /// <param name="valor">Valor del campo</param>
    public NotFoundException(string entidad, string campo, object valor)
        : base(
            $"{entidad} con {campo} '{valor}' no fue encontrado.",
            TipoError.ErrorNoEncontrado,
            "RESOURCE_NOT_FOUND",
            new { Entidad = entidad, Campo = campo, Valor = valor })
    {
    }
}
