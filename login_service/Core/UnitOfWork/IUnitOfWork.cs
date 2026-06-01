// =====================================================================================
// UNIT OF WORK INTERFACE - IUnitOfWork.cs
// =====================================================================================
//
// ¿QUÉ ES EL UNIT OF WORK PATTERN?
// Es un patrón que mantiene una lista de objetos afectados por una transacción
// de negocio y coordina la escritura de cambios y la resolución de problemas
// de concurrencia.
//
// PROPÓSITO:
// - Agrupar múltiples operaciones de BD en una sola transacción
// - Garantizar consistencia de datos (ACID)
// - Simplificar el rollback en caso de error
// - Centralizar el SaveChanges()
//
// CUÁNDO USARLO:
// - Operaciones que involucran múltiples tablas
// - Cuando necesitas garantizar que TODAS las operaciones se completen o NINGUNA
// - Ejemplo: Crear orden + crear ejecución + actualizar vehículo = 1 transacción
//
// BENEFICIOS:
// ✅ Atomicidad: Todo o nada
// ✅ Consistencia: Los datos siempre están en estado válido
// ✅ Aislamiento: Las transacciones no interfieren entre sí
// ✅ Durabilidad: Los cambios persisten después del commit
//
// =====================================================================================

using back_cabs.CRM.Interfaces.Auth;
using back_cabs.CRM.Interfaces.Shared;


namespace back_cabs.CRM.Core.UnitOfWork
{
    /// <summary>
    /// Interfaz del patrón Unit of Work que coordina transacciones entre múltiples repositorios
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // ═══════════════════════════════════════════════════════════════
        // REPOSITORIOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Repositorio para operaciones con usuarios
        /// </summary>
        IUsuarioAuthRepository Usuarios { get; }    

        // ═══════════════════════════════════════════════════════════════
        // OPERACIONES DE PERSISTENCIA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // ═══════════════════════════════════════════════════════════════
        // MANEJO DE TRANSACCIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Inicia una nueva transacción de base de datos
        /// </summary>
        /// <remarks>
        /// Uso:
        /// <code>
        /// await _unitOfWork.BeginTransactionAsync();
        /// try
        /// {
        ///     // operaciones...
        ///     await _unitOfWork.CommitAsync();
        /// }
        /// catch
        /// {
        ///     await _unitOfWork.RollbackAsync();
        ///     throw;
        /// }
        /// </code>
        /// </remarks>
        Task BeginTransactionAsync();

        /// <summary>
        /// Confirma la transacción actual y guarda todos los cambios
        /// </summary>
        /// <remarks>
        /// Si falla, automáticamente hace rollback
        /// </remarks>
        Task CommitAsync();

        /// <summary>
        /// Revierte la transacción actual y descarta todos los cambios
        /// </summary>
        Task RollbackAsync();

        // ═══════════════════════════════════════════════════════════════
        // UTILIDADES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Indica si hay una transacción activa
        /// </summary>
        bool HasActiveTransaction { get; }

        /// <summary>
        /// Ejecuta una operación dentro de una transacción de forma automática
        /// </summary>
        /// <typeparam name="T">Tipo de retorno de la operación</typeparam>
        /// <param name="operation">Operación a ejecutar</param>
        /// <returns>Resultado de la operación</returns>
        /// <remarks>
        /// Simplifica el código:
        /// <code>
        /// var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        /// {
        ///     // operaciones...
        ///     return result;
        /// });
        /// </code>
        /// </remarks>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);

        /// <summary>
        /// Ejecuta una operación dentro de una transacción de forma automática (sin retorno)
        /// </summary>
        /// <param name="operation">Operación a ejecutar</param>
        Task ExecuteInTransactionAsync(Func<Task> operation);

        /// <summary>
        /// Ejecuta una operación utilizando la estrategia de ejecución configurada (Resiliency/Retries).
        /// Necesario cuando se usan transacciones distribuidas con EF Core y retry policies habilitadas.
        /// </summary>
        Task<T> ExecuteStrategyAsync<T>(Func<Task<T>> operation);

        /// <summary>
        /// Ejecuta una operación utilizando la estrategia de ejecución configurada (Resiliency/Retries) (sin retorno).
        /// Necesario cuando se usan transacciones distribuidas con EF Core y retry policies habilitadas.
        /// </summary>
        Task ExecuteStrategyAsync(Func<Task> operation);
    }
}
