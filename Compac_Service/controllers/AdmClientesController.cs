using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace back_cabs.CRM.controllers.Legacy
{
    /// <summary>
    /// Controlador para AdmClientes con Domicilios - Legacy Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CLIENTE,USUARIO")]
    public class AdmClientesController : ControllerBase
    {
        private readonly IAdmClienteService _service;
        private readonly ILogger<AdmClientesController> _logger;

        public AdmClientesController(
            IAdmClienteService service,
            ILogger<AdmClientesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Busca clientes con domicilios aplicando filtros con paginación
        /// </summary>
        /// <remarks>
        /// Filtros disponibles:
        /// - codigoCliente: Código del cliente (búsqueda parcial)
        /// - razonSocial: Razón social/nombre (búsqueda parcial)
        /// - rfc: RFC del cliente (búsqueda parcial)
        /// - email: Email (búsqueda parcial)
        /// - telefono: Teléfono (búsqueda parcial)
        /// - estado: Estado del domicilio (búsqueda parcial)
        /// - ciudad: Ciudad del domicilio (búsqueda parcial)
        /// - estatus: 1=Activo, 0=Inactivo, null=Todos (default: 1)
        /// - tipoDireccion: 1=Fiscal, 2=Envío, 3=Otros, null=Todas (default: 1)
        /// - incluirDetalleUbicacion: true/false (default: true)
        /// - numeroPagina: Número de página (default: 1)
        /// - tamanoPagina: Registros por página (default: 50, max: 100)
        /// 
        /// Ejemplo:
        /// GET /api/AdmClientes/search?razonSocial=COMPUTACION&estatus=1&incluirDetalleUbicacion=true
        /// </remarks>
        [HttpGet("search")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] AdmClienteFilterDto filter)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Validación
                if (filter.TamanoPagina > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El tamaño de página no puede ser mayor a 100"
                    });
                }

                if (filter.NumeroPagina < 1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El número de página debe ser mayor a 0"
                    });
                }

                var (clientes, totalRegistros, totalPaginas) = await _service.SearchPaginatedAsync(filter);

                sw.Stop();
                _logger.LogInformation(
                    "✅ GET /api/AdmClientes/search completado en {Ms}ms. Página {Page}/{TotalPages}, Total: {Total}",
                    sw.ElapsedMilliseconds, filter.NumeroPagina, totalPaginas, totalRegistros);

                return Ok(new
                {
                    success = true,
                    data = clientes,
                    pagination = new
                    {
                        currentPage = filter.NumeroPagina,
                        pageSize = filter.TamanoPagina,
                        totalPages = totalPaginas,
                        totalRecords = totalRegistros,
                        hasNextPage = filter.NumeroPagina < totalPaginas,
                        hasPreviousPage = filter.NumeroPagina > 1
                    },
                    filters = new
                    {
                        codigoCliente = filter.CodigoCliente,
                        razonSocial = filter.RazonSocial,
                        rfc = filter.RFC,
                        email = filter.Email,
                        telefono = filter.Telefono,
                        estado = filter.Estado,
                        ciudad = filter.Ciudad,
                        estatus = filter.Estatus,
                        tipoDireccion = filter.TipoDireccion,
                        incluirDetalleUbicacion = filter.IncluirDetalleUbicacion
                    },
                    message = $"Se encontraron {totalRegistros} cliente(s)"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error en búsqueda de clientes después de {Ms}ms", sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al buscar clientes",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene un cliente por ID con su domicilio
        /// </summary>
        /// <param name="id">ID del cliente (CIDCLIENTEPROVEEDOR)</param>
        /// <param name="incluirDetalleUbicacion">Incluir detalle completo de ubicación (default: true)</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id, [FromQuery] bool incluirDetalleUbicacion = true)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var cliente = await _service.GetByIdAsync(id, incluirDetalleUbicacion);

                if (cliente == null)
                {
                    sw.Stop();
                    _logger.LogWarning("⚠️ GET /api/AdmClientes/{Id} - Cliente no encontrado después de {Ms}ms",
                        id, sw.ElapsedMilliseconds);

                    return NotFound(new
                    {
                        success = false,
                        message = $"Cliente con ID {id} no encontrado"
                    });
                }

                sw.Stop();
                _logger.LogInformation("✅ GET /api/AdmClientes/{Id} completado en {Ms}ms",
                    id, sw.ElapsedMilliseconds);

                return Ok(new
                {
                    success = true,
                    data = cliente,
                    message = "Cliente encontrado exitosamente"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener cliente {Id} después de {Ms}ms",
                    id, sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener cliente",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de clientes
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStats()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Clientes activos
                var (activos, totalActivos, _) = await _service.SearchPaginatedAsync(new AdmClienteFilterDto
                {
                    Estatus = 1,
                    NumeroPagina = 1,
                    TamanoPagina = 1,
                    IncluirDetalleUbicacion = false
                });

                // Clientes inactivos
                var (inactivos, totalInactivos, _) = await _service.SearchPaginatedAsync(new AdmClienteFilterDto
                {
                    Estatus = 0,
                    NumeroPagina = 1,
                    TamanoPagina = 1,
                    IncluirDetalleUbicacion = false
                });

                // Todos los clientes
                var (todos, totalTodos, _) = await _service.SearchPaginatedAsync(new AdmClienteFilterDto
                {
                    Estatus = null,
                    NumeroPagina = 1,
                    TamanoPagina = 1,
                    IncluirDetalleUbicacion = false
                });

                sw.Stop();
                _logger.LogInformation("✅ GET /api/AdmClientes/stats completado en {Ms}ms", sw.ElapsedMilliseconds);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalClientes = totalTodos,
                        clientesActivos = totalActivos,
                        clientesInactivos = totalInactivos,
                        porcentajeActivos = totalTodos > 0 ? Math.Round((totalActivos / (double)totalTodos) * 100, 2) : 0
                    },
                    message = "Estadísticas obtenidas exitosamente"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener estadísticas después de {Ms}ms", sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener estadísticas",
                    error = ex.Message
                });
            }
        }

        ///<summary>
        /// Endpoint para obtener el limite de credito, documentos y dias.
        /// </summary>
        [HttpGet("credit/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCreditAsync(int id)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var credit = _service.GetCreditByIdAsync(id);
                if (credit == null)
                {
                    sw.Stop();
                    _logger.LogWarning("GET /api/credit/{id} - Cliente no encontrado despues de {Ms}ms", id, sw.ElapsedMilliseconds);
                    return NotFound(new
                    {
                        success = false,
                        message = $"Cliente con ID {id} no encontrado"
                    });
                }
                sw.Stop();
                _logger.LogInformation(" GET /api/AdmClientes/credit/{id} -  Completado en {Ms}ms", id, sw.ElapsedMilliseconds);
                return Ok(new
                {
                    success = true,
                    data = credit,
                    message = "Crédito del cliente obtenido exitosamente"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error al obtener el credito del cliente {Id} en {Ms}ms", id, sw.ElapsedMilliseconds);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener el credito del cliente",
                    error = ex.Message
                });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ENDPOINT AUXILIAR PARA COTIZACIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca clientes por Razón Social o RFC (para uso en cotizaciones)
        /// </summary>
        /// <remarks>
        /// Endpoint simplificado para búsqueda rápida de clientes al crear cotizaciones.
        /// Busca coincidencias parciales en Razón Social o RFC.
        /// 
        /// **Ejemplos de uso:**
        /// - GET /api/AdmClientes/buscar?texto=COMPUTACION
        /// - GET /api/AdmClientes/buscar?texto=ABC123456
        /// 
        /// **Respuesta:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "data": [
        ///     {
        ///       "idCliente": 123,
        ///       "codigoCliente": "C001",
        ///       "razonSocial": "COMPUTACION Y SISTEMAS SA DE CV",
        ///       "rfc": "CSI950101ABC",
        ///       "email": "contacto@computacion.com",
        ///       "telefono": "5551234567"
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="texto">Texto a buscar (Razón Social o RFC)</param>
        /// <param name="limit">Límite de resultados (default: 20, max: 50)</param>
        /// <returns>Lista de clientes que coinciden con la búsqueda</returns>
        [HttpGet("buscar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BuscarParaCotizacion(
            [FromQuery] string texto,
            [FromQuery] int limit = 20)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (string.IsNullOrWhiteSpace(texto))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El parámetro 'texto' es obligatorio"
                    });
                }

                if (limit > 50)
                {
                    limit = 50;
                }

                _logger.LogInformation("🔍 Buscando clientes con texto: {Texto}, Límite: {Limit}", texto, limit);

                // Buscar por razón social o RFC
                var filter = new AdmClienteFilterDto
                {
                    RazonSocial = texto,
                    Estatus = 1, // Solo activos
                    NumeroPagina = 1,
                    TamanoPagina = limit,
                    IncluirDetalleUbicacion = true
                };

                var (clientes, totalRegistros, _) = await _service.SearchPaginatedAsync(filter);

                // Si no encontró por razón social, buscar por RFC
                if (clientes.Count == 0)
                {
                    filter.RazonSocial = null;
                    filter.RFC = texto;
                    (clientes, totalRegistros, _) = await _service.SearchPaginatedAsync(filter);
                }

                sw.Stop();
                _logger.LogInformation("✅ Búsqueda completada en {Ms}ms. Encontrados: {Total}",
                    sw.ElapsedMilliseconds, clientes.Count);

                // Simplificar respuesta para cotizaciones
                var resultadosSimplificados = clientes.Select(c => new
                {
                    idCliente = c.Id,
                    codigoCliente = c.CodigoCliente,
                    razonSocial = c.Nombre,
                    rfc = c.RFC,
                    email = c.Email,
                    telefono = c.Telefono,
                    ubicacion = c.Ubicacion

                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = resultadosSimplificados,
                    totalEncontrados = clientes.Count,
                    textoBuscado = texto,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al buscar clientes después de {Ms}ms. Texto: {Texto}",
                    sw.ElapsedMilliseconds, texto);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al buscar clientes",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
        }
    }
}
