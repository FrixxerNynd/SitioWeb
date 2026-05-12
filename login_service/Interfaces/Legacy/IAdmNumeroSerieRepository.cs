using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el repositorio de AdmNumerosSerie
    /// </summary>
    public interface IAdmNumeroSerieRepository
    {
        /// <summary>
        /// Busca números de serie por producto y/o número de serie
        /// </summary>
        Task<List<AdmNumeroSerie>> SearchAsync(int? idProducto, string? numeroSerie);
    }
}
