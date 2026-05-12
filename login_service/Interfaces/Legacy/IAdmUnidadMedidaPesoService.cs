using back_cabs.CRM.DTOs.Legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el servicio de unidades de medida y peso
    /// </summary>
    public interface IAdmUnidadMedidaPesoService
    {
        /// <summary>
        /// Obtener todas las unidades de medida
        /// </summary>
        Task<List<AdmUnidadMedidaPesoResponseDto>> GetAllAsync();

        /// <summary>
        /// Obtener unidad por ID
        /// </summary>
        Task<AdmUnidadMedidaPesoResponseDto?> GetByIdAsync(int id);

        /// <summary>
        /// Buscar unidades por nombre
        /// </summary>
        Task<List<AdmUnidadMedidaPesoResponseDto>> SearchByNameAsync(string nombre);
    }
}
