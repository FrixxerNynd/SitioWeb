using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el repositorio de unidades de medida y peso
    /// </summary>
    public interface IAdmUnidadMedidaPesoRepository
    {
        /// <summary>
        /// Obtener todas las unidades de medida
        /// </summary>
        Task<List<AdmUnidadMedidaPeso>> GetAllAsync();

        /// <summary>
        /// Obtener unidad por ID
        /// </summary>
        Task<AdmUnidadMedidaPeso?> GetByIdAsync(int id);

        /// <summary>
        /// Buscar unidades por nombre (búsqueda parcial)
        /// </summary>
        Task<List<AdmUnidadMedidaPeso>> SearchByNameAsync(string nombre);
    }
}
