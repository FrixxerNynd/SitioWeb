using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el repositorio de movimientos serie
    /// </summary>
    public interface IAdmMovimientoSerieRepository
    {
        /// <summary>
        /// Obtener todos los movimientos serie con paginación
        /// </summary>
        Task<(List<AdmMovimientoSerie> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Obtener movimiento serie por ID
        /// </summary>
        Task<AdmMovimientoSerie?> GetByIdAsync(int id);

        /// <summary>
        /// Obtener movimientos serie por ID de movimiento
        /// </summary>
        Task<List<AdmMovimientoSerie>> GetByMovimientoIdAsync(int idMovimiento);

        /// <summary>
        /// Obtener movimientos serie por ID de serie
        /// </summary>
        Task<List<AdmMovimientoSerie>> GetBySerieIdAsync(int idSerie);

        /// <summary>
        /// Obtener movimientos serie por rango de fechas
        /// </summary>
        Task<List<AdmMovimientoSerie>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
