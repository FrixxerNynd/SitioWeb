using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace back_cabs.CRM.controllers.Legacy
{
    /// <summary>
    /// Controlador para AdmDocumentos (Cotizaciones/Documentos) - Legacy Adminpaq
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class AdmDocumentosController : ControllerBase
    {
        private readonly IAdmDocumentoService _service;
        private readonly ILogger<AdmDocumentosController> _logger;

        public AdmDocumentosController(
            IAdmDocumentoService service,
            ILogger<AdmDocumentosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Busca documentos (cotizaciones) aplicando filtros con paginación
        /// </summary>
        /// <remarks>
        /// Filtros disponibles:
        /// - fechaInicio/fechaFin: Rango de fechas del documento
        /// - folio: Folio del documento (búsqueda exacta)
        /// - razonSocial: Razón social del cliente (búsqueda parcial)
        /// - fechaVencimientoInicio/fechaVencimientoFin: Rango de fechas de vencimiento
        /// - idConcepto: ID del concepto del documento
        /// - idAgente: ID del agente de ventas
        /// - page: Número de página (default: 1)
        /// - pageSize: Registros por página (default: 50, max: 100)
        /// - incluirMovimientos: Incluye los productos cotizados (default: false)
        /// </remarks>
        [HttpGet("search")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] AdmDocumentoFilterDto filter)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Validación básica de fechas
                if (filter.FechaInicio.HasValue && filter.FechaFin.HasValue &&
                    filter.FechaInicio.Value > filter.FechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor que la fecha final"
                    });
                }

                if (filter.FechaVencimientoInicio.HasValue && filter.FechaVencimientoFin.HasValue &&
                    filter.FechaVencimientoInicio.Value > filter.FechaVencimientoFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha de vencimiento inicial no puede ser mayor que la final"
                    });
                }

                var (documentos, totalRegistros) = await _service.SearchPaginatedAsync(filter);

                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)filter.PageSize);

                sw.Stop();
                _logger.LogInformation(
                    "✅ GET /api/AdmDocumentos/search completado en {Ms}ms. Página {Page}/{TotalPages}, Total: {Total}",
                    sw.ElapsedMilliseconds, filter.Page, totalPaginas, totalRegistros);

                return Ok(new
                {
                    success = true,
                    data = documentos,
                    pagination = new
                    {
                        currentPage = filter.Page,
                        pageSize = filter.PageSize,
                        totalPages = totalPaginas,
                        totalRecords = totalRegistros,
                        hasNextPage = filter.Page < totalPaginas,
                        hasPreviousPage = filter.Page > 1
                    },
                    filters = new
                    {
                        fechaInicio = filter.FechaInicio,
                        fechaFin = filter.FechaFin,
                        folio = filter.Folio,
                        razonSocial = filter.RazonSocial,
                        fechaVencimientoInicio = filter.FechaVencimientoInicio,
                        fechaVencimientoFin = filter.FechaVencimientoFin,
                        idConcepto = filter.IdConcepto,
                        idAgente = filter.IdAgente,
                        incluirMovimientos = filter.IncluirMovimientos
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error en búsqueda de documentos después de {Ms}ms",
                    sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al buscar documentos",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene un documento específico por ID incluyendo sus movimientos (productos cotizados)
        /// </summary>
        /// <param name="id">ID del documento</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var documento = await _service.GetByIdWithMovimientosAsync(id);

                if (documento == null)
                {
                    sw.Stop();
                    _logger.LogWarning(
                        "⚠️ Documento {Id} no encontrado después de {Ms}ms",
                        id, sw.ElapsedMilliseconds);

                    return NotFound(new
                    {
                        success = false,
                        message = $"Documento {id} no encontrado"
                    });
                }
                var factura = await _service.GetByIdForFacturaAsync(id);
                sw.Stop();
                _logger.LogInformation(
                    "✅ GET /api/AdmDocumentos/{Id} completado en {Ms}ms. Movimientos: {MovCount}",
                    id, sw.ElapsedMilliseconds, documento.Movimientos.Count);

                return Ok(new
                {
                    success = true,
                    data = documento,
                    executionTime = $"{sw.ElapsedMilliseconds}ms",
                    Facturado = factura
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error al obtener documento {Id} después de {Ms}ms",
                    id, sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener documento {id}",
                    error = ex.Message
                });
            }
        }


        /// <summary>
        /// Obtiene un resumen rápido de documentos por fecha
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial</param>
        /// <param name="fechaFin">Fecha final</param>
        [HttpGet("resumen")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResumen(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (!fechaInicio.HasValue || !fechaFin.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Se requieren fechaInicio y fechaFin"
                    });
                }

                if (fechaInicio.Value > fechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor que la fecha final"
                    });
                }

                var filter = new AdmDocumentoFilterDto
                {
                    FechaInicio = fechaInicio.Value,
                    FechaFin = fechaFin.Value,
                    Page = 1,
                    PageSize = 1, // Solo necesitamos el total
                    IncluirMovimientos = false
                };

                var (_, totalRegistros) = await _service.SearchPaginatedAsync(filter);

                sw.Stop();
                _logger.LogInformation(
                    "✅ GET /api/AdmDocumentos/resumen completado en {Ms}ms. Total: {Total}",
                    sw.ElapsedMilliseconds, totalRegistros);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        fechaInicio = fechaInicio.Value,
                        fechaFin = fechaFin.Value,
                        totalDocumentos = totalRegistros
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error al obtener resumen después de {Ms}ms",
                    sw.ElapsedMilliseconds);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener resumen",
                    error = ex.Message
                });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS POST (CREACIÓN)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un nuevo documento en el sistema legacy
        /// </summary>
        /// <remarks>
        /// Campos requeridos:
        /// - IdDocumentoDe: ID del modelo de documento (FK a admDocumentosModelo)
        /// - IdConceptoDocumento: ID del concepto (FK a admConceptos)
        /// - IdClienteProveedor: ID del cliente/proveedor (FK a admClientes)
        /// - IdAgente: ID del agente (FK a admAgentes)
        /// - IdMoneda: ID de la moneda (FK a admMonedas)
        /// - SerieDocumento: Serie del documento (máx 11 caracteres)
        /// - Folio: Número de folio
        /// - FechaVencimiento: Fecha de vencimiento
        /// - TipoCambio: Tipo de cambio de la moneda
        /// - Referencia: Referencia del documento (máx 20 caracteres)
        /// - Naturaleza: 1=Cargo, 2=Abono
        /// - UsaCliente: 1=Cliente, 0=Proveedor
        /// - Afectado: 1=Sí, 0=No
        /// - Impreso: 1=Sí, 0=No
        /// - Cancelado: 1=Sí, 0=No
        /// - Neto: Subtotal antes de impuestos
        /// - Impuesto1: Impuesto aplicado (IVA)
        /// - DescuentoMov: Descuento a nivel movimiento
        /// - Total: Total del documento
        /// - Pendiente: Saldo pendiente
        /// - TotalUnidades: Cantidad total de productos/servicios
        /// 
        /// Campos opcionales:
        /// - Observaciones: Notas del documento
        /// 
        /// El sistema genera automáticamente:
        /// - CFecha (fecha actual)
        /// - CGuidDocumento (GUID único)
        /// - CTimestamp (timestamp del sistema)
        /// - CRazonSocial y CRfc (obtenidos del cliente/proveedor)
        /// </remarks>
        /// <param name="dto">Datos del documento a crear</param>
        /// <response code="201">Documento creado exitosamente</response>
        /// <response code="400">Datos inválidos o validación fallida</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AdmDocumentoCreateDto dto)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("📝 POST /api/AdmDocumentos - Iniciando creación. Serie: {Serie}, Folio: {Folio}",
                    dto.SerieDocumento, dto.Folio);

                // Validar ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("⚠️ Validación fallida. Errores: {Errores}", string.Join(", ", errors));

                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors = errors
                    });
                }

                // Validaciones adicionales de negocio
                if (dto.Total <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El total del documento debe ser mayor a 0"
                    });
                }

                if (dto.Pendiente < 0 || dto.Pendiente > dto.Total)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El pendiente debe estar entre 0 y el total del documento"
                    });
                }

                if (dto.FechaVencimiento < DateTime.Today)
                {
                    _logger.LogWarning("⚠️ Fecha de vencimiento {FechaVencimiento} es menor a la fecha actual",
                        dto.FechaVencimiento);
                }

                // Crear documento
                var idDocumento = await _service.CreateAsync(dto);

                sw.Stop();
                _logger.LogInformation(
                    "✅ POST /api/AdmDocumentos completado en {Ms}ms. ID creado: {IdDocumento}, Serie: {Serie}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, idDocumento, dto.SerieDocumento, dto.Folio);

                // Retornar 201 Created con Location header
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = idDocumento },
                    new
                    {
                        success = true,
                        message = "Documento creado exitosamente",
                        data = new
                        {
                            idDocumento = idDocumento,
                            serieDocumento = dto.SerieDocumento,
                            folio = dto.Folio,
                            total = dto.Total,
                            fechaCreacion = DateTime.Now
                        },
                        executionTime = $"{sw.ElapsedMilliseconds}ms"
                    });
            }
            catch (InvalidOperationException ex)
            {
                // Errores de validación de relaciones (FKs no existen)
                sw.Stop();
                _logger.LogWarning(ex,
                    "⚠️ Validación fallida en creación de documento después de {Ms}ms. Serie: {Serie}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, dto.SerieDocumento, dto.Folio);

                return BadRequest(new
                {
                    success = false,
                    message = "Validación fallida",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "❌ Error al crear documento después de {Ms}ms. Serie: {Serie}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, dto.SerieDocumento, dto.Folio);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear documento",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ENDPOINT MEJORADO PARA COTIZACIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea una nueva cotización de forma simplificada (ENDPOINT MEJORADO)
        /// </summary>
        /// <remarks>
        /// Este endpoint facilita la creación de cotizaciones con valores por defecto automáticos.
        /// 
        /// **Campos que el usuario debe proporcionar:**
        /// - `idCliente` (obligatorio): ID del cliente obtenido mediante búsqueda previa
        /// - `productos` (obligatorio): Array con los productos a cotizar
        ///   - `idProducto`: ID del producto
        ///   - `idAlmacen`: ID del almacén
        ///   - `unidades`: Cantidad
        ///   - `precio`: Precio unitario
        ///   - `porcentajeDescuento` (opcional): Descuento individual del producto
        /// 
        /// **Campos opcionales:**
        /// - `idAgente`: Agente de ventas (default: 1)
        /// - `fechaVencimiento`: Fecha de vencimiento (default: 30 días después)
        /// - `descuentoDoc1`: Descuento global 1 (porcentaje)
        /// - `descuentoDoc2`: Descuento global 2 (porcentaje)
        /// - `montoPagado`: Monto que el cliente pagó o abonó
        /// - `aplicarIVA`: Si se aplica IVA (default: true)
        /// - `porcentajeIVA`: Porcentaje de IVA (default: 16%)
        /// - `observaciones`: Notas adicionales
        /// - `referencia`: Referencia del documento
        /// 
        /// **Campos automáticos (el sistema los genera):**
        /// - Serie: "CA"
        /// - Folio: Auto-incremental (se obtiene de admConceptos)
        /// - Fecha: Fecha actual
        /// - IdDocumentoDe: 1
        /// - IdConceptoDocumento: 1
        /// - TipoCambio: 1
        /// - IdMoneda: 1 (MXN)
        /// - Naturaleza: 1 (Cargo)
        /// - SistOrig: 205
        /// - Afectado: 1
        /// - Impreso: 0
        /// - Cancelado: 0
        /// - Devuelto: 0
        /// - IdPrepoliza: 0
        /// - Destinatario: Igual a razón social del cliente
        /// - Total, Neto, Impuestos: Calculados automáticamente
        /// - Pendiente: Total - MontoPagado
        /// 
        /// **Ejemplo de petición:**
        /// ```json
        /// {
        ///   "idCliente": 123,
        ///   "idAgente": 1,
        ///   "productos": [
        ///     {
        ///       "idProducto": 10,
        ///       "idAlmacen": 1,
        ///       "unidades": 5,
        ///       "precio": 100.00,
        ///       "porcentajeDescuento": 10
        ///     }
        ///   ],
        ///   "descuentoDoc1": 5,
        ///   "montoPagado": 200,
        ///   "aplicarIVA": true,
        ///   "observaciones": "Cotización urgente"
        /// }
        /// ```
        /// </remarks>
        /// <param name="dto">DTO simplificado con los datos de la cotización</param>
        /// <returns>Datos de la cotización creada con folio asignado</returns>
        [HttpPost("cotizacion")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCotizacion([FromBody] AdmCotizacionCreateDto dto)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("🚀 [POST] /api/AdmDocumentos/cotizacion - Cliente: {IdCliente}, Productos: {CantidadProductos}",
                    dto.IdCliente, dto.Productos?.Count ?? 0);

                // Validar ModelState
                if (!ModelState.IsValid)
                {
                    sw.Stop();
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("⚠️ Validación fallida después de {Ms}ms. Errores: {Errores}",
                        sw.ElapsedMilliseconds, string.Join(", ", errors));

                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos de entrada inválidos",
                        errors = errors,
                        executionTime = $"{sw.ElapsedMilliseconds}ms"
                    });
                }

                // Crear cotización
                var response = await _service.CreateCotizacionAsync(dto);

                sw.Stop();
                _logger.LogInformation("✅ Cotización creada exitosamente después de {Ms}ms. ID: {IdDocumento}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, response.IdDocumento, response.Folio);

                return StatusCode(201, new
                {
                    success = true,
                    message = response.Mensaje,
                    data = response,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (InvalidOperationException ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "⚠️ Operación inválida después de {Ms}ms. Cliente: {IdCliente}",
                    sw.ElapsedMilliseconds, dto.IdCliente);

                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al crear cotización después de {Ms}ms. Cliente: {IdCliente}",
                    sw.ElapsedMilliseconds, dto.IdCliente);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear cotización",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
        }

        /// <summary>
        /// Edita una cotización existente (ENDPOINT PUT)
        /// </summary>
        /// <remarks>
        /// Este endpoint permite editar una cotización existente, actualizando sus datos y movimientos de forma inteligente.
        /// 
        /// **Estrategia de actualización:**
        /// - **Movimientos:** Se comparan los movimientos recibidos con los existentes:
        ///   - Si tiene `idMovimiento` y existe en BD: Se actualiza.
        ///   - Si tiene `idMovimiento` pero NO existe en BD: Error (o se ignora, depende de consistencia).
        ///   - Si NO tiene `idMovimiento`: Se crea como nuevo movimiento.
        ///   - Si un movimiento existe en BD pero NO viene en el array: Se ELIMINA.
        /// 
        /// **Campos requeridos:**
        /// - `idDocumento` (obligatorio): ID de la cotización a editar
        /// - `idCliente` (obligatorio): Cliente (puede haber cambiado)
        /// - `productos` (obligatorio): Lista completa de productos (nuevos + existentes modificados)
        /// 
        /// **Ejemplo de petición:**
        /// ```json
        /// {
        ///   "idDocumento": 12345,
        ///   "idCliente": 100,
        ///   "productos": [
        ///     { "idMovimiento": 555, "idProducto": 10, "unidades": 5, ... }, // Actualizar
        ///     { "idProducto": 20, "unidades": 2, ... } // Insertar nuevo
        ///   ],
        ///   ...
        /// }
        /// ```
        /// </remarks>
        [HttpPut("cotizacion")]
        [ProducesResponseType(typeof(AdmCotizacionCreateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditCotizacion([FromBody] AdmCotizacionUpdateDto dto)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { success = false, message = "Datos de cotización inválidos" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Errores de validación",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var resultado = await _service.EditCotizacionAsync(dto);

                sw.Stop();
                _logger.LogInformation(
                    "✅ PUT /api/AdmDocumentos/cotizacion completado en {Ms}ms. Nueva Versión: {Folio}",
                    sw.ElapsedMilliseconds, resultado.Folio);

                return Ok(new
                {
                    success = true,
                    data = resultado,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (InvalidOperationException ex)
            {
                sw.Stop();
                _logger.LogWarning("⚠️ Error de validación al editar cotización: {Message}", ex.Message);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al editar cotización después de {Ms}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al procesar la edición de la cotización",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancela una cotización existente (ENDPOINT PUT)
        /// </summary>
        /// <remarks>
        /// Este endpoint permite cancelar una cotización existente cambiando el campo CCANCELADO a 1.
        /// 
        /// **Campos requeridos:**
        /// - `idDocumento` (obligatorio): ID del documento a cancelar
        /// 
        /// **Campos opcionales:**
        /// - `motivo`: Motivo de la cancelación
        /// - `usuarioCancela`: Usuario que cancela (si no se envía, se usa "SISTEMA")
        /// 
        /// **Validaciones:**
        /// - El documento debe existir
        /// - El documento no debe estar previamente cancelado
        /// 
        /// **Ejemplo de petición:**
        /// ```json
        /// {
        ///   "idDocumento": 12345,
        ///   "motivo": "Cliente solicitó cancelación",
        ///   "usuarioCancela": "ADMIN"
        /// }
        /// ```
        /// </remarks>
        /// <param name="dto">DTO con los datos de cancelación</param>
        /// <returns>Datos de la cotización cancelada</returns>
        [HttpPut("cotizacion/cancelar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelarCotizacion([FromBody] AdmCotizacionCancelarDto dto)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation(" [PUT] /api/AdmDocumentos/cotizacion/cancelar - Documento: {IdDocumento}",
                    dto.IdDocumento);

                // Validar ModelState
                if (!ModelState.IsValid)
                {
                    sw.Stop();
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning(" Validación fallida después de {Ms}ms. Errores: {Errores}",
                        sw.ElapsedMilliseconds, string.Join(", ", errors));

                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos de entrada inválidos",
                        errors = errors,
                        executionTime = $"{sw.ElapsedMilliseconds}ms"
                    });
                }

                // Cancelar cotización
                var response = await _service.CancelarCotizacionAsync(dto);

                sw.Stop();
                _logger.LogInformation("✅ Cotización cancelada exitosamente después de {Ms}ms. ID: {IdDocumento}, Folio: {Folio}",
                    sw.ElapsedMilliseconds, response.IdDocumento, response.Folio);

                return Ok(new
                {
                    success = true,
                    message = response.Mensaje,
                    data = response,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (InvalidOperationException ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "⚠️ Operación inválida después de {Ms}ms. Documento: {IdDocumento}",
                    sw.ElapsedMilliseconds, dto.IdDocumento);

                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al cancelar cotización después de {Ms}ms. Documento: {IdDocumento}",
                    sw.ElapsedMilliseconds, dto.IdDocumento);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al cancelar cotización",
                    error = ex.Message,
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
        }

        /// <summary>
        /// Elimina una cotización (solo si está cancelada)
        /// </summary>
        /// <param name="id">ID del documento a eliminar</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("🗑️ DELETE /api/AdmDocumentos/{Id}", id);

                await _service.DeleteAsync(id);

                sw.Stop();
                _logger.LogInformation("✅ Documento {Id} eliminado exitosamente en {Ms}ms", id, sw.ElapsedMilliseconds);

                return Ok(new
                {
                    success = true,
                    message = $"Documento {id} eliminado exitosamente",
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (InvalidOperationException ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "⚠️ No se pudo eliminar el documento {Id}: {Message}", id, ex.Message);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al eliminar documento {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar documento",
                    error = ex.Message
                });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ENDPOINTS PARA REPORTES Y ESTADÍSTICAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene estadísticas generales de cotizaciones para dashboard
        /// </summary>
        /// <remarks>
        /// Retorna métricas clave como:
        /// - Total de cotizaciones en el período
        /// - Montos totales, promedios, máximos y mínimos
        /// - Cotizaciones activas vs canceladas
        /// - Clientes únicos y productos únicos
        /// 
        /// Parámetros opcionales:
        /// - fechaInicio: Filtrar desde esta fecha (formato: yyyy-MM-dd)
        /// - fechaFin: Filtrar hasta esta fecha (formato: yyyy-MM-dd)
        /// </remarks>
        [HttpGet("estadisticas")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEstadisticas([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("📊 GET /api/AdmDocumentos/estadisticas - Período: {FechaInicio} - {FechaFin}",
                    fechaInicio?.ToString("yyyy-MM-dd") ?? "sin límite",
                    fechaFin?.ToString("yyyy-MM-dd") ?? "sin límite");

                // Validación de fechas
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor que la fecha final"
                    });
                }

                var estadisticas = await _service.GetEstadisticasGeneralesAsync(fechaInicio, fechaFin);

                sw.Stop();
                _logger.LogInformation("✅ Estadísticas obtenidas en {Ms}ms: {Total} cotizaciones, ${Monto:N2} total",
                    sw.ElapsedMilliseconds,
                    estadisticas.TotalCotizaciones,
                    estadisticas.MontoTotal);

                return Ok(new
                {
                    success = true,
                    data = estadisticas,
                    filtros = new
                    {
                        fechaInicio,
                        fechaFin
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
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

        /// <summary>
        /// Obtiene el ranking de top clientes por cotizaciones y montos
        /// </summary>
        /// <remarks>
        /// Retorna los clientes más activos ordenados por monto total, incluyendo:
        /// - Datos del cliente (ID, código, razón social, RFC)
        /// - Total de cotizaciones y montos
        /// - Promedio de monto por cotización
        /// - Cotizaciones activas
        /// - Fecha de última cotización
        /// - Posición en el ranking
        /// 
        /// Parámetros:
        /// - top: Número de clientes a retornar (default: 10, max: 100)
        /// - fechaInicio: Filtrar desde esta fecha (opcional)
        /// - fechaFin: Filtrar hasta esta fecha (opcional)
        /// </remarks>
        [HttpGet("top-clientes")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTopClientes(
            [FromQuery] int top = 10,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("📊 GET /api/AdmDocumentos/top-clientes - Top: {Top}, Período: {FechaInicio} - {FechaFin}",
                    top,
                    fechaInicio?.ToString("yyyy-MM-dd") ?? "sin límite",
                    fechaFin?.ToString("yyyy-MM-dd") ?? "sin límite");

                // Validación de parámetros
                if (top < 1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El parámetro 'top' debe ser mayor a 0"
                    });
                }

                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor que la fecha final"
                    });
                }

                var topClientes = await _service.GetTopClientesAsync(top, fechaInicio, fechaFin);

                sw.Stop();
                _logger.LogInformation("✅ Top clientes obtenido en {Ms}ms: {Count} registros",
                    sw.ElapsedMilliseconds,
                    topClientes.Count);

                return Ok(new
                {
                    success = true,
                    data = topClientes,
                    filtros = new
                    {
                        top,
                        fechaInicio,
                        fechaFin
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener top clientes después de {Ms}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener top clientes",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene cotizaciones próximas a vencer (alertas)
        /// </summary>
        /// <remarks>
        /// Retorna cotizaciones activas que vencen en los próximos N días, ordenadas por urgencia.
        /// Incluye nivel de urgencia (Crítico, Alto, Medio, Bajo) basado en días restantes.
        ///
        /// Parámetros:
        /// - dias: Número de días para considerar como "próximas a vencer" (default: 7, max: 90)
        ///
        /// Niveles de urgencia:
        /// - Crítico: 1 día o menos
        /// - Alto: 2-3 días
        /// - Medio: 4-7 días
        /// - Bajo: 8+ días
        /// </remarks>
        [HttpGet("proximas-vencer")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProximasVencer(
            [FromQuery] int dias = 30,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("📊 GET /api/AdmDocumentos/proximas-vencer - Días: {Dias}, Página: {Page}", dias, page);

                // Validación de parámetros
                if (dias < 1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El parámetro 'dias' debe ser mayor a 0"
                    });
                }

                if (dias > 90)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El parámetro 'dias' no puede ser mayor a 90"
                    });
                }

                if (page < 1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El parámetro 'page' debe ser mayor a 0"
                    });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El parámetro 'pageSize' debe estar entre 1 y 100"
                    });
                }

                var (cotizaciones, totalRegistros) = await _service.GetProximasVencerAsync(dias, page, pageSize);

                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);

                sw.Stop();
                _logger.LogInformation("✅ Cotizaciones próximas a vencer obtenidas en {Ms}ms: {Count} registros de {Total}",
                    sw.ElapsedMilliseconds,
                    cotizaciones.Count,
                    totalRegistros);

                return Ok(new
                {
                    success = true,
                    data = cotizaciones,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize,
                        totalPages = totalPaginas,
                        totalRecords = totalRegistros,
                        hasNextPage = page < totalPaginas,
                        hasPreviousPage = page > 1
                    },
                    filtros = new
                    {
                        dias
                    },
                    resumen = new
                    {
                        total = totalRegistros,
                        criticos = cotizaciones.Count(c => c.NivelUrgencia == "Crítico"),
                        altos = cotizaciones.Count(c => c.NivelUrgencia == "Alto"),
                        medios = cotizaciones.Count(c => c.NivelUrgencia == "Medio"),
                        bajos = cotizaciones.Count(c => c.NivelUrgencia == "Bajo")
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener cotizaciones próximas a vencer después de {Ms}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener cotizaciones próximas a vencer",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene rendimiento por agente de ventas
        /// </summary>
        /// <remarks>
        /// Retorna métricas de rendimiento para cada agente de ventas incluyendo:
        /// - Total de cotizaciones realizadas
        /// - Monto total y promedio de cotizaciones
        /// - Número de cotizaciones ganadas y pendientes
        /// - Tasa de conversión (cotizaciones ganadas / total)
        ///
        /// Parámetros opcionales:
        /// - fechaInicio: Fecha inicial del rango (formato: YYYY-MM-DD)
        /// - fechaFin: Fecha final del rango (formato: YYYY-MM-DD)
        /// Si no se especifican fechas, se usa el último mes por defecto.
        /// </remarks>
        [HttpGet("rendimiento-agentes")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRendimientoAgentes([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("📊 GET /api/AdmDocumentos/rendimiento-agentes - Inicio: {FechaInicio}, Fin: {FechaFin}",
                    fechaInicio?.ToShortDateString() ?? "Sin especificar",
                    fechaFin?.ToShortDateString() ?? "Sin especificar");

                // Validación de fechas
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor a la fecha final"
                    });
                }

                var rendimiento = await _service.GetRendimientoAgentesAsync(fechaInicio, fechaFin);

                sw.Stop();
                _logger.LogInformation("✅ Rendimiento de agentes obtenido en {Ms}ms: {Count} agentes",
                    sw.ElapsedMilliseconds,
                    rendimiento.Count);

                return Ok(new
                {
                    success = true,
                    data = rendimiento,
                    filtros = new
                    {
                        fechaInicio = fechaInicio?.ToString("yyyy-MM-dd"),
                        fechaFin = fechaFin?.ToString("yyyy-MM-dd")
                    },
                    resumen = new
                    {
                        totalAgentes = rendimiento.Count,
                        totalCotizaciones = rendimiento.Sum(r => r.TotalCotizaciones),
                        montoTotalGeneral = rendimiento.Sum(r => r.MontoTotal),
                        promedioConversion = rendimiento.Any() ? rendimiento.Average(r => r.TasaConversion) : 0,
                        mejorAgente = rendimiento.OrderByDescending(r => r.MontoTotal).FirstOrDefault()?.NombreAgente
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener rendimiento de agentes después de {Ms}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener rendimiento de agentes",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene productos más cotizados por frecuencia y volumen
        /// </summary>
        /// <remarks>
        /// Retorna el ranking de productos más demandados en cotizaciones, ordenados por:
        /// - Número total de cotizaciones que incluyen el producto
        /// - Monto total cotizado del producto
        ///
        /// Incluye métricas como:
        /// - Cantidad total cotizada
        /// - Precio promedio por unidad
        /// - Número de clientes únicos
        ///
        /// Parámetros opcionales:
        /// - top: Número de productos a retornar (1-100, default: 10)
        /// - fechaInicio: Fecha inicial del rango (formato: YYYY-MM-DD)
        /// - fechaFin: Fecha final del rango (formato: YYYY-MM-DD)
        /// Si no se especifican fechas, se usa el último mes por defecto.
        /// </remarks>
        [HttpGet("productos-mas-cotizados")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductosMasCotizados([FromQuery] int top = 10, [FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("📊 GET /api/AdmDocumentos/productos-mas-cotizados - Top: {Top}, Inicio: {FechaInicio}, Fin: {FechaFin}",
                    top, fechaInicio?.ToShortDateString() ?? "Sin especificar",
                    fechaFin?.ToShortDateString() ?? "Sin especificar");

                // Validación de parámetros
                if (top < 1 || top > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El parámetro 'top' debe estar entre 1 y 100"
                    });
                }

                // Validación de fechas
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor a la fecha final"
                    });
                }

                var productos = await _service.GetProductosMasCotizadosAsync(top, fechaInicio, fechaFin);

                sw.Stop();
                _logger.LogInformation("✅ Productos más cotizados obtenidos en {Ms}ms: {Count} productos",
                    sw.ElapsedMilliseconds,
                    productos.Count);

                return Ok(new
                {
                    success = true,
                    data = productos,
                    filtros = new
                    {
                        top,
                        fechaInicio = fechaInicio?.ToString("yyyy-MM-dd"),
                        fechaFin = fechaFin?.ToString("yyyy-MM-dd")
                    },
                    resumen = new
                    {
                        totalProductos = productos.Count,
                        montoTotalGeneral = productos.Sum(p => p.MontoTotal),
                        cantidadTotalGeneral = productos.Sum(p => p.CantidadTotal),
                        clientesUnicosTotal = productos.Sum(p => p.ClientesUnicos)
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener productos más cotizados después de {Ms}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener productos más cotizados",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene distribución de cotizaciones por rangos de monto
        /// </summary>
        /// <remarks>
        /// Retorna estadísticas de cotizaciones agrupadas por rangos de monto predefinidos:
        /// - $0 - $1,000
        /// - $1,000 - $5,000
        /// - $5,000 - $10,000
        /// - $10,000 - $25,000
        /// - $25,000 - $50,000
        /// - $50,000+
        ///
        /// Incluye métricas como total de cotizaciones, montos acumulados,
        /// promedios y distribución por estatus.
        ///
        /// Parámetros opcionales:
        /// - fechaInicio: Fecha inicial del rango (formato: YYYY-MM-DD)
        /// - fechaFin: Fecha final del rango (formato: YYYY-MM-DD)
        /// Si no se especifican fechas, se usa el último mes por defecto.
        /// </remarks>
        [HttpGet("cotizaciones-por-rango-monto")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCotizacionesPorRangoMonto([FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("📊 GET /api/AdmDocumentos/cotizaciones-por-rango-monto - Inicio: {FechaInicio}, Fin: {FechaFin}",
                    fechaInicio?.ToShortDateString() ?? "Sin especificar",
                    fechaFin?.ToShortDateString() ?? "Sin especificar");

                // Validación de fechas
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha inicial no puede ser mayor a la fecha final"
                    });
                }

                var rangos = await _service.GetCotizacionesPorRangoMontoAsync(fechaInicio, fechaFin);

                sw.Stop();
                _logger.LogInformation("✅ Cotizaciones por rango de monto obtenidas en {Ms}ms: {Count} rangos",
                    sw.ElapsedMilliseconds,
                    rangos.Count);

                return Ok(new
                {
                    success = true,
                    data = rangos,
                    filtros = new
                    {
                        fechaInicio = fechaInicio?.ToString("yyyy-MM-dd"),
                        fechaFin = fechaFin?.ToString("yyyy-MM-dd")
                    },
                    resumen = new
                    {
                        totalRangos = rangos.Count,
                        totalCotizaciones = rangos.Sum(r => r.TotalCotizaciones),
                        montoTotalGeneral = rangos.Sum(r => r.MontoTotal),
                        rangoMasFrecuente = rangos.OrderByDescending(r => r.TotalCotizaciones).FirstOrDefault()?.RangoMonto,
                        rangoMayorMonto = rangos.OrderByDescending(r => r.MontoTotal).FirstOrDefault()?.RangoMonto
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener cotizaciones por rango de monto después de {Ms}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener cotizaciones por rango de monto",
                    error = ex.Message
                });
            }
        }
        /// <summary>
        /// Obtiene estadísticas de cotizaciones agrupadas por mes para un año específico
        /// </summary>
        /// <remarks>
        /// Retorna las estadísticas mensuales (activas, canceladas, montos) para graficar la evolución anual.
        ///
        /// Parámetros:
        /// - anio: Año a consultar (default: año actual)
        /// </remarks>
        [HttpGet("estadisticas-mensuales")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEstadisticasMensuales([FromQuery] int? anio = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                int anioConsulta = anio ?? DateTime.Now.Year;

                _logger.LogInformation("📊 GET /api/AdmDocumentos/estadisticas-mensuales - Año: {Anio}", anioConsulta);

                if (anioConsulta < 2000 || anioConsulta > 2100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El año debe estar entre 2000 y 2100"
                    });
                }

                var estadisticas = await _service.GetEstadisticasMensualesAsync(anioConsulta);

                sw.Stop();
                _logger.LogInformation("✅ Estadísticas mensuales obtenidas en {Ms}ms", sw.ElapsedMilliseconds);

                return Ok(new
                {
                    success = true,
                    data = estadisticas,
                    filtros = new
                    {
                        anio = anioConsulta
                    },
                    resumen = new
                    {
                        totalAnual = estadisticas.Sum(m => m.CotizacionesActivas + m.CotizacionesCanceladas),
                        montoTotalAnual = estadisticas.Sum(m => m.MontoTotal)
                    },
                    executionTime = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "❌ Error al obtener estadísticas mensuales después de {Ms}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener estadísticas mensuales",
                    error = ex.Message
                });
            }
        }
    }
}

