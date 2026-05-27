// =====================================================================================
// SERVICE ADM ALMACEN - AdmAlmacenService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa lógica de negocio para almacenes legacy sin cache Redis.
// Catálogo pequeño y estable que no requiere cache.
//
// PROPÓSITO:
// - Lógica de negocio para almacenes legacy
// - Consulta directa sin cache (catálogo pequeño)
// - Mapeo de entidades a DTOs
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio de lógica de negocio para almacenes legacy sin cache
    /// Implementa consulta directa para admAlmacenes de adCABS2016
    /// </summary>
    public class AdmAlmacenService : IAdmAlmacenService
    {
        private readonly IAdmAlmacenRepository _repository;
        private readonly ILogger<AdmAlmacenService> _logger;

        public AdmAlmacenService(
            IAdmAlmacenRepository repository,
            ILogger<AdmAlmacenService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL
        // ═══════════════════════════════════════════════════════════════

        public async Task<List<AdmAlmacenResponseDto>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Consultando todos los almacenes legacy");

                var almacenes = await _repository.GetAllAsync();
                var almacenesDto = almacenes.Select(MapToDto).ToList();

                _logger.LogInformation("✅ Consulta exitosa: {Count} almacenes legacy obtenidos",
                    almacenesDto.Count);

                return almacenesDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener almacenes legacy");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MAPEO: ENTIDAD -> DTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Mapea entidad AdmAlmacen a DTO de respuesta
        /// </summary>
        private static AdmAlmacenResponseDto MapToDto(AdmAlmacen almacen)
        {
            return new AdmAlmacenResponseDto
            {
                IdAlmacen = almacen.CIdAlmacen,
                CodigoAlmacen = almacen.CCodigoAlmacen,
                NombreAlmacen = almacen.CNombreAlmacen,
                FechaAltaAlmacen = almacen.CFechaAltaAlmacen,
                SegContAlmacen = almacen.CSegContAlmacen,
                Clasificacion1 = almacen.CIdValorClasificacion1,
                Clasificacion2 = almacen.CIdValorClasificacion2,
                Clasificacion3 = almacen.CIdValorClasificacion3,
                Clasificacion4 = almacen.CIdValorClasificacion4,
                Clasificacion5 = almacen.CIdValorClasificacion5,
                Clasificacion6 = almacen.CIdValorClasificacion6,
                TextoExtra1 = almacen.CTextoExtra1,
                TextoExtra2 = almacen.CTextoExtra2,
                TextoExtra3 = almacen.CTextoExtra3,
                FechaExtra = almacen.CFechaExtra,
                ImporteExtra1 = almacen.CImporteExtra1,
                ImporteExtra2 = almacen.CImporteExtra2,
                ImporteExtra3 = almacen.CImporteExtra3,
                ImporteExtra4 = almacen.CImporteExtra4,
                BanDomicilio = almacen.CBanDomicilio,
                Timestamp = almacen.CTimestamp,
                ScAlmac2 = almacen.CScAlmac2,
                ScAlmac3 = almacen.CScAlmac3,
                SistOrig = almacen.CSistOrig
            };
        }
    }
}