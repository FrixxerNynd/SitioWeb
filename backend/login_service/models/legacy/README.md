# 📋 PLAN DE DESARROLLO - APIS LEGACY ADMINPAQ (adCABS2016)

## 🎯 Objetivo

Implementar APIs REST robustas, escalables y profesionales para exponer datos del sistema legacy **Adminpaq** (base de datos `adCABS2016` en instancia `Adrian\COMPAC`) siguiendo arquitectura limpia con patrones Repository-Service-Controller.

---

## 📊 ANÁLISIS DE DEPENDENCIAS

### Tablas Modeladas (11 en total)

| # | Tabla | Dependencias | Tipo |
|---|-------|-------------|------|
| 1 | `admMonedas` | **Ninguna** | Catálogo base |
| 2 | `admAgentes` | **Ninguna** | Catálogo base |
| 3 | `admAlmacenes` | **Ninguna** | Catálogo base |
| 4 | `admUnidadesMedidaPeso` | **Ninguna** | Catálogo base |
| 5 | `admDocumentosModelo` | **Ninguna** | Catálogo base |
| 6 | `admProductos` | admUnidadesMedidaPeso, admMonedas | Catálogo dependiente |
| 7 | `admConceptos` | admDocumentosModelo, admMonedas | Catálogo dependiente |
| 8 | `admNumerosSerie` | admProductos, admAlmacenes | Maestro-Detalle |
| 9 | `admDocumentos` | admDocumentosModelo, admConceptos, admAgentes, admMonedas | Transaccional |
| 10 | `admMovimientos` | admDocumentos, admProductos, admAlmacenes, admUnidadesMedidaPeso | Transaccional |
| 11 | `admMovimientosSerie` | admMovimientos, admNumerosSerie | Relación N:N |

---

## 🔄 ORDEN DE IMPLEMENTACIÓN SUGERIDO

### **FASE 1: Catálogos Base (Sin dependencias)**
Implementar primero las tablas que NO dependen de ninguna otra:

#### 1️⃣ **AdmMonedas** 
- **Modelo:** `AdmMoneda.cs` ✅ Creado
- **Propósito:** Catálogo de monedas (MXN, USD, EUR, etc.)
- **Endpoints:**
  - `GET /api/AdmMonedas/paginated` (paginado)
  - `GET /api/AdmMonedas/search/paginated?q={term}` (búsqueda paginada)
- **Cache Redis:** 30 min (GetAll), 60 min (Search)
- **Métodos HTTP:** `GET` solamente (solo lectura)
- **Prioridad:** ALTA (muchas tablas dependen de ella)

#### 2️⃣ **AdmAgentes** ✅ **YA IMPLEMENTADO**
- **Modelo:** `AdmAgente.cs` ✅ Creado
- **Propósito:** Catálogo de agentes/vendedores
- **Endpoints:** 
  - `GET /api/AdmAgentes/paginated` ✅ Funcional
  - `GET /api/AdmAgentes/search/paginated` ✅ Funcional
- **Estado:** ✅ Completo (Repository, Service, Controller, Cache)

#### 3️⃣ **AdmAlmacenes**
- **Modelo:** `AdmAlmacen.cs` ✅ Creado
- **Propósito:** Catálogo de almacenes/ubicaciones
- **Endpoints:**
  - `GET /api/AdmAlmacenes/paginated`
  - `GET /api/AdmAlmacenes/search/paginated?q={term}`
- **Cache Redis:** 30 min (GetAll), 60 min (Search)
- **Métodos HTTP:** `GET` solamente
- **Prioridad:** ALTA (productos y números de serie dependen)

#### 4️⃣ **AdmUnidadesMedidaPeso**
- **Modelo:** `AdmUnidadMedidaPeso.cs` ✅ Creado
- **Propósito:** Catálogo de unidades de medida (KG, PZA, LT, etc.)
- **Endpoints:**
  - `GET /api/AdmUnidadesMedidaPeso/paginated`
  - `GET /api/AdmUnidadesMedidaPeso/search/paginated?q={term}`
- **Cache Redis:** 30 min (GetAll), 60 min (Search)
- **Métodos HTTP:** `GET` solamente
- **Prioridad:** ALTA (productos y movimientos dependen)

#### 5️⃣ **AdmDocumentosModelo**
- **Modelo:** `AdmDocumentoModelo.cs` ✅ Creado
- **Propósito:** Modelos de documentos (Factura, Nota de Crédito, etc.)
- **Endpoints:**
  - `GET /api/AdmDocumentosModelo/paginated`
  - `GET /api/AdmDocumentosModelo/search/paginated?q={term}`
- **Cache Redis:** 30 min (GetAll), 60 min (Search)
- **Métodos HTTP:** `GET` solamente
- **Prioridad:** ALTA (conceptos y documentos dependen)

---

### **FASE 2: Catálogos Dependientes**
Implementar tablas que dependen de catálogos base:

#### 6️⃣ **AdmProductos**
- **Modelo:** `AdmProducto.cs` ✅ Creado
- **Dependencias:** 
  - `admUnidadesMedidaPeso` (CIDUNIDADBASE, CIDUNIDADCOMPRA, CIDUNIDADVENTA)
  - `admMonedas` (CIDMONEDA)
- **Endpoints:**
  - `GET /api/AdmProductos/paginated`
  - `GET /api/AdmProductos/search/paginated?q={term}` (buscar por código o nombre)
  - `GET /api/AdmProductos/{id}` (detalle individual)
- **Cache Redis:** 30 min (GetAll), 60 min (Search)
- **Métodos HTTP:** `GET` solamente
- **Prioridad:** ALTA (números de serie y movimientos dependen)
- **Nota:** Tabla grande (muchos productos), optimizar consultas con índices

#### 7️⃣ **AdmConceptos**
- **Modelo:** `AdmConcepto.cs` ✅ Creado
- **Dependencias:**
  - `admDocumentosModelo` (CIDDOCUMENTODE)
  - `admMonedas` (CIDMONEDA)
- **Endpoints:**
  - `GET /api/AdmConceptos/paginated`
  - `GET /api/AdmConceptos/search/paginated?q={term}`
  - `GET /api/AdmConceptos/byDocumentoModelo/{idModelo}` (filtrar por modelo)
- **Cache Redis:** 30 min (GetAll), 60 min (Search)
- **Métodos HTTP:** `GET` solamente
- **Prioridad:** MEDIA (documentos dependen)

---

### **FASE 3: Maestro-Detalle e Inventario**
Implementar tablas de inventario y relaciones:

#### 8️⃣ **AdmNumerosSerie**
- **Modelo:** `AdmNumeroSerie.cs` ✅ Creado
- **Dependencias:**
  - `admProductos` (CIDPRODUCTO)
  - `admAlmacenes` (CIDALMACEN)
- **Endpoints:**
  - `GET /api/AdmNumerosSerie/paginated`
  - `GET /api/AdmNumerosSerie/search/paginated?q={term}` (buscar por número de serie)
  - `GET /api/AdmNumerosSerie/byProducto/{idProducto}` (series de un producto)
  - `GET /api/AdmNumerosSerie/byAlmacen/{idAlmacen}` (series en un almacén)
  - `GET /api/AdmNumerosSerie/disponibles` (solo series disponibles CESTADO=1)
- **Cache Redis:** 15 min (GetAll), 30 min (Search) - Cache más corto por volatilidad
- **Métodos HTTP:** `GET` solamente
- **Prioridad:** MEDIA (movimientos serie dependen)
- **Nota:** Tabla muy grande (15,495+ registros), SIEMPRE usar paginación

---

### **FASE 4: Transaccionales (Documentos y Movimientos)**
Implementar operaciones de negocio principales:

#### 9️⃣ **AdmDocumentos** ⚠️ **CRÍTICO**
- **Modelo:** `AdmDocumento.cs` ✅ Creado
- **Dependencias:**
  - `admDocumentosModelo` (CIDDOCUMENTODE)
  - `admConceptos` (CIDCONCEPTODOCUMENTO)
  - `admAgentes` (CIDAGENTE)
  - `admMonedas` (CIDMONEDA, CIDMONEDCA)
  - `admClientes/admProveedores` (CIDCLIENTEPROVEEDOR - externo)
- **Endpoints:**
  - `GET /api/AdmDocumentos/paginated`
  - `GET /api/AdmDocumentos/search/paginated?q={term}` (buscar por folio, RFC, razón social)
  - `GET /api/AdmDocumentos/{id}` (detalle completo)
  - `GET /api/AdmDocumentos/byConcepto/{idConcepto}` (filtrar por concepto)
  - `GET /api/AdmDocumentos/byAgente/{idAgente}` (documentos de un agente)
  - `GET /api/AdmDocumentos/byFechas?desde={fecha}&hasta={fecha}` (rango de fechas)
  - `POST /api/AdmDocumentos` ⚠️ (crear documento - **usar con precaución**)
  - `PUT /api/AdmDocumentos/{id}` ⚠️ (actualizar - **validar integridad**)
  - `DELETE /api/AdmDocumentos/{id}` ⚠️ (cancelar - **soft delete CCANCELADO=1**)
- **Cache Redis:** 10 min (GetAll), 20 min (Search) - Cache corto (datos transaccionales)
- **Métodos HTTP:** `GET`, `POST`, `PUT`, `DELETE`
- **Prioridad:** ALTA (core del negocio)
- **Seguridad:** 
  - Validar que CCANCELADO=0 antes de modificar
  - Auditoría de cambios
  - Transacciones para mantener integridad

#### 🔟 **AdmMovimientos** ⚠️ **CRÍTICO**
- **Modelo:** `AdmMovimiento.cs` ✅ Creado
- **Dependencias:**
  - `admDocumentos` (CIDDOCUMENTO)
  - `admProductos` (CIDPRODUCTO)
  - `admAlmacenes` (CIDALMACEN)
  - `admUnidadesMedidaPeso` (CIDUNIDAD, CIDUNIDADNC)
- **Endpoints:**
  - `GET /api/AdmMovimientos/paginated`
  - `GET /api/AdmMovimientos/search/paginated?q={term}`
  - `GET /api/AdmMovimientos/{id}` (detalle)
  - `GET /api/AdmMovimientos/byDocumento/{idDocumento}` (movimientos de un documento)
  - `GET /api/AdmMovimientos/byProducto/{idProducto}` (historial de producto)
  - `GET /api/AdmMovimientos/byAlmacen/{idAlmacen}` (movimientos de almacén)
  - `POST /api/AdmMovimientos` ⚠️ (crear movimiento)
  - `PUT /api/AdmMovimientos/{id}` ⚠️ (actualizar)
  - `DELETE /api/AdmMovimientos/{id}` ⚠️ (eliminar - **validar que documento no esté afectado**)
- **Cache Redis:** 10 min (GetAll), 20 min (Search)
- **Métodos HTTP:** `GET`, `POST`, `PUT`, `DELETE`
- **Prioridad:** ALTA (inventario en tiempo real)
- **Seguridad:**
  - Validar CAFECTAEXISTENCIA antes de modificar
  - Transacciones atómicas
  - Auditoría completa

#### 1️⃣1️⃣ **AdmMovimientosSerie** ⚠️ **RELACIÓN N:N**
- **Modelo:** `AdmMovimientoSerie.cs` ✅ Creado
- **Dependencias:**
  - `admMovimientos` (CIDMOVIMIENTO)
  - `admNumerosSerie` (CIDSERIE)
- **Endpoints:**
  - `GET /api/AdmMovimientosSerie/paginated`
  - `GET /api/AdmMovimientosSerie/byMovimiento/{idMovimiento}` (series de un movimiento)
  - `GET /api/AdmMovimientosSerie/bySerie/{idSerie}` (historial de una serie)
- **Cache Redis:** 10 min (volátil)
- **Métodos HTTP:** `GET` solamente
- **Prioridad:** BAJA (consulta especializada)
- **Nota:** Clave primaria compuesta (CIDMOVIMIENTO, CIDSERIE)

---

## 🏗️ ARQUITECTURA Y PATRONES

### Estructura de Carpetas
```
back_cabs/CRM/
├── models/
│   └── legacy/
│       ├── AdmAgente.cs           ✅ Creado
│       ├── AdmMoneda.cs           ✅ Creado
│       ├── AdmAlmacen.cs          ✅ Creado
│       ├── AdmUnidadMedidaPeso.cs ✅ Creado
│       ├── AdmProducto.cs         ✅ Creado
│       ├── AdmDocumentoModelo.cs  ✅ Creado
│       ├── AdmConcepto.cs         ✅ Creado
│       ├── AdmNumeroSerie.cs      ✅ Creado
│       ├── AdmDocumento.cs        ✅ Creado
│       ├── AdmMovimiento.cs       ✅ Creado
│       └── AdmMovimientoSerie.cs  ✅ Creado
├── DTOs/
│   └── legacy/
│       ├── AdmAgenteResponseDto.cs        ✅ Creado
│       ├── AdmMonedaResponseDto.cs        ⏳ Pendiente
│       ├── AdmAlmacenResponseDto.cs       ⏳ Pendiente
│       ├── AdmProductoResponseDto.cs      ⏳ Pendiente
│       ├── AdmConceptoResponseDto.cs      ⏳ Pendiente
│       ├── AdmNumeroSerieResponseDto.cs   ⏳ Pendiente
│       ├── AdmDocumentoResponseDto.cs     ⏳ Pendiente
│       └── AdmMovimientoResponseDto.cs    ⏳ Pendiente
├── repositories/
│   └── legacy/
│       ├── IAdmAgenteRepository.cs        ✅ Creado
│       ├── AdmAgenteRepository.cs         ✅ Creado
│       └── ... (pendientes)
├── services/
│   └── legacy/
│       ├── IAdmAgenteService.cs           ✅ Creado
│       ├── AdmAgenteService.cs            ✅ Creado
│       └── ... (pendientes)
├── controllers/
│   └── legacy/
│       ├── AdmAgentesController.cs        ✅ Creado
│       └── ... (pendientes)
└── contexts/
    ├── LegacyCompacContext.cs             ✅ Actualizado (ReadOnly)
    └── LegacyCompacWriteContext.cs        ✅ Actualizado (Write)
```

### Patrón de Implementación (Seguir AdmAgentes como referencia)

#### 1️⃣ **Modelo (models/legacy/)**
```csharp
[Table("admTabla")]
public class AdmTabla
{
    [Key]
    [Column("CIDCAMPO")]
    public int CIdCampo { get; set; }
    
    // ... propiedades con Data Annotations
    
    // Propiedades de navegación (marcadas como Ignore en contexto)
    [ForeignKey("CIdRelacion")]
    public AdmRelacionada? Relacion { get; set; }
}
```

#### 2️⃣ **DTO (DTOs/legacy/)**
```csharp
public class AdmTablaResponseDto
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    // Solo campos esenciales para respuestas
    // Propiedades computed si es necesario
}
```

#### 3️⃣ **Repository Interface (repositories/legacy/)**
```csharp
public interface IAdmTablaRepository
{
    Task<(List<AdmTabla> Items, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize);
    Task<(List<AdmTabla> Items, int TotalRecords)> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
}
```

#### 4️⃣ **Repository Implementation**
```csharp
public class AdmTablaRepository : IAdmTablaRepository
{
    private readonly LegacyCompacReadOnlyContext _context;

    public async Task<(List<AdmTabla> Items, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize)
    {
        var query = _context.AdmTablas.AsNoTracking();
        var totalRecords = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.Codigo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (items, totalRecords);
    }
    
    // ... implementar SearchPaginatedAsync
}
```

#### 5️⃣ **Service Interface (services/legacy/)**
```csharp
public interface IAdmTablaService
{
    Task<PaginatedResponseDto<AdmTablaResponseDto>> GetAllPaginatedAsync(int page, int pageSize);
    Task<PaginatedResponseDto<AdmTablaResponseDto>> SearchPaginatedAsync(string searchTerm, int page, int pageSize);
}
```

#### 6️⃣ **Service Implementation**
```csharp
public class AdmTablaService : IAdmTablaService
{
    private readonly IAdmTablaRepository _repository;
    private readonly ICacheService _cacheService;
    private const string CACHE_KEY_ALL = "adm_tabla:all:page:{0}:size:{1}";
    private const int CACHE_TTL_GET_ALL = 30; // minutos
    
    public async Task<PaginatedResponseDto<AdmTablaResponseDto>> GetAllPaginatedAsync(int page, int pageSize)
    {
        // Validar pageSize
        pageSize = pageSize < 1 ? 30 : (pageSize > 100 ? 100 : pageSize);
        
        var cacheKey = string.Format(CACHE_KEY_ALL, page, pageSize);
        
        try
        {
            var cachedData = await _cacheService.GetAsync<PaginatedResponseDto<AdmTablaResponseDto>>(cacheKey);
            if (cachedData != null) return cachedData;
            
            var (items, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize);
            var dtos = items.Select(MapToDto).ToList();
            
            var response = new PaginatedResponseDto<AdmTablaResponseDto>
            {
                Items = dtos,
                Pagina = page,
                ResultadosPorPagina = pageSize,
                TotalItems = totalRecords,
                TotalPaginas = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };
            
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(CACHE_TTL_GET_ALL));
            return response;
        }
        catch
        {
            // Fallback sin cache
            var (items, totalRecords) = await _repository.GetAllPaginatedAsync(page, pageSize);
            var dtos = items.Select(MapToDto).ToList();
            return new PaginatedResponseDto<AdmTablaResponseDto> { /* ... */ };
        }
    }
    
    private AdmTablaResponseDto MapToDto(AdmTabla entity)
    {
        return new AdmTablaResponseDto
        {
            Id = entity.CIdCampo,
            Codigo = entity.CCodigo,
            Nombre = entity.CNombre
        };
    }
}
```

#### 7️⃣ **Controller (controllers/legacy/)**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMINISTRACION,RECEPCION")]
public class AdmTablasController : ControllerBase
{
    private readonly IAdmTablaService _service;
    private readonly ILogger<AdmTablasController> _logger;

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(PaginatedResponseDto<AdmTablaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _service.GetAllPaginatedAsync(page, pageSize);
            stopwatch.Stop();
            
            _logger.LogInformation("GET AdmTablas paginated: {Count} items, page {Page}, {Ms}ms", 
                result.Items.Count, page, stopwatch.ElapsedMilliseconds);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AdmTablas paginated");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
```

#### 8️⃣ **Registro en Program.cs**
```csharp
// Registrar Repository
builder.Services.AddScoped<IAdmTablaRepository, AdmTablaRepository>();

// Registrar Service
builder.Services.AddScoped<IAdmTablaService, AdmTablaService>();
```

---

## 🔒 CONSIDERACIONES DE SEGURIDAD

### ⚠️ Operaciones de Solo Lectura (Fases 1-3)
- ✅ Usar `LegacyCompacReadOnlyContext`
- ✅ Solo endpoints `GET`
- ✅ Cache agresivo (30-60 min)
- ✅ No hay riesgo de corrupción de datos

### ⚠️ Operaciones de Escritura (Fase 4)
- 🔴 Usar `LegacyCompacWriteContext` con extrema precaución
- 🔴 Validar integridad referencial antes de insertar/actualizar
- 🔴 Implementar transacciones con `_context.Database.BeginTransactionAsync()`
- 🔴 Auditoría completa (quién, cuándo, qué cambió)
- 🔴 Soft deletes preferibles (CCANCELADO=1 en lugar de DELETE físico)
- 🔴 Validar estados (CAFECTADO, CIMPRESO, CCANCELADO)
- 🔴 Autorización estricta (solo ADMINISTRACION)

---

## 📈 OPTIMIZACIÓN Y RENDIMIENTO

### Paginación Obligatoria
```csharp
// ✅ BUENO
GET /api/AdmProductos/paginated?page=1&pageSize=30

// ❌ MALO (NUNCA hacer esto con tablas grandes)
GET /api/AdmProductos  // retornaría miles de registros
```

### Cache Redis Estratégico
| Tipo de Dato | TTL Recomendado | Razón |
|--------------|-----------------|-------|
| Catálogos base (Monedas, Agentes) | 30-60 min | Raramente cambian |
| Productos, Almacenes | 20-30 min | Cambios moderados |
| Números de Serie | 10-15 min | Inventario volátil |
| Documentos, Movimientos | 5-10 min | Transaccional |

### Índices SQL Server
Asegurar índices en:
- Claves primarias (ya existen)
- Claves foráneas (CIDPRODUCTO, CIDALMACEN, etc.)
- Campos de búsqueda (CCODIGOPRODUCTO, CNOMBREPRODUCTO)
- Campos de filtro (CFECHA, CNATURALEZA, CESTADO)

---

## 🧪 TESTING

### Orden de Pruebas
1. **Unit Tests:** Servicios con repositorios mockeados
2. **Integration Tests:** Endpoints contra BD de pruebas
3. **Load Tests:** Paginación con 10,000+ registros
4. **E2E Tests:** Flujo completo Documento → Movimientos → Series

### Casos de Prueba Críticos
- ✅ Paginación con página inexistente (debe retornar vacío)
- ✅ Búsqueda sin resultados
- ✅ Cache hit/miss
- ✅ Validación de pageSize (min=1, max=100)
- ⚠️ Transacciones rollback en caso de error
- ⚠️ Soft delete no afecta registros relacionados

---

## 📝 CHECKLIST DE IMPLEMENTACIÓN

Por cada API nueva:

### Desarrollo
- [ ] Crear DTO con campos esenciales
- [ ] Crear interfaz de Repository
- [ ] Implementar Repository con paginación
- [ ] Crear interfaz de Service
- [ ] Implementar Service con cache Redis
- [ ] Crear Controller con endpoints paginados
- [ ] Registrar en `Program.cs`
- [ ] Agregar logs con `ILogger`

### Testing
- [ ] Probar con Swagger/Postman
- [ ] Verificar cache MISS → SET
- [ ] Verificar cache HIT
- [ ] Probar paginación (página 1, 2, última)
- [ ] Probar búsqueda
- [ ] Validar performance (< 3 segundos)

### Documentación
- [ ] Comentarios XML en código
- [ ] Ejemplos en Swagger
- [ ] Actualizar README si hay cambios

---

## 🚀 CRONOGRAMA SUGERIDO

| Semana | APIs a Implementar | Estimación |
|--------|-------------------|------------|
| 1 | AdmMonedas, AdmAlmacenes, AdmUnidadesMedidaPeso | 12 horas |
| 2 | AdmDocumentosModelo, AdmProductos | 16 horas |
| 3 | AdmConceptos, AdmNumerosSerie | 16 horas |
| 4 | AdmDocumentos (solo GET) | 12 horas |
| 5 | AdmMovimientos (solo GET), AdmMovimientosSerie | 16 horas |
| 6 | AdmDocumentos/AdmMovimientos (POST/PUT/DELETE) ⚠️ | 24 horas |
| **TOTAL** | **11 APIs** | **96 horas (~2.5 meses)** |

---

## 🎓 REFERENCIAS

### Código de Referencia
- ✅ **AdmAgentes:** Implementación completa y funcional
  - `AdmAgente.cs`
  - `AdmAgenteResponseDto.cs`
  - `AdmAgenteRepository.cs`
  - `AdmAgenteService.cs`
  - `AdmAgentesController.cs`

### Documentación
- [EF Core Best Practices](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core REST API](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api)
- [Redis Caching](https://redis.io/docs/manual/client-side-caching/)

---

## 📞 CONTACTO Y SOPORTE

Para dudas o problemas durante la implementación:
1. Revisar logs en `back_cabs/logs/`
2. Verificar conexión a BD legacy con SQL Server Management Studio
3. Consultar este README

**Última actualización:** Noviembre 13, 2025  
**Versión:** 1.0  
**Estado:** 1 de 11 APIs completadas (AdmAgentes ✅)
