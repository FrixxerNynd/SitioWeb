using back_cabs.CRM.DTOs.Legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el servicio de AdmNumerosSerie
    /// </summary>
    public interface IAdmNumeroSerieService
    {
        /// <summary>
        /// Busca números de serie por producto y/o número de serie
        /// </summary>
        Task<List<AdmNumeroSerieResponseDto>> SearchAsync(int? idProducto, string? numeroSerie);
    }
}
