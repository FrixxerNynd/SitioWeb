using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio para AdmNumerosSerie
    /// </summary>
    public class AdmNumeroSerieService : IAdmNumeroSerieService
    {
        private readonly IAdmNumeroSerieRepository _repository;
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmNumeroSerieService> _logger;

        public AdmNumeroSerieService(
            IAdmNumeroSerieRepository repository,
            LegacyCompacReadOnlyContext context,
            ILogger<AdmNumeroSerieService> logger)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Busca números de serie por producto y/o número de serie
        /// </summary>
        public async Task<List<AdmNumeroSerieResponseDto>> SearchAsync(int? idProducto, string? numeroSerie)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando números de serie. IdProducto: {IdProducto}, NumeroSerie: {NumeroSerie}",
                    idProducto, numeroSerie);

                var numerosSerie = await _repository.SearchAsync(idProducto, numeroSerie);

                // Por ahora, no cargamos información relacionada para evitar problemas con EF Core y SQL Server
                // TODO: Implementar carga de productos/almacenes de manera más eficiente
                var result = numerosSerie.Select(MapToDto).ToList();

                _logger.LogInformation("✅ Búsqueda de números de serie completada. Total: {Total}",
                    result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar números de serie");
                throw;
            }
        }

        /// <summary>
        /// Mapea AdmNumeroSerie a DTO
        /// </summary>
        private AdmNumeroSerieResponseDto MapToDto(models.legacy.AdmNumeroSerie entity)
        {
            return new AdmNumeroSerieResponseDto
            {
                IdSerie = entity.CIdSerie,
                IdProducto = entity.CIdProducto,
                NumeroSerie = entity.CNumeroSerie,
                IdAlmacen = entity.CIdAlmacen,
                Estado = entity.CEstado,
                EstadoAnterior = entity.CEstadoAnterior,
                NumeroLote = entity.CNumeroLote,
                FechaCaducidad = entity.CFechaCaducidad,
                FechaFabricacion = entity.CFechaFabricacion,
                Pedimento = entity.CPedimento,
                Aduana = entity.CAduana,
                FechaPedimento = entity.CFechaPedimento,
                TipoCambio = entity.CTipoCambio,
                Costo = entity.CCosto,
                Timestamp = entity.CTimestamp,
                NumAduana = entity.CNumAduana,
                ClaveSat = entity.CClaveSat,

                // Información relacionada - por ahora null hasta implementar carga eficiente
                CodigoProducto = null,
                NombreProducto = null,
                CodigoAlmacen = null,
                NombreAlmacen = null
            };
        }
    }
}
