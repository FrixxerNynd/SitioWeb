using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para clientes de Adminpaq con domicilios
    /// </summary>
    public class AdmClienteRepository : IAdmClienteRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmClienteRepository> _logger;

        public AdmClienteRepository(LegacyCompacReadOnlyContext context, ILogger<AdmClienteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Búsqueda paginada con filtros múltiples
        /// </summary>
        public async Task<(List<AdmCliente> Clientes, int TotalRegistros)> SearchPaginatedAsync(AdmClienteFilterDto filter)
        {
            try
            {
                var query = _context.AdmClientes.AsNoTracking();

                // Aplicar filtros de cliente
                if (!string.IsNullOrWhiteSpace(filter.CodigoCliente))
                {
                    var codigoCliente = filter.CodigoCliente.Trim().ToLower();
                    query = query.Where(c => c.CCodigoCliente.ToLower().Contains(codigoCliente));
                }

                if (!string.IsNullOrWhiteSpace(filter.RazonSocial))
                {
                    var razonSocial = filter.RazonSocial.Trim().ToLower();
                    query = query.Where(c => c.CRazonSocial.ToLower().Contains(razonSocial));
                }

                if (!string.IsNullOrWhiteSpace(filter.RFC))
                {
                    var rfc = filter.RFC.Trim().ToLower();
                    query = query.Where(c => c.CRfc.ToLower().Contains(rfc));
                }

                if (!string.IsNullOrWhiteSpace(filter.Email))
                {
                    var email = filter.Email.Trim().ToLower();
                    query = query.Where(c => 
                        c.CEmail1.ToLower().Contains(email) ||
                        c.CEmail2.ToLower().Contains(email) ||
                        c.CEmail3.ToLower().Contains(email)
                    );
                }

                // Nota: Los teléfonos están en AdmDomicilios, no en AdmClientes
                // El filtro de teléfono requeriría un JOIN con AdmDomicilios

                if (filter.Estatus.HasValue)
                {
                    query = query.Where(c => c.CEstatus == filter.Estatus.Value);
                }

                // Si hay filtros de ubicación, hacemos JOIN con domicilios
                if (!string.IsNullOrWhiteSpace(filter.Estado) || !string.IsNullOrWhiteSpace(filter.Ciudad))
                {
                    var domiciliosQuery = _context.AdmDomicilios.AsNoTracking()
                        .Where(d => d.CTipoCatalogo == 1); // 1 = Cliente

                    if (filter.TipoDireccion.HasValue)
                    {
                        domiciliosQuery = domiciliosQuery.Where(d => d.CTipoDireccion == filter.TipoDireccion.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Estado))
                    {
                        var estado = filter.Estado.Trim().ToLower();
                        domiciliosQuery = domiciliosQuery.Where(d => d.CEstado.ToLower().Contains(estado));
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Ciudad))
                    {
                        var ciudad = filter.Ciudad.Trim().ToLower();
                        domiciliosQuery = domiciliosQuery.Where(d => d.CCiudad.ToLower().Contains(ciudad));
                    }

                    var clientesIdsConDomicilio = await domiciliosQuery
                        .Select(d => d.CIdCatalogo)
                        .Distinct()
                        .ToListAsync();

                    // Filtrar clientes que tienen domicilios que cumplen los criterios
                    // EVITAMOS Contains() por OPENJSON, iteramos y filtramos en memoria después de obtener todos los registros
                    query = query.Where(c => clientesIdsConDomicilio.Contains(c.CIdClienteProveedor));
                }

                // Contar total
                var total = await query.CountAsync();

                // Paginación
                var skip = (filter.NumeroPagina - 1) * filter.TamanoPagina;
                var take = Math.Min(filter.TamanoPagina, 100);

                var clientes = await query
                    .OrderBy(c => c.CRazonSocial)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

                _logger.LogInformation("✅ Búsqueda de clientes completada. Total: {Total}, Página: {Pagina}, Tamaño: {Tamanio}",
                    total, filter.NumeroPagina, clientes.Count);

                return (clientes, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar clientes");
                throw;
            }
        }

        /// <summary>
        /// Obtener cliente por ID con domicilio
        /// </summary>
        public async Task<AdmCliente?> GetByIdWithDomicilioAsync(int idCliente, int? tipoDireccion = 1)
        {
            try
            {
                var cliente = await _context.AdmClientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CIdClienteProveedor == idCliente);

                if (cliente == null) return null;

                _logger.LogInformation("✅ Cliente {IdCliente} encontrado: {RazonSocial}", 
                    idCliente, cliente.CRazonSocial);

                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cliente {IdCliente}", idCliente);
                throw;
            }
        }
    }
}
