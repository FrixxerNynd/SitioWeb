using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para acceso a datos de movimientos serie
    /// </summary>
    public class AdmMovimientoSerieRepository : IAdmMovimientoSerieRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmMovimientoSerieRepository> _logger;

        public AdmMovimientoSerieRepository(
            LegacyCompacReadOnlyContext context,
            ILogger<AdmMovimientoSerieRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los movimientos serie con paginación
        /// </summary>
        public async Task<(List<AdmMovimientoSerie> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize)
        {
            try
            {
                var query = _context.AdmMovimientosSerie
                    .AsNoTracking()
                    .Include(ms => ms.NumeroSerie); // Incluir datos del número de serie

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(ms => ms.CFecha)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimientos serie paginados");
                throw;
            }
        }

        /// <summary>
        /// Obtener movimiento serie por ID autoincrementable
        /// </summary>
        public async Task<AdmMovimientoSerie?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.AdmMovimientosSerie
                    .AsNoTracking()
                    .Include(ms => ms.NumeroSerie)
                    .FirstOrDefaultAsync(ms => ms.CIdAutoIncSql == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimiento serie por ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Obtener movimientos serie por ID de movimiento
        /// </summary>
        public async Task<List<AdmMovimientoSerie>> GetByMovimientoIdAsync(int idMovimiento)
        {
            try
            {
                return await _context.AdmMovimientosSerie
                    .AsNoTracking()
                    .Include(ms => ms.NumeroSerie)
                    .Where(ms => ms.CIdMovimiento == idMovimiento)
                    .OrderBy(ms => ms.CFecha)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimientos serie por ID movimiento: {IdMovimiento}", idMovimiento);
                throw;
            }
        }

        /// <summary>
        /// Obtener movimientos serie por ID de serie
        /// </summary>
        public async Task<List<AdmMovimientoSerie>> GetBySerieIdAsync(int idSerie)
        {
            try
            {
                return await _context.AdmMovimientosSerie
                    .AsNoTracking()
                    .Include(ms => ms.NumeroSerie)
                    .Where(ms => ms.CIdSerie == idSerie)
                    .OrderByDescending(ms => ms.CFecha)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimientos serie por ID serie: {IdSerie}", idSerie);
                throw;
            }
        }

        /// <summary>
        /// Obtener movimientos serie por rango de fechas
        /// </summary>
        public async Task<List<AdmMovimientoSerie>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return await _context.AdmMovimientosSerie
                    .AsNoTracking()
                    .Include(ms => ms.NumeroSerie)
                    .Where(ms => ms.CFecha >= fechaInicio && ms.CFecha <= fechaFin)
                    .OrderByDescending(ms => ms.CFecha)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimientos serie por rango de fechas");
                throw;
            }
        }
    }
}
