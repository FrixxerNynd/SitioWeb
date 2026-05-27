using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio para AdmDocumentos (Cotizaciones)
    /// </summary>
    public class AdmDocumentoService : IAdmDocumentoService
    {
        private readonly IAdmDocumentoRepository _repository;
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmDocumentoService> _logger;
        private readonly ICacheService _cacheService;

        public AdmDocumentoService(
            IAdmDocumentoRepository repository,
            LegacyCompacReadOnlyContext context,
            ILogger<AdmDocumentoService> logger,
            ICacheService cacheService)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Busca documentos aplicando filtros con paginación
        /// </summary>
        public async Task<(List<AdmDocumentoResponseDto> documentos, int totalRegistros)> SearchPaginatedAsync(AdmDocumentoFilterDto filter)
        {
            try
            {
                // Validar y normalizar filtros
                filter.Page = filter.Page < 1 ? 1 : filter.Page;
                filter.PageSize = filter.PageSize < 1 ? 50 : (filter.PageSize > 100 ? 100 : filter.PageSize);

                _logger.LogInformation(
                    "🔍 Buscando documentos. Página: {Page}, Tamaño: {PageSize}, Fecha: {FechaInicio}-{FechaFin}, RazonSocial: {RazonSocial}",
                    filter.Page, filter.PageSize, filter.FechaInicio, filter.FechaFin, filter.RazonSocial);

                var (documentos, totalRegistros) = await _repository.SearchPaginatedAsync(filter);

                // Obtener IDs únicos para consultas relacionadas (solo agentes)
                var agentesIds = documentos.Select(d => d.CIdAgente).Distinct().Where(id => id > 0).ToList();

                // Cargar solo agentes (único campo relacionado necesario)
                var agentes = await CargarAgentesAsync(agentesIds);

                // Si se solicitan movimientos, cargarlos
                Dictionary<int, List<AdmMovimientoResponseDto>> movimientosPorDocumento = new();

                if (filter.IncluirMovimientos)
                {
                    var documentosIds = documentos.Select(d => d.CIdDocumento).ToList();
                    movimientosPorDocumento = await CargarMovimientosPorDocumentosAsync(documentosIds);
                }


                // Mapear a DTOs
                var result = documentos.Select(d =>
                {

                    var dto = MapToDto(d, agentes);

                    if (filter.IncluirMovimientos && movimientosPorDocumento.TryGetValue(d.CIdDocumento, out var movs))
                    {
                        dto.Movimientos = movs;
                    }

                    return dto;
                }).ToList();

                _logger.LogInformation(
                    "✅ Búsqueda de documentos completada. Total: {Total}, Retornados: {Count}",
                    totalRegistros, result.Count);

                return (result, totalRegistros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar documentos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un documento por ID incluyendo sus movimientos (productos cotizados)
        /// </summary>
        public async Task<AdmDocumentoResponseDto?> GetByIdWithMovimientosAsync(int idDocumento)
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo documento {IdDocumento} con movimientos", idDocumento);

                var documento = await _repository.GetByIdWithMovimientosAsync(idDocumento);

                if (documento == null)
                {
                    _logger.LogWarning("⚠️ Documento {IdDocumento} no encontrado", idDocumento);
                    return null;
                }

                var facturado = await GetByIdForFacturaAsync(idDocumento);
                // Cargar solo agentes
                var agentes = await CargarAgentesAsync(new List<int> { documento.CIdAgente });

                // Cargar movimientos con productos
                var movimientosPorDocumento = await CargarMovimientosPorDocumentosAsync(new List<int> { idDocumento });

                var result = MapToDtoFac(documento, agentes, facturado ?? false);

                if (movimientosPorDocumento.TryGetValue(idDocumento, out var movimientos))
                {
                    result.Movimientos = movimientos;
                }

                _logger.LogInformation(
                    "✅ Documento {IdDocumento} obtenido con {MovCount} movimientos",
                    idDocumento, result.Movimientos.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        public async Task<bool?> GetByIdForFacturaAsync(int idDocumento)
        {
            try
            {
                var documento = await _repository.GetByIdForFacturaAsync(idDocumento);

                if (documento == null)
                {
                    _logger.LogInformation("⚠️ Documento {IdDocumento} no encontrado", idDocumento);
                    return false;
                }
                if (documento.CSerieDocumento != "A4")
                {
                    _logger.LogInformation("⚠️ Documento {IdDocumento} no es una factura", idDocumento);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        #region Métodos privados de carga de datos relacionados

        private async Task<Dictionary<int, models.legacy.AdmAgente>> CargarAgentesAsync(List<int> ids)
        {
            if (!ids.Any()) return new Dictionary<int, models.legacy.AdmAgente>();

            // Cargar todos los agentes y filtrar en memoria para evitar OPENJSON
            var allAgentes = await _context.AdmAgentes
                .AsNoTracking()
                .ToListAsync();

            return allAgentes
                .Where(a => ids.Contains(a.CIdAgente))
                .ToDictionary(a => a.CIdAgente);
        }

        private async Task<Dictionary<int, List<AdmMovimientoResponseDto>>> CargarMovimientosPorDocumentosAsync(List<int> documentosIds)
        {
            if (!documentosIds.Any()) return new Dictionary<int, List<AdmMovimientoResponseDto>>();

            // Para evitar OPENJSON, cargamos movimientos iterando por cada documento
            // Dado que es paginado (máx 50-100 docs), es aceptable
            var todosMovimientos = new List<models.legacy.AdmMovimiento>();

            foreach (var docId in documentosIds)
            {
                var movs = await _context.AdmMovimientos
                    .AsNoTracking()
                    .Where(m => m.CIdDocumento == docId)
                    .OrderBy(m => m.CNumeroMovimiento)
                    .ToListAsync();

                todosMovimientos.AddRange(movs);
            }

            // Obtener IDs únicos de entidades relacionadas
            var productosIds = todosMovimientos.Select(m => m.CIdProducto).Distinct().Where(id => id > 0).ToList();
            var almacenesIds = todosMovimientos.Select(m => m.CIdAlmacen).Distinct().Where(id => id > 0).ToList();

            // Cargar productos y almacenes - evitar Contains() para prevenir OPENJSON
            Dictionary<int, models.legacy.AdmProducto> productos;
            if (productosIds.Any())
            {
                var allProductos = await _context.AdmProductos.AsNoTracking().ToListAsync();
                productos = allProductos
                    .Where(p => productosIds.Contains(p.CIdProducto))
                    .ToDictionary(p => p.CIdProducto);
            }
            else
            {
                productos = new Dictionary<int, models.legacy.AdmProducto>();
            }

            Dictionary<int, models.legacy.AdmAlmacen> almacenes;
            if (almacenesIds.Any())
            {
                var allAlmacenes = await _context.AdmAlmacenes.AsNoTracking().ToListAsync();
                almacenes = allAlmacenes
                    .Where(a => almacenesIds.Contains(a.CIdAlmacen))
                    .ToDictionary(a => a.CIdAlmacen);
            }
            else
            {
                almacenes = new Dictionary<int, models.legacy.AdmAlmacen>();
            }

            // Agrupar movimientos por documento
            return todosMovimientos
                .GroupBy(m => m.CIdDocumento)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => MapMovimientoToDto(m, productos, almacenes)).ToList()
                );
        }

        #endregion

        #region Métodos de mapeo

        private AdmDocumentoResponseDto MapToDto(
            models.legacy.AdmDocumento entity,
            Dictionary<int, models.legacy.AdmAgente> agentes)
        {
            agentes.TryGetValue(entity.CIdAgente, out var agente);

            return new AdmDocumentoResponseDto
            {
                IdDocumento = entity.CIdDocumento,
                IdCliente = entity.CIdClienteProveedor,
                IdAgente = entity.CIdAgente,
                SerieDocumento = entity.CSerieDocumento,
                Folio = entity.CFolio,
                Fecha = entity.CFecha,
                RazonSocial = entity.CRazonSocial,
                FechaVencimiento = entity.CFechaVencimiento,
                FechaProntoPago = entity.CFechaProntoPago,
                FechaEntregaRecepcion = entity.CFechaEntregaRecepcion,

                // Totales
                Subtotal = entity.CNeto,
                IVA = entity.CImpuesto1,
                Total = entity.CTotal,

                // Descuentos
                DescuentoDoc1 = entity.CDescuentoDoc1,
                DescuentoDoc2 = entity.CDescuentoDoc2,
                DescuentoDoc3 = entity.CGasto1, // Usamos CGasto1 para el 3er descuento

                // Estado
                Estado = entity.CCancelado == 1 ? "Cancelada" : "Activa",
                Afectado = entity.CAfectado == 1 ? "Sí" : "No",
                Impreso = entity.CImpreso == 1 ? "Sí" : "No",
                Devuelto = entity.CDevuelto == 1 ? "Sí" : "No",

                // Observaciones
                Observaciones = entity.CObservaciones,

                // Agente (nombre completo)
                Agente = agente?.CNombreAgente,

                // Movimientos se cargan por separado
                Movimientos = new List<AdmMovimientoResponseDto>(),
            };
        }

        private AdmDocumentoResponseDto MapToDtoFac(
                    models.legacy.AdmDocumento entity,
                    Dictionary<int, models.legacy.AdmAgente> agentes,
                    bool facturado)
        {
            agentes.TryGetValue(entity.CIdAgente, out var agente);

            return new AdmDocumentoResponseDto
            {
                IdDocumento = entity.CIdDocumento,
                IdCliente = entity.CIdClienteProveedor,
                IdAgente = entity.CIdAgente,
                SerieDocumento = entity.CSerieDocumento,
                Folio = entity.CFolio,
                Fecha = entity.CFecha,
                RazonSocial = entity.CRazonSocial,
                FechaVencimiento = entity.CFechaVencimiento,
                FechaProntoPago = entity.CFechaProntoPago,
                FechaEntregaRecepcion = entity.CFechaEntregaRecepcion,

                // Totales
                Subtotal = entity.CNeto,
                IVA = entity.CImpuesto1,
                Total = entity.CTotal,

                // Descuentos
                DescuentoDoc1 = entity.CDescuentoDoc1,
                DescuentoDoc2 = entity.CDescuentoDoc2,
                DescuentoDoc3 = entity.CGasto1, // Usamos CGasto1 para el 3er descuento

                // Estado
                Estado = entity.CCancelado == 1 ? "Cancelada" : "Activa",
                Afectado = entity.CAfectado == 1 ? "Sí" : "No",
                Impreso = entity.CImpreso == 1 ? "Sí" : "No",
                Devuelto = entity.CDevuelto == 1 ? "Sí" : "No",

                // Observaciones
                Observaciones = entity.CObservaciones,

                // Agente (nombre completo)
                Agente = agente?.CNombreAgente,

                // Movimientos se cargan por separado
                Movimientos = new List<AdmMovimientoResponseDto>(),
                Facturado = facturado
            };
        }

        private AdmMovimientoResponseDto MapMovimientoToDto(
            models.legacy.AdmMovimiento entity,
            Dictionary<int, models.legacy.AdmProducto> productos,
            Dictionary<int, models.legacy.AdmAlmacen> almacenes)
        {
            productos.TryGetValue(entity.CIdProducto, out var producto);
            almacenes.TryGetValue(entity.CIdAlmacen, out var almacen);

            return new AdmMovimientoResponseDto
            {
                IdMovimiento = entity.CIdMovimiento,
                NumeroMovimiento = entity.CNumeroMovimiento,

                // Producto
                IdProducto = entity.CIdProducto,
                CodigoProducto = producto?.CCodigoProducto,
                NombreProducto = producto?.CNombreProducto,
                DescripcionProducto = producto?.CDescripcionProducto,

                // Almacén
                IdAlmacen = entity.CIdAlmacen,
                CodigoAlmacen = almacen?.CCodigoAlmacen,
                NombreAlmacen = almacen?.CNombreAlmacen,

                // Unidades
                Unidades = entity.CUnidades,
                UnidadesCapturadas = entity.CUnidadesCapturadas,
                IdUnidad = entity.CIdUnidad,

                // Precios y costos
                Precio = entity.CPrecio,
                PrecioCapturado = entity.CPrecioCapturado,
                CostoCapturado = entity.CCostoCapturado,
                PorcentajeDescuento = entity.CPorcentajeDescuento1,
                DescuentoLinea = entity.CDescuento1,

                // Impuestos
                Impuesto1 = entity.CImpuesto1,
                Impuesto2 = entity.CImpuesto2,
                Impuesto3 = entity.CImpuesto3,
                Retencion1 = entity.CRetencion1,
                Retencion2 = entity.CRetencion2,

                // Totales
                Neto = entity.CNeto,
                Total = entity.CTotal,

                // Referencia y observaciones
                Referencia = entity.CReferencia,
                Observaciones = entity.CObservaMov,

                // Estado
                Afectado = entity.CAfectaExistencia,
                Venta = 0 // No existe campo CVenta en AdmMovimiento
            };
        }

        #endregion

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS POST (CREACIÓN)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un nuevo documento en el sistema legacy
        /// Valida las relaciones y aplica reglas de negocio
        /// </summary>
        public async Task<int> CreateAsync(AdmDocumentoCreateDto dto)
        {
            try
            {
                _logger.LogInformation("📝 Iniciando creación de documento. Serie: {Serie}, Folio: {Folio}",
                    dto.SerieDocumento, dto.Folio);

                // ═══════════════════════════════════════════════════════════════
                // VALIDACIONES DE RELACIONES (FKs)
                // ═══════════════════════════════════════════════════════════════

                _logger.LogInformation("🔍 Validando relaciones del documento...");

                // Validar DocumentoModelo existe
                if (!await _repository.ExistsDocumentoModeloAsync(dto.IdDocumentoDe))
                {
                    _logger.LogWarning("⚠️ Modelo de documento {IdDocumentoDe} no encontrado", dto.IdDocumentoDe);
                    throw new InvalidOperationException($"El modelo de documento con ID {dto.IdDocumentoDe} no existe");
                }

                // Validar Concepto existe
                if (!await _repository.ExistsConceptoAsync(dto.IdConceptoDocumento))
                {
                    _logger.LogWarning("⚠️ Concepto {IdConcepto} no encontrado", dto.IdConceptoDocumento);
                    throw new InvalidOperationException($"El concepto con ID {dto.IdConceptoDocumento} no existe");
                }

                // Validar Cliente/Proveedor existe
                if (!await _repository.ExistsClienteProveedorAsync(dto.IdClienteProveedor))
                {
                    _logger.LogWarning("⚠️ Cliente/Proveedor {IdClienteProveedor} no encontrado", dto.IdClienteProveedor);
                    throw new InvalidOperationException($"El cliente/proveedor con ID {dto.IdClienteProveedor} no existe");
                }

                // Validar Agente existe
                if (!await _repository.ExistsAgenteAsync(dto.IdAgente))
                {
                    _logger.LogWarning("⚠️ Agente {IdAgente} no encontrado", dto.IdAgente);
                    throw new InvalidOperationException($"El agente con ID {dto.IdAgente} no existe");
                }

                // Validar Moneda existe
                if (!await _repository.ExistsMonedaAsync(dto.IdMoneda))
                {
                    _logger.LogWarning("⚠️ Moneda {IdMoneda} no encontrada", dto.IdMoneda);
                    throw new InvalidOperationException($"La moneda con ID {dto.IdMoneda} no existe");
                }

                _logger.LogInformation("✅ Todas las relaciones validadas correctamente");

                // ═══════════════════════════════════════════════════════════════
                // OBTENER RAZÓN SOCIAL Y RFC DEL CLIENTE/PROVEEDOR
                // ═══════════════════════════════════════════════════════════════

                var cliente = await _context.AdmClientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CIdClienteProveedor == dto.IdClienteProveedor);

                string razonSocial = cliente?.CRazonSocial ?? "Sin razón social";
                string rfc = cliente?.CRfc ?? "XAXX010101000";

                // ═══════════════════════════════════════════════════════════════
                // MAPEAR DTO A ENTIDAD
                // ═══════════════════════════════════════════════════════════════

                var documento = new AdmDocumento
                {
                    // Relaciones (FKs)
                    CIdDocumentoDe = dto.IdDocumentoDe,
                    CIdConceptoDocumento = dto.IdConceptoDocumento,
                    CIdClienteProveedor = dto.IdClienteProveedor,
                    CIdAgente = dto.IdAgente,
                    CIdMoneda = dto.IdMoneda,

                    // Datos del documento
                    CSerieDocumento = dto.SerieDocumento,
                    CFolio = dto.Folio,
                    CFecha = DateTime.Now, // Fecha actual del sistema
                    CFechaVencimiento = dto.FechaVencimiento,
                    CTipoCambio = dto.TipoCambio,
                    CReferencia = dto.Referencia,
                    CObservaciones = dto.Observaciones,
                    CRazonSocial = razonSocial,
                    CRfc = rfc,

                    // Indicadores
                    CNaturaleza = dto.Naturaleza,
                    CUsaCliente = dto.UsaCliente,
                    CUsaProveedor = dto.UsaCliente == 1 ? 0 : 1, // Inverso de UsaCliente
                    CAfectado = dto.Afectado,
                    CImpreso = dto.Impreso,
                    CCancelado = dto.Cancelado,
                    CDevuelto = 0, // Por defecto no devuelto

                    // Importes
                    CNeto = dto.Neto,
                    CImpuesto1 = dto.Impuesto1,
                    CDescuentoMov = dto.DescuentoMov,
                    CTotal = dto.Total,
                    CPendiente = dto.Pendiente,
                    CTotalUnidades = dto.TotalUnidades,

                    // Campos con valores por defecto (ceros)
                    CImpuesto2 = 0,
                    CImpuesto3 = 0,
                    CRetencion1 = 0,
                    CRetencion2 = 0,
                    CDescuentoDoc1 = 0,
                    CDescuentoDoc2 = 0,
                    CGasto1 = 0,
                    CGasto2 = 0,
                    CGasto3 = 0,
                    CDescuentoProntoPago = 0,
                    CPorcentajeImpuesto1 = 0,
                    CPorcentajeImpuesto2 = 0,
                    CPorcentajeImpuesto3 = 0,
                    CPorcentajeRetencion1 = 0,
                    CPorcentajeRetencion2 = 0,
                    CPorcentajeInteres = 0,
                    CImporteExtra1 = 0,
                    CImporteExtra2 = 0,
                    CImporteExtra3 = 0,
                    CImporteExtra4 = 0,
                    CNumeroCajas = 0,
                    CPeso = 0,
                    CUnidadesPendientes = dto.TotalUnidades, // Igual al total al crear

                    // Fechas con valores por defecto (min value o fecha actual)
                    CFechaProntoPago = new DateTime(1899, 12, 30), // Fecha base Adminpaq
                    CFechaEntregaRecepcion = new DateTime(1899, 12, 30),
                    CFechaUltimoInteres = new DateTime(1899, 12, 30),
                    CFechaExtra = new DateTime(1899, 12, 30),

                    // Strings vacíos
                    CTextoExtra1 = string.Empty,
                    CTextoExtra2 = string.Empty,
                    CTextoExtra3 = string.Empty,
                    CDestinatario = string.Empty,
                    CNumeroGuia = string.Empty,
                    CMensajeria = string.Empty,
                    CCuentaMensajeria = string.Empty,
                    CLugarExpe = string.Empty,
                    CMetodoPag = string.Empty,
                    CCondiPago = string.Empty,
                    CNumCtaPag = string.Empty,
                    CUsuario = string.Empty,
                    CVerEsque = string.Empty,
                    CTransactionId = string.Empty,

                    // Flags booleanos
                    CBanObservaciones = 0,
                    CBanDatosEnvio = 0,
                    CBanCondicionesCredito = 0,
                    CBanGastos = 0,

                    // IDs relacionados en 0 (sin relación)
                    CIdDocumentoOrigen = 0,
                    CPlantilla = 0,
                    CIdPrepoliza = 0,
                    CIdPrepolizaCancelacion = 0,
                    CEstadoContable = 0,
                    CImpCheqPaq = 0,
                    CSistOrig = 0,
                    CIdMonedCa = dto.IdMoneda, // Misma moneda para cancelación
                    CTipoCamCa = dto.TipoCambio,
                    CEsCfd = 0,
                    CTieneCfd = 0,
                    CNumParcia = 0,
                    CCantParci = 0,
                    CIdProyecto = 0,
                    CIdCuenta = 0,
                    CIdCopiaDe = 0,

                    // GUID único para el documento
                    CGuidDocumento = Guid.NewGuid().ToString().ToUpper(),

                    // Timestamp del sistema (formato Adminpaq: YYYYMMDD HH:MM:SS.fff)
                    CTimestamp = DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"),

                };

                _logger.LogInformation("📦 Documento mapeado. GUID: {Guid}", documento.CGuidDocumento);

                // ═══════════════════════════════════════════════════════════════
                // CREAR EN BASE DE DATOS
                // ═══════════════════════════════════════════════════════════════

                var idDocumento = await _repository.CreateAsync(documento);

                _logger.LogInformation("✅ Documento creado exitosamente. ID: {IdDocumento}, Serie: {Serie}, Folio: {Folio}",
                    idDocumento, dto.SerieDocumento, dto.Folio);

                return idDocumento;
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar excepciones de validación sin log adicional
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear documento. Serie: {Serie}, Folio: {Folio}",
                    dto.SerieDocumento, dto.Folio);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS PARA COTIZACIONES MEJORADAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea una nueva cotización de forma simplificada
        /// Aplica valores por defecto y calcula totales automáticamente
        /// </summary>
        public async Task<AdmCotizacionCreateResponseDto> CreateCotizacionAsync(AdmCotizacionCreateDto dto)
        {
            try
            {
                _logger.LogInformation("🚀 Iniciando creación de cotización para cliente {IdCliente}", dto.IdCliente);

                // ═══════════════════════════════════════════════════════════════
                // PASO 1: VALIDACIONES DE EXISTENCIA
                // ═══════════════════════════════════════════════════════════════

                _logger.LogInformation("📋 Validando datos de entrada...");

                // Validar cliente
                if (!await _repository.ExistsClienteProveedorAsync(dto.IdCliente))
                {
                    throw new InvalidOperationException($"No existe un cliente con ID {dto.IdCliente}");
                }

                // Obtener datos del cliente
                var cliente = await _context.AdmClientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CIdClienteProveedor == dto.IdCliente);

                if (cliente == null)
                {
                    throw new InvalidOperationException($"No se pudo obtener información del cliente {dto.IdCliente}");
                }

                // Validar agente (si se envió)
                int idAgenteAUsar = dto.IdAgente ?? 1; // Agente por defecto ID 1
                if (!await _repository.ExistsAgenteAsync(idAgenteAUsar))
                {
                    throw new InvalidOperationException($"No existe un agente con ID {idAgenteAUsar}");
                }

                // Validar productos y almacenes
                var productosInfo = new Dictionary<int, AdmProducto>();
                foreach (var producto in dto.Productos)
                {
                    var prodDb = await _context.AdmProductos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.CIdProducto == producto.IdProducto);

                    if (prodDb == null)
                    {
                        throw new InvalidOperationException($"No existe un producto con ID {producto.IdProducto}");
                    }

                    if (!productosInfo.ContainsKey(producto.IdProducto))
                    {
                        productosInfo.Add(producto.IdProducto, prodDb);
                    }

                    // Si el almacén es 0, asignamos el 1 por defecto
                    if (producto.IdAlmacen == 0)
                    {
                        producto.IdAlmacen = 1;
                    }

                    if (!await _repository.ExistsAlmacenAsync(producto.IdAlmacen))
                    {
                        throw new InvalidOperationException($"No existe un almacén con ID {producto.IdAlmacen}");
                    }

                    if (producto.IdUnidad.HasValue)
                    {
                        if (!await _repository.ExistsUnidadAsync(producto.IdUnidad.Value))
                        {
                            throw new InvalidOperationException($"No existe una unidad con ID {producto.IdUnidad.Value}");
                        }
                    }
                }

                _logger.LogInformation("✅ Validaciones completadas exitosamente");

                // ═══════════════════════════════════════════════════════════════
                // PASO 2: CARGAR INFORMACIÓN DE PRODUCTOS
                // ═══════════════════════════════════════════════════════════════

                // Ya se cargaron en la validación para evitar error de sintaxis SQL con Contains en versiones antiguas
                // var productosIds = dto.Productos.Select(p => p.IdProducto).Distinct().ToList();
                // var productosInfo = await _context.AdmProductos
                //    .AsNoTracking()
                //    .Where(p => productosIds.Contains(p.CIdProducto))
                //    .ToDictionaryAsync(p => p.CIdProducto);

                // ═══════════════════════════════════════════════════════════════
                // PASO 3: CALCULAR TOTALES
                // ═══════════════════════════════════════════════════════════════

                _logger.LogInformation("🧮 Calculando totales...");

                double sumaNetoMovimientos = 0;

                var movimientos = new List<AdmMovimiento>();
                int numeroMovimiento = 1;

                foreach (var productoDto in dto.Productos)
                {
                    var productoInfo = productosInfo[productoDto.IdProducto];

                    // Calcular neto del movimiento (sin descuentos)
                    double netoMovimiento = productoDto.Unidades * productoDto.Precio;

                    // Calcular descuento del movimiento
                    // Soporta tanto porcentaje como importe fijo
                    double descuentoMovimiento = 0;
                    if (productoDto.DescuentoImporte.HasValue && productoDto.DescuentoImporte.Value > 0)
                    {
                        // Descuento por importe fijo tiene prioridad
                        descuentoMovimiento = productoDto.DescuentoImporte.Value;
                    }
                    else if (productoDto.PorcentajeDescuento.HasValue && productoDto.PorcentajeDescuento.Value > 0)
                    {
                        // Descuento por porcentaje
                        descuentoMovimiento = netoMovimiento * (productoDto.PorcentajeDescuento.Value / 100);
                    }

                    // Neto final del movimiento (después de descuento)
                    double netoFinalMovimiento = netoMovimiento - descuentoMovimiento;

                    // Calcular impuesto del movimiento (si aplica IVA)
                    double impuestoMovimiento = 0;
                    double porcentajeImpuesto = 0;

                    if (dto.AplicarIVA)
                    {
                        porcentajeImpuesto = dto.PorcentajeIVA;
                        impuestoMovimiento = netoFinalMovimiento * (porcentajeImpuesto / 100);
                    }

                    sumaNetoMovimientos += netoFinalMovimiento;

                    // Crear movimiento
                    var movimiento = new AdmMovimiento
                    {
                        CNumeroMovimiento = numeroMovimiento++,
                        CIdProducto = productoDto.IdProducto,
                        CIdAlmacen = productoDto.IdAlmacen,
                        CIdUnidad = productoDto.IdUnidad ?? productoInfo.CIdUnidadBase,
                        CIdUnidadNc = productoDto.IdUnidad ?? productoInfo.CIdUnidadBase,
                        CUnidades = productoDto.Unidades,
                        CUnidadesNc = productoDto.Unidades,
                        CUnidadesCapturadas = productoDto.Unidades,
                        CPrecio = productoDto.Precio,
                        CPrecioCapturado = productoDto.Precio,
                        CCostoCapturado = 0,
                        CCostoEspecifico = 0,
                        CNeto = netoFinalMovimiento,
                        CImpuesto1 = impuestoMovimiento,
                        CPorcentajeImpuesto1 = porcentajeImpuesto,
                        CImpuesto2 = 0,
                        CPorcentajeImpuesto2 = 0,
                        CImpuesto3 = 0,
                        CPorcentajeImpuesto3 = 0,
                        CRetencion1 = 0,
                        CPorcentajeRetencion1 = 0,
                        CRetencion2 = 0,
                        CPorcentajeRetencion2 = 0,
                        CDescuento1 = descuentoMovimiento,
                        CPorcentajeDescuento1 = productoDto.PorcentajeDescuento ?? 0,
                        CDescuento2 = 0,
                        CPorcentajeDescuento2 = 0,
                        CDescuento3 = 0,
                        CPorcentajeDescuento3 = 0,
                        CDescuento4 = 0,
                        CPorcentajeDescuento4 = 0,
                        CDescuento5 = 0,
                        CPorcentajeDescuento5 = 0,
                        CTotal = netoFinalMovimiento + impuestoMovimiento,
                        CPorcentajeComision = 0,
                        CReferencia = "",
                        CObservaMov = productoDto.Observaciones ?? "",
                        CAfectaExistencia = 0,
                        CAfectadoSaldos = 0,
                        CAfectadoInventario = 0,
                        CFecha = DateTime.Now,
                        CMovtoOculto = 0,
                        CIdMovtoOwner = 0,
                        CIdMovtoOrigen = 0,
                        CUnidadesPendientes = productoDto.Unidades,
                        CUnidadesNcPendientes = productoDto.Unidades,
                        CUnidadesOrigen = 0,
                        CUnidadesNcOrigen = 0,
                        CTipoTraspaso = 0,
                        CIdValorClasificacion = 0,
                        CTextoExtra1 = "",
                        CTextoExtra2 = "",
                        CTextoExtra3 = "",
                        CFechaExtra = DateTime.Now,
                        CImporteExtra1 = 0,
                        CImporteExtra2 = 0,
                        CImporteExtra3 = 0,
                        CImporteExtra4 = 0,
                        CTimestamp = "",
                        CGtoMovto = 0,
                        CScMovto = "",
                        CComVenta = 0,
                        CIdMovtoDestino = 0,
                        CNumeroConsolidaciones = 0,
                        CObjImpu01 = ""
                    };

                    movimientos.Add(movimiento);
                }

                // ═══════════════════════════════════════════════════════════════
                // PASO 3: CALCULAR TOTALES Y DESCUENTOS
                // IMPORTANTE: Se usa el CTOTAL proporcionado por el usuario, NO se calcula
                // ═══════════════════════════════════════════════════════════════

                _logger.LogInformation("🧮 Procesando cálculos con CTOTAL manual = {CTotal}...", dto.CTotal);

                // Suma de descuentos a nivel movimiento (calculada de los movimientos ya creados)
                double sumaDescuentoMovimientos = movimientos.Sum(m => m.CDescuento1);

                // ⚠️ CAMBIO CRÍTICO: El CTOTAL es proporcionado por el usuario
                // No se calcula autom\u00e1ticamente. El usuario define el precio final.
                double totalDocumento = dto.CTotal;

                // Calcular CNETO (Total antes de impuestos)
                // Si se aplica IVA: CNETO = CTOTAL / (1 + PorcentajeIVA/100)
                // Si no: CNETO = CTOTAL
                double netoDocumento;
                double impuestoTotal;

                if (dto.AplicarIVA)
                {
                    // CNETO = CTOTAL / (1 + 0.16) si IVA = 16%
                    netoDocumento = totalDocumento / (1 + (dto.PorcentajeIVA / 100));
                    impuestoTotal = totalDocumento - netoDocumento;
                }
                else
                {
                    netoDocumento = totalDocumento;
                    impuestoTotal = 0;
                }

                // Descuentos a nivel documento (aplicados sobre el CNETO)
                // NOTA: Se modificó para que sean importes directos, no porcentajes
                double descuentoDoc1Valor = dto.DescuentoDoc1 ?? 0;
                double descuentoDoc2Valor = dto.DescuentoDoc2 ?? 0;
                double descuentoDoc3Valor = dto.DescuentoDoc3 ?? 0;

                // Calcular pendiente
                double montoPagado = dto.MontoPagado ?? 0;
                double pendiente = totalDocumento - montoPagado;

                if (pendiente < 0)
                {
                    throw new InvalidOperationException("El monto pagado no puede ser mayor al total de la cotizaci\u00f3n");
                }

                _logger.LogInformation("💰 Totales finales - CTOTAL (Manual): {Total}, CNETO: {Neto}, Impuesto: {Impuesto}, Pendiente: {Pendiente}, Desc1: {Desc1}, Desc2: {Desc2}, Desc3: {Desc3}",
                    totalDocumento, netoDocumento, impuestoTotal, pendiente, descuentoDoc1Valor, descuentoDoc2Valor, descuentoDoc3Valor);

                // ═══════════════════════════════════════════════════════════════
                // PASO 4: CREAR DOCUMENTO
                // ═══════════════════════════════════════════════════════════════

                var fechaActual = DateTime.Now.Date; // Fecha sin hora como requiere AdminPAQ
                var fechaVencimiento = dto.FechaVencimiento?.Date ?? fechaActual;
                var fechaProntoPago = dto.FechaProntoPago?.Date ?? fechaVencimiento;
                var fechaEntregaRecepcion = dto.FechaEntregaRecepcion?.Date ?? fechaActual;
                var RazonSocialManual = "";

                if (dto.IdCliente == 832)
                {
                    RazonSocialManual = dto.RazonSocial;
                }
                else
                {
                    RazonSocialManual = cliente.CRazonSocial;
                }


                var documento = new AdmDocumento
                {
                    // Valores por defecto del sistema
                    CIdDocumentoDe = 1,
                    CIdConceptoDocumento = 1,
                    CSerieDocumento = "CA",
                    // CFolio se asigna automáticamente en el repository
                    CFecha = fechaActual,
                    CIdClienteProveedor = dto.IdCliente,
                    CRazonSocial = RazonSocialManual ?? cliente.CRazonSocial,
                    CRfc = cliente.CRfc,
                    CIdAgente = idAgenteAUsar,
                    CFechaVencimiento = fechaVencimiento,
                    CFechaProntoPago = fechaProntoPago,
                    CFechaEntregaRecepcion = fechaEntregaRecepcion,
                    CFechaUltimoInteres = fechaActual,
                    CIdMoneda = 1, // MXN por defecto
                    CTipoCambio = 1,
                    CReferencia = dto.Referencia ?? $"COT-{fechaActual:yyyyMMdd}",
                    CObservaciones = dto.Observaciones ?? "",
                    CNaturaleza = 1, // Cargo
                    CIdDocumentoOrigen = 0,
                    CPlantilla = 0,
                    CUsaCliente = 1,
                    CUsaProveedor = 0,
                    CAfectado = 1,
                    CImpreso = 0,
                    CCancelado = 0,
                    CDevuelto = 0,
                    CIdPrepoliza = 0,
                    CIdPrepolizaCancelacion = 0,
                    CEstadoContable = 0,
                    CNeto = netoDocumento,
                    CImpuesto1 = impuestoTotal,
                    CImpuesto2 = 0,
                    CImpuesto3 = 0,
                    CRetencion1 = 0,
                    CRetencion2 = 0,
                    CDescuentoMov = sumaDescuentoMovimientos,
                    CDescuentoDoc1 = descuentoDoc1Valor,
                    CDescuentoDoc2 = descuentoDoc2Valor,
                    CGasto1 = descuentoDoc3Valor, // ✅ Usamos CGasto1 para almacenar el 3er descuento
                    CGasto2 = 0,
                    CGasto3 = 0,
                    CTotal = totalDocumento,
                    CPendiente = pendiente,
                    CTotalUnidades = movimientos.Sum(m => m.CUnidades),
                    CDescuentoProntoPago = 0,
                    CPorcentajeImpuesto1 = dto.AplicarIVA ? dto.PorcentajeIVA : 0,
                    CPorcentajeImpuesto2 = 0,
                    CPorcentajeImpuesto3 = 0,
                    CPorcentajeRetencion1 = 0,
                    CPorcentajeRetencion2 = 0,
                    CPorcentajeInteres = 0,
                    CTextoExtra1 = "",
                    CTextoExtra2 = "",
                    CTextoExtra3 = "",
                    CFechaExtra = fechaActual,
                    CImporteExtra1 = 0,
                    CImporteExtra2 = 0,
                    CImporteExtra3 = 0,
                    CImporteExtra4 = 0,
                    CDestinatario = cliente.CRazonSocial,
                    CNumeroGuia = "",
                    CMensajeria = "",
                    CCuentaMensajeria = "",
                    CNumeroCajas = 0,
                    CPeso = 0,
                    CBanObservaciones = 0,
                    CBanDatosEnvio = 0,
                    CBanCondicionesCredito = 0,
                    CBanGastos = 0,
                    CUnidadesPendientes = movimientos.Sum(m => m.CUnidades),
                    CTimestamp = "",
                    CImpCheqPaq = 0,
                    CSistOrig = 205,
                    CIdMonedCa = 1,
                    CTipoCamCa = 1,
                    CEsCfd = 0,
                    CTieneCfd = 0,
                    CLugarExpe = "",
                    CMetodoPag = "",
                    CNumParcia = 0,
                    CCantParci = 0,
                    CCondiPago = "",
                    CNumCtaPag = "",
                    CGuidDocumento = Guid.NewGuid().ToString(),
                    CUsuario = "SISTEMA",
                    CIdProyecto = 0,
                    CIdCuenta = 0,
                    CTransactionId = "",
                    CIdCopiaDe = 0,
                    CVerEsque = ""
                };

                // ═══════════════════════════════════════════════════════════════
                // PASO 5: INSERTAR EN BASE DE DATOS CON TRANSACCIÓN
                // ═══════════════════════════════════════════════════════════════

                _logger.LogInformation("💾 Insertando documento y movimientos en base de datos...");

                var documentoId = await _repository.CreateDocumentoConMovimientosAsync(documento, movimientos);

                _logger.LogInformation("✅ Cotización creada exitosamente - ID: {DocumentoId}, Folio: {Folio}",
                    documentoId, documento.CFolio);

                // ═══════════════════════════════════════════════════════════════
                // PASO 6: PREPARAR RESPUESTA
                // ═══════════════════════════════════════════════════════════════

                var response = new AdmCotizacionCreateResponseDto
                {
                    IdDocumento = documentoId,
                    Serie = documento.CSerieDocumento,
                    Folio = documento.CFolio,
                    Fecha = documento.CFecha,
                    RazonSocial = documento.CRazonSocial ?? "",
                    Total = totalDocumento,
                    Pendiente = pendiente,
                    Neto = netoDocumento,
                    Impuesto = impuestoTotal,
                    CantidadProductos = movimientos.Count,
                    Mensaje = $"Cotización {documento.CSerieDocumento}-{documento.CFolio} creada exitosamente"
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear cotización para cliente {IdCliente}", dto.IdCliente);
                throw;
            }
        }

        /// <summary>
        /// Edita una cotización existente con actualización inteligente (Copy-on-Write)
        /// Soporta actualizaciones parciales (campos nulos se preservan del original)
        /// </summary>
        public async Task<AdmCotizacionCreateResponseDto> EditCotizacionAsync(AdmCotizacionUpdateDto dto)
        {
            try
            {
                _logger.LogInformation("✏️ Iniciando edición de cotización {IdDocumento}", dto.IdDocumento);

                dto.Productos ??= new List<CotizacionMovimientoUpdateDto>();

                // ═══════════════════════════════════════════════════════════════
                // PASO 1: VALIDACIONES PREVIAS Y RECUPERACIÓN ORIGINAL
                // ═══════════════════════════════════════════════════════════════

                var documentoExistente = await _repository.GetByIdWithMovimientosAsync(dto.IdDocumento);
                if (documentoExistente == null)
                    throw new InvalidOperationException($"No existe el documento con ID {dto.IdDocumento}");

                if (documentoExistente.CCancelado == 1)
                    throw new InvalidOperationException("No se puede editar una cotización cancelada");

                // Determinar Cliente: Si viene en DTO usarlo, si no, usar el original
                int idClienteFinal = dto.IdCliente ?? documentoExistente.CIdClienteProveedor;

                // Validar cliente si cambió
                if (dto.IdCliente.HasValue && !await _repository.ExistsClienteProveedorAsync(dto.IdCliente.Value))
                    throw new InvalidOperationException($"No existe un cliente con ID {dto.IdCliente}");

                // Obtener info del cliente final (sea nuevo o el original)
                var cliente = await _context.AdmClientes.AsNoTracking().FirstOrDefaultAsync(c => c.CIdClienteProveedor == idClienteFinal);
                if (cliente == null) throw new InvalidOperationException($"No se pudo obtener información del cliente {idClienteFinal}");

                // Validar agente (si cambió o original)
                int idAgenteAUsar = dto.IdAgente ?? (documentoExistente.CIdAgente > 0 ? documentoExistente.CIdAgente : 1);
                if (dto.IdAgente.HasValue && !await _repository.ExistsAgenteAsync(idAgenteAUsar))
                    throw new InvalidOperationException($"No existe un agente con ID {idAgenteAUsar}");

                // Validar productos y almacenes (Similar a Create)
                var productosInfo = new Dictionary<int, AdmProducto>();
                var movimientosDb = await _repository.GetMovimientosByDocumentoIdAsync(dto.IdDocumento);

                foreach (var productoDto in dto.Productos)
                {
                    // Determinar ID Producto: DTO o Original (si es update)
                    int idProductoFinal = 0;
                    if (productoDto.IdProducto.HasValue && productoDto.IdProducto.Value > 0)
                        idProductoFinal = productoDto.IdProducto.Value;
                    else if (productoDto.IdMovimiento.HasValue && movimientosDb.Any(m => m.CIdMovimiento == productoDto.IdMovimiento.Value))
                        idProductoFinal = movimientosDb.First(m => m.CIdMovimiento == productoDto.IdMovimiento.Value).CIdProducto;
                    else
                        throw new InvalidOperationException("Debe especificar IdProducto para nuevos movimientos");

                    if (!productosInfo.ContainsKey(idProductoFinal))
                    {
                        var prodDb = await _context.AdmProductos.AsNoTracking().FirstOrDefaultAsync(p => p.CIdProducto == idProductoFinal);
                        if (prodDb == null) throw new InvalidOperationException($"No existe un producto con ID {idProductoFinal}");
                        productosInfo.Add(idProductoFinal, prodDb);
                    }

                    // Almacén
                    int idAlmacenFinal = productoDto.IdAlmacen ?? 1; // Default 1 si es nuevo y null
                    if (productoDto.IdMovimiento.HasValue && !productoDto.IdAlmacen.HasValue && movimientosDb.Any(m => m.CIdMovimiento == productoDto.IdMovimiento.Value))
                        idAlmacenFinal = movimientosDb.First(m => m.CIdMovimiento == productoDto.IdMovimiento.Value).CIdAlmacen;

                    if (!await _repository.ExistsAlmacenAsync(idAlmacenFinal))
                        throw new InvalidOperationException($"No existe un almacén con ID {idAlmacenFinal}");
                }

                // ═══════════════════════════════════════════════════════════════
                // PASO 2: ESTRATEGIA COPY-ON-WRITE (VERSIONADO/COPIA)
                // ═══════════════════════════════════════════════════════════════
                // En lugar de editar el existente, creamos uno NUEVO basado en el anterior.

                var nuevoDocumento = new AdmDocumento
                {
                    // Copiar datos del documento original
                    CIdDocumentoDe = documentoExistente.CIdDocumentoDe,
                    CIdConceptoDocumento = documentoExistente.CIdConceptoDocumento,
                    CSerieDocumento = documentoExistente.CSerieDocumento,
                    CIdClienteProveedor = documentoExistente.CIdClienteProveedor,
                    CRazonSocial = documentoExistente.CRazonSocial,
                    CRfc = cliente.CRfc,
                    CIdAgente = documentoExistente.CIdAgente,
                    CIdMoneda = documentoExistente.CIdMoneda,
                    CTipoCambio = dto.TipoCambio ?? documentoExistente.CTipoCambio,
                    CNaturaleza = documentoExistente.CNaturaleza,
                    CIdDocumentoOrigen = documentoExistente.CIdDocumento, // ENLACE AL ORIGINAL
                    CPlantilla = documentoExistente.CPlantilla,
                    CUsaCliente = documentoExistente.CUsaCliente,
                    CUsaProveedor = documentoExistente.CUsaProveedor,
                    CAfectado = 0, // Crear como no afectado/pendiente
                    CImpreso = 0,
                    CCancelado = 0,
                    CDevuelto = 0,
                    CEstadoContable = 0,
                    CFecha = DateTime.Now.Date,
                    // Fechas: Si viene null, usar la original o recalcular
                    CFechaVencimiento = dto.FechaVencimiento ?? documentoExistente.CFechaVencimiento,
                    CFechaProntoPago = dto.FechaProntoPago ?? documentoExistente.CFechaProntoPago,
                    CFechaEntregaRecepcion = dto.FechaEntregaRecepcion ?? DateTime.Now,
                    CFechaUltimoInteres = DateTime.Now,
                    CFechaExtra = DateTime.Now,
                    CDestinatario = cliente.CRazonSocial,
                    CNumeroGuia = "",
                    CMensajeria = "",
                    CCuentaMensajeria = "",
                    // Copiar textos extra originales (importante para preservar info legacy)
                    CTextoExtra1 = documentoExistente.CTextoExtra1,
                    CTextoExtra2 = documentoExistente.CTextoExtra2,
                    CTextoExtra3 = documentoExistente.CTextoExtra3,

                    CReferencia = documentoExistente.CReferencia,
                    CObservaciones = dto.Observaciones ?? documentoExistente.CObservaciones,

                    // Campos de sistema
                    CSistOrig = 205,
                    CTimestamp = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    CGuidDocumento = Guid.NewGuid().ToString(),
                    CUsuario = "SISTEMA", // O el usuario actual
                    CLugarExpe = documentoExistente.CLugarExpe,
                    CMetodoPag = documentoExistente.CMetodoPag,
                    CCondiPago = documentoExistente.CCondiPago,
                    CNumCtaPag = documentoExistente.CNumCtaPag,
                    CTransactionId = documentoExistente.CTransactionId,
                    CVerEsque = documentoExistente.CVerEsque
                };

                var listaNuevosMovimientos = new List<AdmMovimiento>();

                double sumaNetoMovimientos = 0;
                int siguienteNumeroMovimiento = 1;

                // IVA
                double porcentajeIVA = documentoExistente.CPorcentajeImpuesto1;

                foreach (var productoDto in dto.Productos)
                {
                    // LÓGICA DE RECUPERACIÓN O DEFAULT
                    // Si IdMovimiento existe y coincide, recuperamos valores originales para lo que venga NULL

                    AdmMovimiento? original = null;
                    if (productoDto.IdMovimiento.HasValue && movimientosDb.Any(m => m.CIdMovimiento == productoDto.IdMovimiento.Value))
                    {
                        original = movimientosDb.First(m => m.CIdMovimiento == productoDto.IdMovimiento.Value);
                    }

                    // Determinar valores finales (DTO > Original > Default)
                    int idProductoFinal = productoDto.IdProducto ?? (original?.CIdProducto ?? 0);
                    int idAlmacenFinal = productoDto.IdAlmacen ?? (original?.CIdAlmacen ?? 1);
                    double unidadesFinal = productoDto.Unidades ?? (original?.CUnidades ?? 1);
                    double precioFinal = productoDto.Precio ?? (original?.CPrecio ?? 0);
                    double porcDescuentoFinal = productoDto.PorcentajeDescuento ?? (original?.CPorcentajeDescuento1 ?? 0);
                    double impDescuentoFinal = productoDto.DescuentoImporte ?? (original?.CDescuento1 ?? 0); // Ojo: CDescuento1 es importe

                    // Si venía importe de descuento en DTO, tiene prioridad sobre porcentaje
                    // Si no venía importe, calculamos basado en porcentaje (nuevo o original)

                    var productoInfo = productosInfo[idProductoFinal];

                    // Cálculos por movimiento
                    double netoMovimiento = unidadesFinal * precioFinal;
                    double descuentoMovimiento = 0;

                    // Prioridad: Importe explícito DTO > Porcentaje DTO > Original Importe > Original Porcentaje
                    if (productoDto.DescuentoImporte.HasValue && productoDto.DescuentoImporte.Value >= 0)
                    {
                        descuentoMovimiento = productoDto.DescuentoImporte.Value;
                    }
                    else if (productoDto.PorcentajeDescuento.HasValue && productoDto.PorcentajeDescuento.Value >= 0)
                    {
                        descuentoMovimiento = netoMovimiento * (productoDto.PorcentajeDescuento.Value / 100);
                    }
                    else if (original != null)
                    {
                        // Mantenemos el descuento original (Importe)
                        // Podríamos recalcularlo si cambiaron unidades/precio pero se mantuvo el porcentaje... 
                        // Estrategia segura: Si cambiaron unidades/precio, recalcular en base a porcentaje original
                        if (original.CPorcentajeDescuento1 > 0)
                            descuentoMovimiento = netoMovimiento * (original.CPorcentajeDescuento1 / 100);
                        else
                            descuentoMovimiento = original.CDescuento1; // Monto fijo original (riesgoso si cambiaron unidades)
                    }

                    double netoFinalMovimiento = netoMovimiento - descuentoMovimiento;

                    double impuestoMovimiento = 0;
                    impuestoMovimiento = netoFinalMovimiento * (porcentajeIVA / 100);

                    sumaNetoMovimientos += netoFinalMovimiento;

                    // Crear NUEVO objeto movimiento (SIEMPRE INSERTAMOS NUEVOS)
                    var nuevoMov = new AdmMovimiento();

                    if (original != null)
                    {
                        // CLONAR DATOS LEGACY DEL ORIGINAL
                        nuevoMov.CTextoExtra1 = original.CTextoExtra1;
                        nuevoMov.CTextoExtra2 = original.CTextoExtra2;
                        nuevoMov.CTextoExtra3 = original.CTextoExtra3;
                        nuevoMov.CConfImp1 = original.CConfImp1;
                        nuevoMov.CConfImp2 = original.CConfImp2;
                        nuevoMov.CIdDocumentoDe = original.CIdDocumentoDe;
                    }
                    else
                    {
                        // NUEVO - DEFAULTS
                        nuevoMov.CTextoExtra1 = "";
                        nuevoMov.CTextoExtra2 = "";
                        nuevoMov.CTextoExtra3 = "";
                        nuevoMov.CFechaExtra = DateTime.Now.Date;
                        nuevoMov.CIdDocumentoDe = documentoExistente.CIdDocumentoDe;
                        nuevoMov.CConfImp1 = 0;
                        nuevoMov.CConfImp2 = 0;
                    }

                    // Asignar valores finales
                    nuevoMov.CNumeroMovimiento = siguienteNumeroMovimiento++;
                    nuevoMov.CIdProducto = idProductoFinal;
                    nuevoMov.CIdAlmacen = idAlmacenFinal;

                    // Unidades
                    if (productoDto.IdUnidad.HasValue)
                        nuevoMov.CIdUnidad = productoDto.IdUnidad.Value;
                    else if (original != null)
                        nuevoMov.CIdUnidad = original.CIdUnidad;
                    else
                        nuevoMov.CIdUnidad = productoInfo.CIdUnidadBase;

                    nuevoMov.CUnidades = unidadesFinal;
                    nuevoMov.CUnidadesCapturadas = unidadesFinal;
                    nuevoMov.CPrecio = precioFinal;
                    nuevoMov.CPrecioCapturado = precioFinal;
                    nuevoMov.CNeto = netoFinalMovimiento;
                    nuevoMov.CImpuesto1 = impuestoMovimiento;
                    nuevoMov.CPorcentajeImpuesto1 = porcentajeIVA;
                    nuevoMov.CDescuento1 = descuentoMovimiento;
                    nuevoMov.CPorcentajeDescuento1 = porcDescuentoFinal;
                    nuevoMov.CTotal = netoFinalMovimiento + impuestoMovimiento;
                    nuevoMov.CObservaMov = productoDto.Observaciones ?? (original?.CObservaMov ?? "");

                    // Fechas y defaults obligatorios
                    nuevoMov.CFecha = DateTime.Now;
                    nuevoMov.CFechaExtra = DateTime.Now.Date; // Asegurar fecha correcta
                    nuevoMov.CUnidadesPendientes = unidadesFinal;
                    nuevoMov.CUnidadesNcPendientes = unidadesFinal;
                    nuevoMov.CUnidadesNc = unidadesFinal;
                    nuevoMov.CAfectaExistencia = 0;
                    nuevoMov.CIdUnidadNc = nuevoMov.CIdUnidad;
                    nuevoMov.CTimestamp = "";

                    listaNuevosMovimientos.Add(nuevoMov);
                }

                // ═══════════════════════════════════════════════════════════════
                // PASO 3: RECALCULAR TOTALES DOCUMENTO
                // ═══════════════════════════════════════════════════════════════

                double netoDocumento;
                double impuestoTotal;

                netoDocumento = sumaNetoMovimientos;
                impuestoTotal = netoDocumento * 0.16;

                // Ajustar totales cabecera
                nuevoDocumento.CNeto = netoDocumento;
                nuevoDocumento.CImpuesto1 = impuestoTotal;
                nuevoDocumento.CPorcentajeImpuesto1 = porcentajeIVA;
                nuevoDocumento.CDescuentoDoc1 = dto.DescuentoDoc1 ?? documentoExistente.CDescuentoDoc1;
                nuevoDocumento.CDescuentoDoc2 = dto.DescuentoDoc2 ?? documentoExistente.CDescuentoDoc2;
                // Preservar Doc3
                // si no viene en DTO, usar original (no estaba en DTO orig, pero si en modelo)

                // Recalcular Total Final
                double desc1 = nuevoDocumento.CDescuentoDoc1; // Ya tiene el valor final (DTO o Orig)
                double desc2 = nuevoDocumento.CDescuentoDoc2;

                nuevoDocumento.CTotal = netoDocumento + impuestoTotal - desc1 - desc2;

                if (dto.MontoPagado.HasValue)
                    nuevoDocumento.CPendiente = nuevoDocumento.CTotal - dto.MontoPagado.Value;
                else
                    nuevoDocumento.CPendiente = nuevoDocumento.CTotal; // O preservar lógica original de pagos? Cotizaciones suelen ser 100% pendientes

                nuevoDocumento.CTotalUnidades = listaNuevosMovimientos.Sum(m => m.CUnidades);
                nuevoDocumento.CPorcentajeImpuesto1 = porcentajeIVA;
                nuevoDocumento.CUnidadesPendientes = nuevoDocumento.CTotalUnidades;
                if (dto.IdCliente == 832)
                {
                    nuevoDocumento.CRazonSocial = dto.RazonSocial ?? documentoExistente.CRazonSocial;
                }
                // ═══════════════════════════════════════════════════════════════
                // PASO 5: CREAR NUEVA VERSIÓN (INSERT)
                // ═══════════════════════════════════════════════════════════════

                // Usamos CreateDocumentoConMovimientosAsync que ya maneja Transacción y Folios
                int nuevoId = await _repository.CreateDocumentoConMovimientosAsync(nuevoDocumento, listaNuevosMovimientos);

                // Recargar para devolver info completa con Folio asignado
                var documentoCreado = await _repository.GetByIdWithMovimientosAsync(nuevoId);

                _logger.LogInformation("✅ Versión de Cotización creada exitosamente. Original: {IdOrig}, Nueva: {IdNew}, Folio: {Folio}",
                    dto.IdDocumento, nuevoId, documentoCreado.CFolio);

                return new AdmCotizacionCreateResponseDto
                {
                    IdDocumento = documentoCreado.CIdDocumento,
                    Serie = documentoCreado.CSerieDocumento,
                    Folio = documentoCreado.CFolio,
                    Fecha = documentoCreado.CFecha,
                    RazonSocial = documentoCreado.CRazonSocial,
                    Total = documentoCreado.CTotal,
                    Pendiente = documentoCreado.CPendiente,
                    Neto = documentoCreado.CNeto,
                    Impuesto = documentoCreado.CImpuesto1,
                    CantidadProductos = listaNuevosMovimientos.Count,
                    Mensaje = $"Cotización actualizada exitosamente. Nueva versión creada con folio {documentoCreado.CFolio}"
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al editar cotización {Id}", dto.IdDocumento);
                throw;
            }
        }

        /// <summary>
        /// Cancela una cotización existente
        /// Cambia el campo CCANCELADO a 1
        /// </summary>
        public async Task<AdmCotizacionCancelarResponseDto> CancelarCotizacionAsync(AdmCotizacionCancelarDto dto)
        {
            try
            {
                _logger.LogInformation("🚫 Iniciando cancelación de documento {IdDocumento}", dto.IdDocumento);

                // Obtener el documento
                var documento = await _repository.GetDocumentoByIdAsync(dto.IdDocumento);

                if (documento == null)
                {
                    throw new InvalidOperationException($"No existe un documento con ID {dto.IdDocumento}");
                }

                if (documento.CCancelado == 1)
                {
                    throw new InvalidOperationException($"El documento {documento.CSerieDocumento}-{documento.CFolio} ya está cancelado");
                }

                // Cancelar documento
                var usuario = dto.UsuarioCancela ?? "SISTEMA";
                await _repository.CancelarDocumentoAsync(dto.IdDocumento, dto.Motivo, usuario);

                _logger.LogInformation("✅ Documento {IdDocumento} cancelado exitosamente", dto.IdDocumento);

                // Preparar respuesta
                var response = new AdmCotizacionCancelarResponseDto
                {
                    IdDocumento = documento.CIdDocumento,
                    Serie = documento.CSerieDocumento,
                    Folio = documento.CFolio,
                    RazonSocial = documento.CRazonSocial,
                    Total = documento.CTotal,
                    FechaCancelacion = DateTime.Now,
                    Motivo = dto.Motivo,
                    Usuario = usuario,
                    Mensaje = $"Cotización {documento.CSerieDocumento}-{documento.CFolio} cancelada exitosamente"
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cancelar documento {IdDocumento}", dto.IdDocumento);
                throw;
            }
        }

        /// <summary>
        /// Elimina una cotización (solo si está cancelada)
        /// </summary>
        public async Task DeleteAsync(int idDocumento)
        {
            try
            {
                _logger.LogInformation("🗑️ Iniciando eliminación de documento {IdDocumento}", idDocumento);

                // Obtener el documento
                var documento = await _repository.GetDocumentoByIdAsync(idDocumento);

                if (documento == null)
                {
                    throw new InvalidOperationException($"No existe un documento con ID {idDocumento}");
                }

                // Validar que esté cancelado
                if (documento.CCancelado != 1)
                {
                    throw new InvalidOperationException($"El documento {documento.CSerieDocumento}-{documento.CFolio} no se puede eliminar porque no está cancelado");
                }

                // Eliminar documento
                await _repository.DeleteDocumentoAsync(idDocumento);

                _logger.LogInformation("✅ Documento {IdDocumento} eliminado exitosamente", idDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS PARA REPORTES Y ESTADÍSTICAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene estadísticas generales de cotizaciones para dashboard
        /// </summary>
        public async Task<EstadisticasGeneralesDto> GetEstadisticasGeneralesAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Generar clave de caché
                var cacheKey = $"dashboard:estadisticas:{fechaInicio?.ToString("yyyyMMdd") ?? "all"}:{fechaFin?.ToString("yyyyMMdd") ?? "all"}";

                // Intentar obtener del caché
                var cached = await _cacheService.GetAsync<EstadisticasGeneralesDto>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("📦 Estadísticas obtenidas desde Redis caché");
                    return cached;
                }

                _logger.LogInformation("📊 Obteniendo estadísticas generales. Período: {FechaInicio} - {FechaFin}",
                    fechaInicio?.ToString("yyyy-MM-dd") ?? "sin límite",
                    fechaFin?.ToString("yyyy-MM-dd") ?? "sin límite");

                var estadisticas = await _repository.GetEstadisticasGeneralesAsync(fechaInicio, fechaFin);

                // Guardar en caché por 5 minutos
                await _cacheService.SetAsync(cacheKey, estadisticas, TimeSpan.FromMinutes(5));

                _logger.LogInformation("✅ Estadísticas obtenidas: {Total} cotizaciones, ${Monto:N2} total, {Clientes} clientes únicos",
                    estadisticas.TotalCotizaciones,
                    estadisticas.MontoTotal,
                    estadisticas.ClientesUnicos);

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener estadísticas generales");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el top N de clientes con más cotizaciones y montos
        /// </summary>
        public async Task<List<TopClienteDto>> GetTopClientesAsync(int top, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Validar top
                if (top < 1) top = 10;
                if (top > 100) top = 100;

                // Generar clave de caché
                var cacheKey = $"dashboard:topclientes:{top}:{fechaInicio?.ToString("yyyyMMdd") ?? "all"}:{fechaFin?.ToString("yyyyMMdd") ?? "all"}";

                // Intentar obtener del caché
                var cached = await _cacheService.GetAsync<List<TopClienteDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("📦 Top clientes obtenidos desde Redis caché");
                    return cached;
                }

                _logger.LogInformation("📊 Obteniendo top {Top} clientes. Período: {FechaInicio} - {FechaFin}",
                    top,
                    fechaInicio?.ToString("yyyy-MM-dd") ?? "sin límite",
                    fechaFin?.ToString("yyyy-MM-dd") ?? "sin límite");

                var topClientes = await _repository.GetTopClientesAsync(top, fechaInicio, fechaFin);

                // Guardar en caché por 5 minutos
                await _cacheService.SetAsync(cacheKey, topClientes, TimeSpan.FromMinutes(5));

                _logger.LogInformation("✅ Top clientes obtenido: {Count} registros",
                    topClientes.Count);

                return topClientes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener top clientes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene cotizaciones próximas a vencer en los próximos N días con paginación
        /// </summary>
        public async Task<(List<CotizacionVencimientoDto> items, int total)> GetProximasVencerAsync(int dias, int page, int pageSize)
        {
            try
            {
                // Validar días
                if (dias < 1) dias = 7;
                if (dias > 90) dias = 90;
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;

                _logger.LogInformation("📊 Obteniendo cotizaciones próximas a vencer en {Dias} días (Página {Page})", dias, page);

                var resultado = await _repository.GetProximasVencerAsync(dias, page, pageSize);

                _logger.LogInformation("✅ {Count} cotizaciones próximas a vencer obtenidas", resultado.items.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cotizaciones próximas a vencer");
                throw;
            }
        }

        /// <summary>
        /// Obtiene rendimiento por agente de ventas
        /// </summary>
        public async Task<List<RendimientoAgenteDto>> GetRendimientoAgentesAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Validar fechas
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    throw new ArgumentException("La fecha inicial no puede ser mayor a la fecha final");
                }

                // Generar clave de caché
                var cacheKey = $"dashboard:agentes:{fechaInicio?.ToString("yyyyMMdd") ?? "all"}:{fechaFin?.ToString("yyyyMMdd") ?? "all"}";

                // Intentar obtener del caché
                var cached = await _cacheService.GetAsync<List<RendimientoAgenteDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("📦 Rendimiento de agentes obtenido desde Redis caché");
                    return cached;
                }

                _logger.LogInformation("📊 Obteniendo rendimiento de agentes desde {FechaInicio} hasta {FechaFin}",
                    fechaInicio?.ToShortDateString() ?? "inicio", fechaFin?.ToShortDateString() ?? "fin");

                var rendimiento = await _repository.GetRendimientoAgentesAsync(fechaInicio, fechaFin);

                // Guardar en caché por 5 minutos
                await _cacheService.SetAsync(cacheKey, rendimiento, TimeSpan.FromMinutes(5));

                _logger.LogInformation("✅ Rendimiento de {Count} agentes obtenido", rendimiento.Count);

                return rendimiento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener rendimiento de agentes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos más cotizados por frecuencia y volumen
        /// </summary>
        public async Task<List<ProductoCotizadoDto>> GetProductosMasCotizadosAsync(int top, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Validar parámetros
                if (top < 1) top = 10;
                if (top > 100) top = 100;

                // Validar fechas
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    throw new ArgumentException("La fecha inicial no puede ser mayor a la fecha final");
                }

                // Si no se especifican fechas, NO usar defaults - usar null para obtener todo
                // Esto permite al frontend decidir el rango

                // Generar clave de caché
                var cacheKey = $"dashboard:productos:{top}:{fechaInicio?.ToString("yyyyMMdd") ?? "all"}:{fechaFin?.ToString("yyyyMMdd") ?? "all"}";

                // Intentar obtener del caché
                var cached = await _cacheService.GetAsync<List<ProductoCotizadoDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("📦 Productos obtenidos desde Redis caché");
                    return cached;
                }

                _logger.LogInformation("📊 Obteniendo top {Top} productos más cotizados desde {FechaInicio} hasta {FechaFin}",
                    top, fechaInicio?.ToShortDateString() ?? "inicio", fechaFin?.ToShortDateString() ?? "fin");

                var productos = await _repository.GetProductosMasCotizadosAsync(top, fechaInicio, fechaFin);

                // Guardar en caché por 5 minutos
                await _cacheService.SetAsync(cacheKey, productos, TimeSpan.FromMinutes(5));

                _logger.LogInformation("✅ {Count} productos más cotizados obtenidos", productos.Count);

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener productos más cotizados");
                throw;
            }
        }

        /// <summary>
        /// Obtiene distribución de cotizaciones por rangos de monto
        /// </summary>
        public async Task<List<CotizacionPorRangoDto>> GetCotizacionesPorRangoMontoAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Validar fechas
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value > fechaFin.Value)
                {
                    throw new ArgumentException("La fecha inicial no puede ser mayor a la fecha final");
                }

                // Generar clave de caché
                var cacheKey = $"dashboard:rangos:{fechaInicio?.ToString("yyyyMMdd") ?? "all"}:{fechaFin?.ToString("yyyyMMdd") ?? "all"}";

                // Intentar obtener del caché
                var cached = await _cacheService.GetAsync<List<CotizacionPorRangoDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("📦 Rangos de monto obtenidos desde Redis caché");
                    return cached;
                }

                _logger.LogInformation("📊 Obteniendo distribución de cotizaciones por rangos de monto desde {FechaInicio} hasta {FechaFin}",
                    fechaInicio?.ToShortDateString() ?? "inicio", fechaFin?.ToShortDateString() ?? "fin");

                var rangos = await _repository.GetCotizacionesPorRangoMontoAsync(fechaInicio, fechaFin);

                // Guardar en caché por 5 minutos
                await _cacheService.SetAsync(cacheKey, rangos, TimeSpan.FromMinutes(5));

                _logger.LogInformation("✅ Distribución por rangos obtenida: {Count} rangos analizados", rangos.Count);

                return rangos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cotizaciones por rango de monto");
                throw;
            }
        }
        /// <summary>
        /// Obtiene estadísticas de cotizaciones agrupadas por mes para un año específico
        /// </summary>
        public async Task<List<AdmEstadisticasMensualesDto>> GetEstadisticasMensualesAsync(int anio)
        {
            try
            {
                // Generar clave de caché (v2 para limpiar datos incorrectos previos)
                var cacheKey = $"dashboard:mensuales:v2:{anio}";

                // Intentar obtener del caché
                var cached = await _cacheService.GetAsync<List<AdmEstadisticasMensualesDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("📦 Estadísticas mensuales obtenidas desde Redis caché");
                    return cached;
                }

                _logger.LogInformation("📊 Obteniendo estadísticas mensuales para el año {Anio}", anio);

                var fechaInicio = new DateTime(anio, 1, 1);
                var fechaFin = new DateTime(anio, 12, 31, 23, 59, 59);

                // Consulta base: filtrar por fecha y tipo de documento (Cotizaciones = Serie 'CA')
                var query = _context.AdmDocumentos
                    .AsNoTracking()
                    .Where(d => d.CFecha >= fechaInicio && d.CFecha <= fechaFin && d.CSerieDocumento == "CA");

                // Ejecutar agrupación en memoria para evitar problemas con funciones de fecha SQL en algunas versiones de EF Core
                // Ojo: Si son muchos registros, esto podría ser pesado.
                // Optimización: Traer solo las columnas necesarias
                var datosRaw = await query
                    .Select(d => new { d.CFecha, d.CCancelado, d.CTotal })
                    .ToListAsync();

                var resultado = datosRaw
                    .GroupBy(d => d.CFecha.Month)
                    .Select(g => new AdmEstadisticasMensualesDto
                    {
                        Mes = g.Key,
                        NombreMes = new DateTime(anio, g.Key, 1).ToString("MMMM", new System.Globalization.CultureInfo("es-ES")),
                        CotizacionesActivas = g.Count(x => x.CCancelado == 0),
                        CotizacionesCanceladas = g.Count(x => x.CCancelado != 0),
                        MontoTotal = (decimal)g.Where(x => x.CCancelado == 0).Sum(x => x.CTotal) // Solo sumar montos de activas
                    })
                    .OrderBy(x => x.Mes)
                    .ToList();

                // Rellenar meses faltantes con ceros
                var listaCompleta = new List<AdmEstadisticasMensualesDto>();
                var cultura = new System.Globalization.CultureInfo("es-ES");

                for (int mes = 1; mes <= 12; mes++)
                {
                    var datoMes = resultado.FirstOrDefault(r => r.Mes == mes);
                    if (datoMes != null)
                    {
                        // Capitalizar nombre del mes
                        datoMes.NombreMes = cultura.TextInfo.ToTitleCase(datoMes.NombreMes);
                        listaCompleta.Add(datoMes);
                    }
                    else
                    {
                        listaCompleta.Add(new AdmEstadisticasMensualesDto
                        {
                            Mes = mes,
                            NombreMes = cultura.TextInfo.ToTitleCase(new DateTime(anio, mes, 1).ToString("MMMM", cultura)),
                            CotizacionesActivas = 0,
                            CotizacionesCanceladas = 0,
                            MontoTotal = 0
                        });
                    }
                }

                // Guardar en caché por 10 minutos (los datos históricos cambian poco, los del mes actual sí)
                await _cacheService.SetAsync(cacheKey, listaCompleta, TimeSpan.FromMinutes(10));

                _logger.LogInformation("✅ Estadísticas mensuales calculadas exitosamente");

                return listaCompleta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener estadísticas mensuales");
                throw;
            }
        }
    }
}
