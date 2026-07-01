# SO 2.6 - 30-06-26

## Resumen

Múltiples ajustes en backend y frontend.

## 1. Pre-registro envío de constancia fiscal en FormData — Frontend

**Archivo:** `front-end/src/app/auth/pre-registro/pre-registro.service.ts`
**Archivo:** `front-end/src/app/auth/pre-registro/pages/datos-fiscales/datos-fiscales.ts`
**Archivo:** `front-end/src/app/auth/pre-registro/pages/datos-fiscales/datos-fiscales.html`

### Problema

Por parte del frontend, el asistente de pre-registro enviaba los datos del formulario como un objeto JSON plano (`application/json`), lo que impedía que el archivo PDF/imagen de la constancia fiscal fuera transmitido al backend. Aunque en el primer paso del wizard se capturaba el archivo de la constancia, este no se almacenaba en el estado persistido (localStorage solo admite texto) ni se adjuntaba a la petición HTTP final.

### Solución

1. Modificar `datos-fiscales.ts` para inicializar el formulario con validación obligatoria (`Validators.required`) en el campo de la constancia fiscal, garantizando que el usuario deba subir el archivo.
2. Modificar el archivo `datos-fiscales.html` removiendo la directiva `formControlName="constancia"` del input de tipo archivo. Esto previene que Angular intente programáticamente volver a escribir el objeto `File` en el input del navegador (lo cual está bloqueado por seguridad y generaba un error `DOMException`).
3. Modificar `datos-fiscales.ts` en `onFileSelected(event)` para que asigne manualmente el archivo al control del formulario y lo guarde en memoria en el servicio `PreRegistroService` (el cual actúa como un singleton persistente en memoria). En caso de cancelar la selección, se limpia el control a `null`.
4. Reescribir el método `registrar()` en `pre-registro.service.ts` para que ensamble y envíe un objeto `FormData` (`multipart/form-data`) en lugar de JSON.
5. Aplanar los campos del formulario y los del objeto anidado de la dirección (`direccion.calle`, `direccion.colonia`, etc.) usando notación de puntos, e incorporar el archivo binario del PDF bajo la clave `constanciaFiscal`.

### Código — Antes

**`pre-registro.service.ts`**

```typescript
  registrar(): Observable<RegistroClienteResponse> {
    const estado = this.obtenerEstado();

    const payload = {
      nombre: estado.nombre ?? '',
      apellidoPaterno: estado.apellidoPaterno ?? '',
      apellidoMaterno: estado.apellidoMaterno ?? '',
      RFC: estado.RFC ?? '',
      CURP: estado.CURP ?? '',
      telefono: estado.telefono ?? '',
      email: estado.email ?? '',
      email2: '',
      email3: '',
      contraseña: estado.contrasena ?? '',
      recaptchaToken: estado.recaptchaToken ?? '',
      direccion: estado.direccion,
    };

    return this.http.post<RegistroClienteResponse>(
      `${this.apiUrl}/api/Auth/registro-cliente`,
      payload,
    );
  }
```

**`datos-fiscales.ts`**

```typescript
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.loginForm.patchValue({ constancia: file });
      this.loginForm.get('constancia')?.updateValueAndValidity();
    }
  }
```

**`datos-fiscales.html`**

```html
<input
  type="file"
  class="form-control form-control-lg"
  id="constanciaInput"
  formControlName="constancia"
  accept=".pdf,.jpg,.png"
  (change)="onFileSelected($event)"
/>
```

### Código — Después

**`pre-registro.service.ts`**

```typescript
  private archivoConstancia: File | null = null;

  guardarArchivoConstancia(file: File): void {
    this.archivoConstancia = file;
  }

  obtenerArchivoConstancia(): File | null {
    return this.archivoConstancia;
  }

  limpiar(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    this.archivoConstancia = null; // Se agregó para limpiar el archivo
  }

  registrar(): Observable<RegistroClienteResponse> {
    const estado = this.obtenerEstado();
    const formData = new FormData();

    formData.append('nombre', estado.nombre ?? '');
    formData.append('apellidoPaterno', estado.apellidoPaterno ?? '');
    formData.append('apellidoMaterno', estado.apellidoMaterno ?? '');
    formData.append('RFC', estado.RFC ?? '');
    formData.append('CURP', estado.CURP ?? '');
    formData.append('telefono', estado.telefono ?? '');
    formData.append('email', estado.email ?? '');
    formData.append('email2', '');
    formData.append('email3', '');
    formData.append('contraseña', estado.contrasena ?? '');
    formData.append('recaptchaToken', estado.recaptchaToken ?? '');

    if (estado.direccion) {
      formData.append('direccion.calle', estado.direccion.calle ?? '');
      formData.append('direccion.numeroExterior', estado.direccion.numeroExterior ?? '');
      if (estado.direccion.numeroInterior) {
        formData.append('direccion.numeroInterior', estado.direccion.numeroInterior);
      }
      formData.append('direccion.colonia', estado.direccion.colonia ?? '');
      formData.append('direccion.codigoPostal', estado.direccion.codigoPostal ?? '');
      formData.append('direccion.ciudad', estado.direccion.ciudad ?? '');
      if (estado.direccion.municipio) {
        formData.append('direccion.municipio', estado.direccion.municipio);
      }
      formData.append('direccion.estado', estado.direccion.estado ?? '');
      formData.append('direccion.pais', estado.direccion.pais ?? '');
      if (estado.direccion.telefono1) {
        formData.append('direccion.telefono1', estado.direccion.telefono1);
      }
    }

    if (this.archivoConstancia) {
      formData.append('constanciaFiscal', this.archivoConstancia, this.archivoConstancia.name);
    }

    return this.http.post<RegistroClienteResponse>(
      `${this.apiUrl}/api/Auth/registro-cliente`,
      formData,
    );
  }
```

**`datos-fiscales.ts`**

```typescript
    // En el constructor / ngOnInit al definir el formulario:
    this.loginForm = this.fb.group({
      rfc: ['', [Validators.required, Validators.pattern(/^[A-ZÑ&]{4}\d{6}[A-Z0-9]{3}$/)]],
      constancia: [null, Validators.required]
    });

  // Manejo del evento del archivo:
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.loginForm.patchValue({ constancia: file });
      this.loginForm.get('constancia')?.updateValueAndValidity();
      this.preRegistroService.guardarArchivoConstancia(file);
    } else {
      this.loginForm.patchValue({ constancia: null });
      this.loginForm.get('constancia')?.updateValueAndValidity();
    }
  }
```

**`datos-fiscales.html`**

```html
<input
  type="file"
  class="form-control form-control-lg"
  id="constanciaInput"
  accept=".pdf,.jpg,.png"
  (change)="onFileSelected($event)"
/>
```

**Motivo:** El formulario enviado mandaba los datos en formato JSON y lo que se espera es que se manden en formato FormData para adjuntar el archivo de la constancia fiscal (`constanciaFiscal`) y estandarizar el envío de datos binarios y estructurados al backend.

---

## 2. Implementación de ruta de constancias fiscales — Backend

**Archivo:** `Compac_Service/DTOs/Request/UsuarioRequestDto.cs`
**Archivo:** `Compac_Service/controllers/Auth/AuthController.cs`
**Archivo:** `Compac_Service/services/Legacy/AdmClienteService.cs`

### Problema

El backend no admitía el envío de archivos adjuntos (`multipart/form-data`) en el registro de clientes debido a que el parámetro del controlador esperaba un JSON plano (`[FromBody]`), el DTO no contaba con un campo para recibir el archivo binario de la constancia, y el servicio no guardaba ningún archivo en disco ni asociaba metadatos a la tabla `admClientes`.

### Solución

1. Modificar `UsuarioRequestDto.cs` agregando la propiedad de tipo `IFormFile?` con el nombre `ConstanciaFiscal`.
2. Modificar `UsuarioRequestDto.cs` cambiando `Email2` y `Email3` a tipo `string?` (anulables) para evitar que la validación automática de modelos de ASP.NET Core rechace peticiones sin dichos correos secundarios, y usar el operador null-coalescing (`?? string.Empty`) en `AdmClienteService.cs` al mapearlos para evitar inserciones de valores NULL en la base de datos.
3. Modificar `AuthController.cs` cambiando la anotación del parámetro `request` en `RegistrarCliente` a `[FromForm]`.
4. Modificar `AdmClienteService.cs` para inyectar la configuración del sistema, leer la ruta base de almacenamiento configurada mediante la variable de entorno `RUTA_CONSTANCIAS`, y en `RegistrarAsync` guardar el stream binario de la constancia en un archivo manteniendo su nombre original.
5. Asociar el valor de la carpeta de la ruta base (el directorio de configuración) en `CTEXTOEXTRA3`, el título estructurado en `CTEXTOEXTRA4` y el nombre original del archivo en `CTEXTOEXTRA5`, aplicando en los tres campos una validación preventiva para truncarlos a 50 caracteres (usando `.Substring(0, 50)`) y evitar errores de inserción en SQL Server debido al límite físico de la base de datos de CONTPAQi.

### Código — Antes

**`UsuarioRequestDto.cs`**

```csharp
    public class UserClientRequestDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string RFC { get; set; } = string.Empty;
        public string CURP { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Email2 { get; set; } = string.Empty;
        public string Email3 { get; set; } = string.Empty;
        public string Contraseña { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string RecaptchaToken { get; set; } = string.Empty;
        public UbicacionDetalleDto? Direccion { get; set; }
    }
```

**`AuthController.cs`**

```csharp
        [HttpPost("registro-cliente")]
        public async Task<IActionResult> RegistrarCliente([FromBody] UserClientRequestDto request)
```

**`AdmClienteService.cs`**

```csharp
        public AdmClienteService(
            IAdmClienteRepository repository,
            IUsuarioAuthRepository repositoryAuth,
            LegacyCompacReadOnlyContext context,
            ReadOnlyContext authContext,
            WriteContext writeContext,
            ILogger<AdmClienteService> logger,
            ICacheService cacheService)
        {
            _repository = repository;
            _context = context;
            _authContext = authContext;
            _writeContext = writeContext;
            _repositoryAuth = repositoryAuth;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<ServiceResult<AdmClienteConDomicilioResponseDto>> RegistrarAsync(UserClientRequestDto clientData)
        {
            //Validar existencia de RFC o email para evitar duplicados
            var existeRfc = await _context.AdmClientes
                .AsNoTracking()
                .AnyAsync(c => c.CRfc == clientData.RFC);
            if (existeRfc)
            {
                _logger.LogWarning("Intento de registro con RFC duplicado: {RFC}", clientData.RFC);
            }

            var nuevoCliente = new AdmCliente
            {
                //Campos Extra
                CTextoExtra1 = clientData.Contraseña,
                CTextoExtra2 = "Cliente registrado desde sitio web",
                CTextoExtra3 = "",
                CTextoExtra4 = "",
                CTextoExtra5 = "",
```

### Código — Después

**`UsuarioRequestDto.cs`**

```csharp
    public class UserClientRequestDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string RFC { get; set; } = string.Empty;
        public string CURP { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Email2 { get; set; }
        public string? Email3 { get; set; }
        public string Contraseña { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string RecaptchaToken { get; set; } = string.Empty;
        public UbicacionDetalleDto? Direccion { get; set; }
        public IFormFile? ConstanciaFiscal { get; set; } // Se agregó para recibir el archivo binario del PDF
    }
```

**`AuthController.cs`**

```csharp
        [HttpPost("registro-cliente")]
        public async Task<IActionResult> RegistrarCliente([FromForm] UserClientRequestDto request) // Se cambió a [FromForm] para admitir multipart/form-data
```

**`AdmClienteService.cs`**

```csharp
        private readonly IConfiguration _config; // Se agregó para acceder a las variables de configuración

        public AdmClienteService(
            IAdmClienteRepository repository,
            IUsuarioAuthRepository repositoryAuth,
            LegacyCompacReadOnlyContext context,
            ReadOnlyContext authContext,
            WriteContext writeContext,
            ILogger<AdmClienteService> logger,
            ICacheService cacheService,
            IConfiguration config) // Se agregó la inyección de IConfiguration
        {
            _repository = repository;
            _context = context;
            _authContext = authContext;
            _writeContext = writeContext;
            _repositoryAuth = repositoryAuth;
            _logger = logger;
            _cacheService = cacheService;
            _config = config; // Se agregó para inicializar el campo
        }

        private string? GetRutaConstancias() // Se agregó para obtener la ruta física del archivo
        {
            var ruta = _config["RUTA_CONSTANCIAS"];
            if (!string.IsNullOrEmpty(ruta)) return ruta;

            ruta = Environment.GetEnvironmentVariable("RUTA_CONSTANCIAS");
            if (!string.IsNullOrEmpty(ruta)) return ruta;

            try
            {
                var dir = Directory.GetCurrentDirectory();
                for (int i = 0; i < 4; i++)
                {
                    var envPath = Path.Combine(dir, ".env");
                    if (File.Exists(envPath))
                    {
                        var lines = File.ReadAllLines(envPath);
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                                continue;

                            var parts = line.Split('=', 2);
                            if (parts.Length == 2 && parts[0].Trim() == "RUTA_CONSTANCIAS")
                            {
                                return parts[1].Trim().Trim('"').Trim('\'');
                            }
                        }
                    }
                    var parent = Directory.GetParent(dir);
                    if (parent == null) break;
                    dir = parent.FullName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al intentar leer el archivo .env para buscar RUTA_CONSTANCIAS");
            }

            return null;
        }

        public async Task<ServiceResult<AdmClienteConDomicilioResponseDto>> RegistrarAsync(UserClientRequestDto clientData)
        {
            string rutaBaseGuardada = ""; // Se agregó para guardar la carpeta base del archivo
            string tituloArchivo = ""; // Se agregó para guardar el título del archivo
            string nombreArchivoOriginal = ""; // Se agregó para guardar el nombre original del archivo

            if (clientData.ConstanciaFiscal != null && clientData.ConstanciaFiscal.Length > 0)
            {
                nombreArchivoOriginal = Path.GetFileName(clientData.ConstanciaFiscal.FileName);
                tituloArchivo = $"CSF{clientData.RFC}";

                var rutaBase = GetRutaConstancias();
                if (string.IsNullOrEmpty(rutaBase))
                {
                    _logger.LogWarning("No se definió la variable de entorno RUTA_CONSTANCIAS. Usando ruta por defecto 'wwwroot/constancias'.");
                    rutaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "constancias");
                }

                rutaBaseGuardada = rutaBase;

                if (!Directory.Exists(rutaBase))
                {
                    Directory.CreateDirectory(rutaBase);
                }

                var nombreArchivoDestino = nombreArchivoOriginal;
                var rutaArchivoGuardado = Path.Combine(rutaBase, nombreArchivoDestino);

                try
                {
                    using (var stream = new FileStream(rutaArchivoGuardado, FileMode.Create))
                    {
                        await clientData.ConstanciaFiscal.CopyToAsync(stream);
                    }
                    _logger.LogInformation("Archivo de constancia fiscal guardado exitosamente en: {Ruta}", rutaArchivoGuardado);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar el archivo de constancia fiscal");
                    return new ServiceResult<AdmClienteConDomicilioResponseDto>
                    {
                        Success = false,
                        Message = "Error al guardar el archivo de constancia fiscal en el servidor."
                    };
                }
            }

            //Validar existencia de RFC o email para evitar duplicados
            var existeRfc = await _context.AdmClientes
                .AsNoTracking()
                .AnyAsync(c => c.CRfc == clientData.RFC);
            if (existeRfc)
            {
                _logger.LogWarning("Intento de registro con RFC duplicado: {RFC}", clientData.RFC);
            }

            var nuevoCliente = new AdmCliente
            {
                //Datos de Contacto
                CEmail1 = clientData.Email,
                CEmail2 = clientData.Email2 ?? string.Empty, // Evita insertar null si es opcional
                CEmail3 = clientData.Email3 ?? string.Empty, // Evita insertar null si es opcional
                CWhatsapp = clientData.Telefono,

                //Campos Extra
                CTextoExtra1 = clientData.Contraseña,
                CTextoExtra2 = "Cliente registrado desde sitio web",
                CTextoExtra3 = rutaBaseGuardada.Length > 50 ? rutaBaseGuardada.Substring(0, 50) : rutaBaseGuardada, // Guardado con truncado preventivo
                CTextoExtra4 = tituloArchivo.Length > 50 ? tituloArchivo.Substring(0, 50) : tituloArchivo, // Guardado con truncado preventivo
                CTextoExtra5 = nombreArchivoOriginal.Length > 50 ? nombreArchivoOriginal.Substring(0, 50) : nombreArchivoOriginal, // Guardado con truncado preventivo
            }
```

**Motivo:** Permitir que el backend acepte y procese el archivo adjunto de la constancia fiscal, almacenándolo en el disco del servidor en una ubicación parametrizable y relacionando sus datos en los campos extra del cliente (`CTEXTOEXTRA3`, `CTEXTOEXTRA4`, `CTEXTOEXTRA5`) en la tabla `admClientes`.

---

## 3. Optimización en la búsqueda de Códigos Postales y Colonias — Frontend

**Archivo:** `front-end/src/app/services/mexico-api.service.ts`
**Archivo:** `front-end/src/app/auth/pre-registro/pages/domicilio/domicilio.ts`

### Problema

Anteriormente, al ingresar un código postal en el wizard, el servicio del frontend realizaba una petición general para obtener las primeras 500 colonias del país (`/colonia?per_page=500`) y filtraba el resultado en memoria en el cliente. Esto provocaba que, si el código postal de búsqueda pertenecía a colonias que no estaban dentro de las primeras 500 del país, la lista de colonias apareciera vacía y no permitiera al usuario avanzar en el registro. Además, al parchar los valores del estado y ciudad recuperados de forma automatizada, se generaban disparos recursivos de eventos de formulario que causaban bucles infinitos.

### Solución

1. En `mexico-api.service.ts`, modificar la lógica en `getSettlementsByZipCode(zipCode)` para realizar una consulta directa y específica utilizando el nuevo endpoint de búsqueda por código postal del servidor de la API: `/codigo-postal/${zipCode}`. Esto asegura que se obtengan todas las colonias correspondientes a dicho CP, sin importar el orden alfabético o geográfico a nivel nacional.
2. En `domicilio.ts`, al parchar los campos autocompletados de `estado` y `ciudad` mediante `patchValue`, agregar la opción `{ emitEvent: false }` para evitar disparar eventos encadenados en el formulario y así evitar ciclos infinitos de recarga.

### Código — Antes

**`mexico-api.service.ts`**

```typescript
  async getSettlementsByZipCode(zipCode: string): Promise<IMexicoSettlement[]> {
    try {
      console.log(`🌐 GET Colonias por CP: ${this.baseUrl}/colonia?per_page=500`);
      const response = await firstValueFrom(
        this.http.get<IApiMexicoResponse<IMexicoSettlement>>(`${this.baseUrl}/colonia?per_page=500`)
      );
      if (response?.data) {
        const filtered = response.data.filter((s: IMexicoSettlement) => s.d_codigo === zipCode);
        return filtered;
      }
      return [];
    } catch (error) {
      console.error('Error fetching settlements:', error);
      return [];
    }
  }
```

**`domicilio.ts`**

```typescript
if (data.state) {
  this.domicilioForm.patchValue({ estado: data.state.d_estado });
  await this.loadCitiesByState(data.state.d_estado);
}

if (data.city) {
  this.domicilioForm.patchValue({ ciudad: data.city.d_ciudad });
}
```

### Código — Después

**`mexico-api.service.ts`**

```typescript
  async getSettlementsByZipCode(zipCode: string): Promise<IMexicoSettlement[]> {
    try {
      console.log(`🌐 GET Colonias por CP: ${this.baseUrl}/codigo-postal/${zipCode}`);
      const response = await firstValueFrom(
        this.http.get<IApiMexicoResponse<IMexicoSettlement>>(`${this.baseUrl}/codigo-postal/${zipCode}`)
      );
      if (response?.data) {
        console.log(`✅ Colonias encontradas para CP ${zipCode}:`, response.data.length);
        return response.data;
      }
      return [];
    } catch (error) {
      console.error('Error fetching settlements:', error);
      return [];
    }
  }
```

**`domicilio.ts`**

```typescript
if (data.state) {
  this.domicilioForm.patchValue(
    { estado: data.state.d_estado },
    { emitEvent: false },
  );
  await this.loadCitiesByState(data.state.d_estado);
}

if (data.city) {
  this.domicilioForm.patchValue(
    { ciudad: data.city.d_ciudad },
    { emitEvent: false },
  );
}
```

**Motivo:** Asegurar que cualquier código postal de la República Mexicana cargue de manera inmediata y completa todas sus colonias asociadas llamando directamente al endpoint optimizado en el servidor local de Mexico API, previniendo bucles de cambio de valor en el formulario de Angular.

---

## 4. Inicialización del campo CCODIGOALTERNO para evitar fallos de inserción — Backend

**Archivo:** `Compac_Service/models/legacy/AdmCliente.cs`
**Archivo:** `Compac_Service/services/Legacy/AdmClienteService.cs`

### Problema

Durante el pre-registro, el guardado en la base de datos `adCABS2016` fallaba al insertar en la tabla `admClientes`. Esto se debía a que el campo `CCODIGOALTERNO` no permite valores `NULL` a nivel de esquema en la base de datos de CONTPAQi y, al no estar definido en la entidad de EF Core (`AdmCliente`), el framework no enviaba ningún valor para dicha columna, arrojando una excepción de violación de restricción NOT NULL.

### Solución

1. En `AdmCliente.cs`, registrar la propiedad `CCodigoAlterno` mapeada a la columna `CCODIGOALTERNO` de tipo string/varchar con longitud de hasta 60 caracteres.
2. En `AdmClienteService.cs`, dentro de la inicialización de `nuevoCliente`, asignar `CCodigoAlterno = string.Empty` para garantizar que la columna se inserte con un valor no nulo por defecto en la base de datos.

### Código — Antes

**`AdmCliente.cs`** (No existía la propiedad mapeada)

```csharp
        [StringLength(30)]
        public string CCodigoCliente { get; set; } = string.Empty;

        [Column("CRAZONSOCIAL")]
        [StringLength(60)]
        public string CRazonSocial { get; set; } = string.Empty;
```

**`AdmClienteService.cs`** (No se inicializaba)

```csharp
            var nuevoCliente = new AdmCliente
            {
                //Datos Personales
                CCodigoCliente = clientData.RFC,
                CRazonSocial = $"{clientData.Nombre} {clientData.ApellidoPaterno} {clientData.ApellidoMaterno}".Trim(),
```

### Código — Después

**`AdmCliente.cs`**

```csharp
        [StringLength(30)]
        public string CCodigoCliente { get; set; } = string.Empty;

        [Column("CCODIGOALTERNO")]
        [StringLength(60)]
        public string CCodigoAlterno { get; set; } = string.Empty;

        [Column("CRAZONSOCIAL")]
        [StringLength(60)]
        public string CRazonSocial { get; set; } = string.Empty;
```

**`AdmClienteService.cs`**

```csharp
            var nuevoCliente = new AdmCliente
            {
                //Datos Personales
                CCodigoCliente = clientData.RFC, // Usamos RFC como código cliente para garantizar unicidad
                CCodigoAlterno = string.Empty,
                CRazonSocial = $"{clientData.Nombre} {clientData.ApellidoPaterno} {clientData.ApellidoMaterno}".Trim(),
```

**Motivo:** Evitar fallos por restricción `NOT NULL` en el campo `CCODIGOALTERNO` de la tabla `admClientes` al momento de insertar el nuevo registro del cliente.
