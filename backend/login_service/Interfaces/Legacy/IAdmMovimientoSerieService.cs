using back_cabs.CRM.DTOs.Legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el servicio de movimientos serie
    /// </summary>
    public interface IAdmMovimientoSerieService
    {
        /// <summary>
        /// Obtener todos los movimientos serie con paginación
        /// </summary>
        Task<(List<AdmMovimientoSerieResponseDto> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Obtener movimiento serie por ID
        /// </summary>
        Task<AdmMovimientoSerieResponseDto?> GetByIdAsync(int id);

        /// <summary>
        /// Obtener movimientos serie por ID de movimiento
        /// </summary>
        Task<List<AdmMovimientoSerieResponseDto>> GetByMovimientoIdAsync(int idMovimiento);

        /// <summary>
        /// Obtener movimientos serie por ID de serie
        /// </summary>
        Task<List<AdmMovimientoSerieResponseDto>> GetBySerieIdAsync(int idSerie);

        /// <summary>
        /// Obtener movimientos serie por rango de fechas
        /// </summary>
        Task<List<AdmMovimientoSerieResponseDto>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
