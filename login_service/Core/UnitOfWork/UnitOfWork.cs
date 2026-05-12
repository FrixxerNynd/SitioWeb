// =====================================================================================
// UNIT OF WORK IMPLEMENTATION - UnitOfWork.cs
// =====================================================================================
//
// IMPLEMENTACIÓN CONCRETA del patrón Unit of Work
//
// RESPONSABILIDADES:
// 1. Instanciar repositorios bajo demanda (lazy loading)
// 2. Compartir el mismo WriteContext entre todos los repositorios
// 3. Gestionar transacciones de base de datos
// 4. Coordinar el SaveChanges() de múltiples repositorios
// 5. Garantizar la liberación de recursos (Dispose)
//
// FLUJO DE TRANSACCIÓN:
// BeginTransactionAsync() → Operaciones → CommitAsync() o RollbackAsync()
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Auth;
using back_cabs.CRM.Interfaces.Shared;
using back_cabs.CRM.repositories.Auth;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.Core.UnitOfWork
{
    /// <summary>
    /// Implementación del patrón Unit of Work que coordina transacciones entre múltiples repositorios
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IDbContextTransaction? _transaction;

        // Repositorios lazy-loaded (se crean solo cuando se accede a ellos)
        private IUsuarioAuthRepository? _usuarioRepository;


        /// <summary>
        /// Constructor que inyecta los contextos y fábrica de loggers
        /// </summary>
        /// <param name="writeContext">Contexto de base de datos para operaciones de escritura</param>
        /// <param name="readContext">Contexto de base de datos para operaciones de lectura</param>
        /// <param name="loggerFactory">Fábrica para crear loggers para repositorios</param>
        /// <param name="httpContextAccessor">Accessor para obtener información del usuario autenticado</param>
        public UnitOfWork(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE REPOSITORIOS (LAZY LOADING)
        // ═══════════════════════════════════════════════════════════════

        public IUsuarioAuthRepository Usuarios
        {
            get
            {
                _usuarioRepository ??= new UsuarioAuthRepository(
                    _writeContext,
                    _readContext,
                    _loggerFactory.CreateLogger<UsuarioAuthRepository>());
                return _usuarioRepository;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // OPERACIONES DE PERSISTENCIA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _writeContext.SaveChangesAsync(cancellationToken);
        }

        // ═══════════════════════════════════════════════════════════════
        // MANEJO DE TRANSACCIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Inicia una nueva transacción de base de datos
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Ya existe una transacción activa.");
            }

            _transaction = await _writeContext.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Confirma la transacción actual y guarda todos los cambios
        /// </summary>
        public async Task CommitAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay transacción activa para confirmar.");
            }

            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <summary>
        /// Revierte la transacción actual y descarta todos los cambios
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay transacción activa para revertir.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <summary>
        /// Indica si hay una transacción activa
        /// </summary>
        public bool HasActiveTransaction => _transaction != null;

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS DE UTILIDAD
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Ejecuta una operación dentro de una transacción de forma automática
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            // Si ya hay transacción, ejecutar directamente
            if (HasActiveTransaction)
            {
                return await operation();
            }

            // Si no hay transacción, crear una nueva
            await BeginTransactionAsync();
            try
            {
                var result = await operation();
                await CommitAsync();
                return result;
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una operación dentro de una transacción de forma automática (sin retorno)
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            // Si ya hay transacción, ejecutar directamente
            if (HasActiveTransaction)
            {
                await operation();
                return;
            }

            // Si no hay transacción, crear una nueva
            await BeginTransactionAsync();
            try
            {
                await operation();
                await CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ESTRATEGIAS DE EJECUCIÓN (RESILIENCY)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Ejecuta una operación utilizando la estrategia de ejecución configurada (Resiliency/Retries)
        /// </summary>
        public async Task<T> ExecuteStrategyAsync<T>(Func<Task<T>> operation)
        {
            var strategy = _writeContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(operation);
        }

        /// <summary>
        /// Ejecuta una operación utilizando la estrategia de ejecución configurada (Resiliency/Retries) (sin retorno)
        /// </summary>
        public async Task ExecuteStrategyAsync(Func<Task> operation)
        {
            var strategy = _writeContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(operation);
        }

        // ═══════════════════════════════════════════════════════════════
        // GESTIÓN DE RECURSOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Libera la transacción actual
        /// </summary>
        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Implementación de IDisposable para liberar recursos
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Patrón de dispose protegido
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }

                // NO disponer el contexto aquí, lo maneja el contenedor de DI
                // _context.Dispose();
            }
        }
    }
}
