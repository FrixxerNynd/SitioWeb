using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio de lógica de negocio para movimientos serie
    /// </summary>
    public class AdmMovimientoSerieService : IAdmMovimientoSerieService
    {
        private readonly IAdmMovimientoSerieRepository _repository;
        private readonly ILogger<AdmMovimientoSerieService> _logger;

        public AdmMovimientoSerieService(
            IAdmMovimientoSerieRepository repository,
            ILogger<AdmMovimientoSerieService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los movimientos serie con paginación
        /// </summary>
        public async Task<(List<AdmMovimientoSerieResponseDto> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo movimientos serie. Página: {Page}, Tamaño: {PageSize}", page, pageSize);
                
                var (items, totalCount) = await _repository.GetAllPaginatedAsync(page, pageSize);
                
                var dtos = items.Select(MapToDto).ToList();
                
                _logger.LogInformation("✅ Se encontraron {Count} movimientos serie de {Total} totales", dtos.Count, totalCount);
                return (dtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimientos serie paginados");
                throw;
            }
        }

        /// <summary>
        /// Obtener movimiento serie por ID
        /// </summary>
        public async Task<AdmMovimientoSerieResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando movimiento serie ID: {Id}", id);
                
                var movimientoSerie = await _repository.GetByIdAsync(id);
                
                if (movimientoSerie == null)
                {
                    _logger.LogWarning("⚠️ No se encontró movimiento serie con ID: {Id}", id);
                    return null;
                }

                _logger.LogInformation("✅ Movimiento serie encontrado");
                return MapToDto(movimientoSerie);
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
        public async Task<List<AdmMovimientoSerieResponseDto>> GetByMovimientoIdAsync(int idMovimiento)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando movimientos serie para movimiento ID: {IdMovimiento}", idMovimiento);
                
                var movimientosSerie = await _repository.GetByMovimientoIdAsync(idMovimiento);
                
                _logger.LogInformation("✅ Se encontraron {Count} números de serie para el movimiento", movimientosSerie.Count);
                return movimientosSerie.Select(MapToDto).ToList();
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
        public async Task<List<AdmMovimientoSerieResponseDto>> GetBySerieIdAsync(int idSerie)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando movimientos para número de serie ID: {IdSerie}", idSerie);
                
                var movimientosSerie = await _repository.GetBySerieIdAsync(idSerie);
                
                _logger.LogInformation("✅ Se encontraron {Count} movimientos para el número de serie", movimientosSerie.Count);
                return movimientosSerie.Select(MapToDto).ToList();
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
        public async Task<List<AdmMovimientoSerieResponseDto>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando movimientos serie entre {FechaInicio:yyyy-MM-dd} y {FechaFin:yyyy-MM-dd}", 
                    fechaInicio, fechaFin);
                
                var movimientosSerie = await _repository.GetByDateRangeAsync(fechaInicio, fechaFin);
                
                _logger.LogInformation("✅ Se encontraron {Count} movimientos serie en el rango de fechas", movimientosSerie.Count);
                return movimientosSerie.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimientos serie por rango de fechas");
                throw;
            }
        }

        /// <summary>
        /// Mapear entidad a DTO
        /// </summary>
        private AdmMovimientoSerieResponseDto MapToDto(AdmMovimientoSerie movimientoSerie)
        {
            return new AdmMovimientoSerieResponseDto
            {
                Id = movimientoSerie.CIdAutoIncSql,
                IdMovimiento = movimientoSerie.CIdMovimiento,
                IdSerie = movimientoSerie.CIdSerie,
                Fecha = movimientoSerie.CFecha,
                NumeroSerie = movimientoSerie.NumeroSerie?.CNumeroSerie ?? "Sin número de serie",
                Pedimento = movimientoSerie.NumeroSerie?.CPedimento ?? string.Empty,
                Aduana = movimientoSerie.NumeroSerie?.CAduana ?? string.Empty,
                Lote = movimientoSerie.NumeroSerie?.CNumeroLote ?? string.Empty
            };
        }
    }
}
