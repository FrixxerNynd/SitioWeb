using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para acceso a datos de unidades de medida y peso
    /// </summary>
    public class AdmUnidadMedidaPesoRepository : IAdmUnidadMedidaPesoRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmUnidadMedidaPesoRepository> _logger;

        public AdmUnidadMedidaPesoRepository(
            LegacyCompacReadOnlyContext context,
            ILogger<AdmUnidadMedidaPesoRepository> logger)
        {
            _context = context;
            _logger = logger;   
        }

        
        /// <summary>
        /// Obtener todas las unidades de medida
        /// </summary>
        public async Task<List<AdmUnidadMedidaPeso>> GetAllAsync()
        {
            try
            {
                return await _context.AdmUnidadesMedidaPeso
                    .AsNoTracking()
                    .OrderBy(u => u.CNombreUnidad)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener todas las unidades de medida");
                throw;
            }
        }

        /// <summary>
        /// Obtener unidad por ID
        /// </summary>
        public async Task<AdmUnidadMedidaPeso?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.AdmUnidadesMedidaPeso
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.CIdUnidad == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener unidad de medida por ID: {Id}", id);
                throw;
            }
        }

       
       
        /// <summary>
        /// Buscar unidades por nombre (búsqueda parcial case-insensitive)
        /// </summary>
        public async Task<List<AdmUnidadMedidaPeso>> SearchByNameAsync(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    return await GetAllAsync();

                var nombreLower = nombre.Trim().ToLower();

                return await _context.AdmUnidadesMedidaPeso
                    .AsNoTracking()
                    .Where(u => u.CNombreUnidad.ToLower().Contains(nombreLower))
                    .OrderBy(u => u.CNombreUnidad)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar unidades por nombre: {Nombre}", nombre);
                throw;
            }
        }
    }
}
