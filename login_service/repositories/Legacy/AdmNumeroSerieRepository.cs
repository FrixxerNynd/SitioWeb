using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para AdmNumerosSerie
    /// </summary>
    public class AdmNumeroSerieRepository : IAdmNumeroSerieRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmNumeroSerieRepository> _logger;

        public AdmNumeroSerieRepository(
            LegacyCompacReadOnlyContext context,
            ILogger<AdmNumeroSerieRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Busca números de serie por producto y/o número de serie
        /// </summary>
        public async Task<List<AdmNumeroSerie>> SearchAsync(int? idProducto, string? numeroSerie)
        {
            try
            {
                var query = _context.AdmNumerosSerie
                    .AsNoTracking();

                // Aplicar filtros
                if (idProducto.HasValue)
                {
                    query = query.Where(ns => ns.CIdProducto == idProducto.Value);
                }

                if (!string.IsNullOrWhiteSpace(numeroSerie))
                {
                    var searchTerm = numeroSerie.Trim().ToLower();
                    query = query.Where(ns => ns.CNumeroSerie.ToLower().Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(ns => ns.CNumeroSerie)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar números de serie. IdProducto: {IdProducto}, NumeroSerie: {NumeroSerie}",
                    idProducto, numeroSerie);
                throw;
            }
        }
    }
}
