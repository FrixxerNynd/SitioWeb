// =====================================================================================
// SERVICE ADM CONCEPTO - AdmConceptoService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa lógica de negocio para conceptos legacy sin cache Redis.
// Catálogo pequeño (~89 registros) que no requiere cache.
//
// PROPÓSITO:
// - Lógica de negocio para conceptos legacy
// - Consulta directa sin cache (catálogo pequeño)
// - Paginación y búsqueda optimizadas
// - Mapeo de entidades a DTOs
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using CRM.DTOs.Response;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio de lógica de negocio para conceptos legacy sin cache
    /// Implementa consulta directa para admConceptos de adCABS2016
    /// </summary>
    public class AdmConceptoService : IAdmConceptoService
    {
        private readonly IAdmConceptoRepository _repository;
        private readonly ILogger<AdmConceptoService> _logger;

        public AdmConceptoService(
            IAdmConceptoRepository repository,
            ILogger<AdmConceptoService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO
        // ═══════════════════════════════════════════════════════════════

        public async Task<PaginatedResponseDto<AdmConceptoResponseDto>> GetAllPaginatedAsync(int page, int pageSize)
        {
            try
            {
                // Validación de parámetros
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;
                if (pageSize > 100) pageSize = 100;

                _logger.LogInformation("🔍 Consultando conceptos legacy página {Page} tamaño {PageSize}", page, pageSize);

                var (data, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize);
                var conceptosDto = data.Select(MapToDto).ToList();

                var paginatedResponse = new PaginatedResponseDto<AdmConceptoResponseDto>
                {
                    Items = conceptosDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };

                _logger.LogInformation("✅ Consulta exitosa: {Count} de {TotalRecords} conceptos legacy (página {Page})",
                    conceptosDto.Count, totalRecords, page);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener conceptos legacy paginados (Página {Page}, Tamaño {PageSize})",
                    page, pageSize);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA PAGINADA
        // ═══════════════════════════════════════════════════════════════

        public async Task<PaginatedResponseDto<AdmConceptoResponseDto>> SearchPaginatedAsync(
            string searchTerm,
            int page,
            int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Término de búsqueda vacío, retornando lista vacía");
                    return new PaginatedResponseDto<AdmConceptoResponseDto>
                    {
                        Items = new List<AdmConceptoResponseDto>(),
                        Pagina = page,
                        ResultadosPorPagina = pageSize,
                        TotalItems = 0
                    };
                }

                // Validación de parámetros
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;
                if (pageSize > 100) pageSize = 100;

                _logger.LogInformation("🔍 Buscando conceptos legacy: '{SearchTerm}' página {Page} tamaño {PageSize}",
                    searchTerm, page, pageSize);

                var (data, totalRecords) = await _repository.SearchPaginatedAsync(searchTerm, page, pageSize);
                var conceptosDto = data.Select(MapToDto).ToList();

                var paginatedResponse = new PaginatedResponseDto<AdmConceptoResponseDto>
                {
                    Items = conceptosDto,
                    Pagina = page,
                    ResultadosPorPagina = pageSize,
                    TotalItems = totalRecords
                };

                _logger.LogInformation("✅ Búsqueda exitosa: {Count} de {TotalRecords} conceptos legacy encontrados ('{SearchTerm}', página {Page})",
                    conceptosDto.Count, totalRecords, searchTerm, page);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar conceptos legacy ('{SearchTerm}', Página {Page}, Tamaño {PageSize})",
                    searchTerm, page, pageSize);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MAPEO: ENTIDAD -> DTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Mapea entidad AdmConcepto a DTO de respuesta
        /// Solo incluye campos principales para optimizar performance
        /// </summary>
        private static AdmConceptoResponseDto MapToDto(AdmConcepto concepto)
        {
            return new AdmConceptoResponseDto
            {
                IdConceptoDocumento = concepto.CIdConceptoDocumento,
                CodigoConcepto = concepto.CCodigoConcepto,
                NombreConcepto = concepto.CNombreConcepto,
                PrefijoConcepto = concepto.CPrefijoConcepto,
                IdDocumentoDe = concepto.CIdDocumentoDe,
                IdMoneda = concepto.CIdMoneda,
                Naturaleza = concepto.CNaturaleza,
                TipoFolio = concepto.CTipoFolio,
                NoFolio = concepto.CNoFolio,
                SeriePorOmision = concepto.CSeriePorOmision,
                MaximoMovtos = concepto.CMaximoMovtos,
                UsaNombreCteProv = concepto.CUsaNombreCteProv,
                UsaRfc = concepto.CUsaRfc,
                UsaFechaVencimiento = concepto.CUsaFechaVencimiento,
                UsaMoneda = concepto.CUsaMoneda,
                UsaTipoCambio = concepto.CUsaTipoCambio,
                UsaCodigoAgente = concepto.CUsaCodigoAgente,
                UsaDireccion = concepto.CUsaDireccion,
                UsaReferencia = concepto.CUsaReferencia,
                UsaAlmacen = concepto.CUsaAlmacen,
                UsaPrecio = concepto.CUsaPrecio,
                UsaExistencia = concepto.CUsaExistencia,
                UsaObservaciones = concepto.CUsaObservaciones,
                EsCfd = concepto.CEsCfd,
                ClaveSat = concepto.CClaveSat,
                MetodoPago = concepto.CMetodoPag,
                RegimenFiscal = concepto.CRegimFisc,
                PresentaFiscal = concepto.CPresentaFiscal,
                PresentaReferencia = concepto.CPresentaReferencia,
                PresentaDetalle = concepto.CPresentaDetalle,
                PresentaImprimir = concepto.CPresentaImprimir,
                OrdenCalculo = concepto.COrdenCalculo,
                CreaCliente = concepto.CCreaCliente,
                DoctoACredito = concepto.CDoctoACredito,
                Estatus = concepto.CEstatus,
                Timestamp = concepto.CTimestamp,
                SegContConcepto = concepto.CSegContConcepto
            };
        }
    }
}