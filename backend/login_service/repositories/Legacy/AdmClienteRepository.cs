using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para clientes de Adminpaq con domicilios
    /// </summary>
    public class AdmClienteRepository : IAdmClienteRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly LegacyCompacWriteContext _writeContext;
        private readonly ILogger<AdmClienteRepository> _logger;

        public AdmClienteRepository(LegacyCompacReadOnlyContext context, LegacyCompacWriteContext writeContext, ILogger<AdmClienteRepository> logger)
        {
            _context = context;
            _writeContext = writeContext;
            _logger = logger;
        }

        ///<summary>
        /// Insertar nuevo domicilio para cliente
        /// </summary>
        public async Task<AdmDomicilio> InsertDomicilioAsync(AdmDomicilio domicilio)
        {
            try
            {
                //Validar que el cliente exista antes de insertar domicilio
                var clienteExistente = await _context.AdmClientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CIdClienteProveedor == domicilio.CIdCatalogo);
                if (clienteExistente == null)
                {
                    _logger.LogWarning("⚠️ No se encontró cliente con ID {IdCliente} para insertar domicilio", domicilio.CIdCatalogo);
                    throw new Exception($"No se encontró cliente con ID {domicilio.CIdCatalogo}");
                }

                //Validar que no exista un domicilio igual para el mismo cliente
                var domicilioExistente = await _context.AdmDomicilios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d =>
                        d.CIdCatalogo == domicilio.CIdCatalogo);
                if (domicilioExistente != null)
                {
                    _logger.LogWarning("⚠️ Ya existe un domicilio para el cliente {IdCliente}", domicilio.CIdCatalogo);
                    throw new Exception($"Ya existe un domicilio para el cliente {domicilio.CIdCatalogo}");
                }

                //Obtener siguiente ID
                var maxId = await _context.AdmDomicilios
                                    .OrderByDescending(d => d.CIdDireccion)
                                    .Select(d => d.CIdDireccion)
                                    .FirstOrDefaultAsync();
                domicilio.CIdDireccion = maxId + 1;

                _writeContext.AdmDomicilios.Add(domicilio);
                var succes = await _writeContext.SaveChangesAsync();
                if (succes == 0)
                {
                    _logger.LogWarning("⚠️ No se insertó ningún domicilio");
                    throw new Exception("No se pudo insertar el domicilio");
                }

                _logger.LogInformation("✅ Domicilio insertado con ID {IdDomicilio}", domicilio.CIdDireccion);
                return domicilio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al insertar domicilio");
                throw;
            }
        }

        /// <summary>
        /// Inserta un nuevo cliente en la base de datos
        /// </summary>
        public async Task<AdmCliente> InsertAsync(AdmCliente cliente)
        {
            try
            {
                //Obtener siguiente ID
                var maxId = await _context.AdmClientes
                                    .OrderByDescending(c => c.CIdClienteProveedor)
                                    .Select(c => c.CIdClienteProveedor)
                                    .FirstOrDefaultAsync();
                cliente.CIdClienteProveedor = maxId + 1;

                _writeContext.AdmClientes.Add(cliente);
                var succes = await _writeContext.SaveChangesAsync();
                if (succes == 0)
                {
                    _logger.LogWarning("⚠️ No se insertó ningún cliente");
                    throw new Exception("No se pudo insertar el cliente");
                }

                _logger.LogInformation("✅ Cliente insertado con ID {IdCliente}", cliente.CIdClienteProveedor);
                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al insertar cliente");
                throw;
            }
        }

        /// <summary>
        /// Actualizar/Editar un cliente en la base de datos
        /// </summary>
        public async Task<AdmCliente> UpdateAsync(AdmCliente cliente)
        {
            try
            {
                _writeContext.AdmClientes.Update(cliente);
                var succes = await _writeContext.SaveChangesAsync();
                if (succes == 0)
                {
                    _logger.LogWarning("⚠️ No se actualizó ningún cliente");
                    throw new Exception("No se pudo actualizar el cliente");
                }

                _logger.LogInformation("✅ Cliente actualizado con ID {IdCliente}", cliente.CIdClienteProveedor);
                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar cliente");
                throw;
            }
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

        /// <summary>
        /// Validar credenciales (soporte legacy)
        /// </summary>
        public async Task<AdmCliente?> ValidateCredentialsAsync(string email, string contrasena)
        {
            try
            {
                var cliente = await _context.AdmClientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        (c.CEmail1.ToLower() == email.ToLower() ||
                         c.CEmail2.ToLower() == email.ToLower() ||
                         c.CEmail3.ToLower() == email.ToLower()) &&
                        c.CTextoExtra1 == contrasena);

                if (cliente != null)
                {
                    _logger.LogInformation("✅ Credenciales válidas para cliente: {Email}", email);
                }
                else
                {
                    _logger.LogWarning("⚠️ Credenciales inválidas para cliente: {Email}", email);
                }

                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al validar credenciales para cliente: {Email}", email);
                throw;
            }
        }
    }
}
