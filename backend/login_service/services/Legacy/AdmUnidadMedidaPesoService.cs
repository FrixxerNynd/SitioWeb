using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio de lógica de negocio para unidades de medida y peso
    /// </summary>
    public class AdmUnidadMedidaPesoService : IAdmUnidadMedidaPesoService
    {
        private readonly IAdmUnidadMedidaPesoRepository _repository;
        private readonly ILogger<AdmUnidadMedidaPesoService> _logger;

        public AdmUnidadMedidaPesoService(
            IAdmUnidadMedidaPesoRepository repository,
            ILogger<AdmUnidadMedidaPesoService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las unidades de medida
        /// </summary>
        public async Task<List<AdmUnidadMedidaPesoResponseDto>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo todas las unidades de medida");
                var unidades = await _repository.GetAllAsync();
                
                _logger.LogInformation("✅ Se encontraron {Count} unidades de medida", unidades.Count);
                return unidades.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener todas las unidades");
                throw;
            }
        }

        /// <summary>
        /// Obtener unidad por ID
        /// </summary>
        public async Task<AdmUnidadMedidaPesoResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando unidad de medida ID: {Id}", id);
                var unidad = await _repository.GetByIdAsync(id);
                
                if (unidad == null)
                {
                    _logger.LogWarning("⚠️ No se encontró unidad con ID: {Id}", id);
                    return null;
                }

                _logger.LogInformation("✅ Unidad encontrada: {Nombre}", unidad.CNombreUnidad);
                return MapToDto(unidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener unidad por ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Buscar unidades por nombre
        /// </summary>
        public async Task<List<AdmUnidadMedidaPesoResponseDto>> SearchByNameAsync(string nombre)
        {
            try
            {
                _logger.LogInformation("🔍 Buscando unidades por nombre: {Nombre}", nombre);
                var unidades = await _repository.SearchByNameAsync(nombre);
                
                _logger.LogInformation("✅ Se encontraron {Count} unidades", unidades.Count);
                return unidades.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar unidades por nombre: {Nombre}", nombre);
                throw;
            }
        }

        /// <summary>
        /// Mapear entidad a DTO
        /// </summary>
        private AdmUnidadMedidaPesoResponseDto MapToDto(AdmUnidadMedidaPeso unidad)
        {
            return new AdmUnidadMedidaPesoResponseDto
            {
                Id = unidad.CIdUnidad,
                Nombre = unidad.CNombreUnidad,
                Abreviatura = unidad.CAbreviatura,
                Despliegue = unidad.CDespliegue,
                ClaveInterna = unidad.CClaveInt,
                ClaveSat = unidad.CClaveSat
            };
        }
    }
}
