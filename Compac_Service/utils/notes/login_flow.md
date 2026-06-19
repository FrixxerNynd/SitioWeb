# Análisis del Flujo de Login (`AuthController.Login`)

Este documento analiza en detalle el flujo completo que sigue el método `Login` ubicado en `Compac_Service/controllers/Auth/AuthController.cs` (líneas 493-569).

## 1. Entrada y DTO (`LoginRequest`)

La función recibe como parámetro un objeto DTO llamado `LoginRequest`. 

**Estructura del DTO (`LoginRequest` definido en `AuthController.cs` línea 1453):**
```csharp
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RecaptchaToken { get; set; } = string.Empty;
}
```
**Validaciones de entrada:** 
A diferencia de otros DTOs en el proyecto que usan `[Required]` o `[StringLength]`, este DTO **no tiene anotaciones de validación** (Data Annotations) que restrinjan explícitamente el número de caracteres admitidos. Admite cualquier string que se le envíe en el body del JSON.

---

## 2. Flujo Principal en `AuthController.Login`

### A. Validación de reCAPTCHA (Comentado)
Al inicio del método hay un bloque comentado que hace referencia a `_recaptchaService.ValidarV3Async(request.RecaptchaToken)`. Actualmente esta seguridad está deshabilitada en el código.

### B. Validación de Credenciales
Llama a la función `_usuarioAuthService.ValidarCredencialesAsync(request.Email, request.Password)`. Esta es la validación central del sistema. Si devuelve `null`, el controlador retorna `401 Unauthorized` ("Credenciales inválidas").

### C. Generación de Usuario y Tokens
Si el usuario es válido, mapea sus datos a una clase local `User` (que contiene Id, Email, Name, Role e IdAgenteLegacy).
Luego manda a llamar a la función interna `GenerateTokens(user)`.

### D. Configuración de Cookies
Llama a la función `CookieHelper.SetSecureJwtCookie(Response, tokens.AccessToken, expiryMinutes: 30)` para inyectar el token de forma segura en las cookies del cliente.

### E. Respuesta de Éxito
Retorna un `200 OK` devolviendo en el Body los datos del usuario, el `token` (AccessToken), el `refreshToken`, y un tiempo de expiración de 30 minutos (aunque el JWT por dentro dure más).

### F. Manejo de Excepciones
- Si atrapa un `UnauthorizedAccessException` (por cuenta inactiva), devuelve un `403 Forbidden` indicando "Cliente inactivo".
- Si atrapa un error inesperado, devuelve `500 InternalServerError`.

---

## 3. Análisis Profundo de las Funciones Invocadas

A continuación se detalla qué hace cada función invocada en cascada durante el proceso.

### 3.1. `_usuarioAuthService.ValidarCredencialesAsync` (`UsuarioAuthService.cs`)
Esta función orquesta la validación tanto para usuarios internos del sistema como para clientes del sistema "Legacy" (COMPAC).

1. **Obtiene el usuario base:** Llama a `_usuarioRepository.GetByEmailAsync(email)`.
2. **Validación de cliente inactivo (Si el usuario existe):**
   Si el usuario se encuentra, manda llamar a `_admClienteService.IsActiveAsync(user.IdAgenteLegacy)`. Si esta función retorna `false` (el cliente fue desactivado en COMPAC), lanza una excepción `UnauthorizedAccessException`.
3. **Validación de credenciales (Usuarios Internos):** 
   Llama a `_usuarioRepository.ValidateCredentialsAsync(email, contrasena)`.
4. **Fallback a Sistema Legacy:**
   Si la validación anterior devuelve `null` (es decir, no es un usuario interno válido), asume que es un cliente de COMPAC e intenta loguearlo con `_admClienteService.ValidateCredentialsAsync(email, contrasena)`.
   Si esto tiene éxito, crea en memoria un objeto `UsuarioAuth` con el rol `"CLIENTE_LEGACY"` y los datos de ese cliente para permitirle el acceso.

---

### 3.2. Funciones llamadas dentro del servicio

#### A. `_usuarioRepository.ValidateCredentialsAsync` (`UsuarioAuthRepository.cs`)
**Propósito:** Validar el password de un usuario de la base de datos local `UsuariosAuth`.
- Busca de nuevo el usuario por email.
- **Validación del Password:** Implementa una doble validación:
  1. Compara si la contraseña en base de datos es igual a la enviada **en texto plano** (`usuario.Password == password`).
  2. Si no es así, genera un hash SHA-256 de la contraseña enviada llamando a `ApiUtilities.GenerateSha256Hash(password)` y lo compara con el guardado en base de datos (`usuario.Password == contrasenaHash`).
- Si ninguna coincide, retorna `null`. Si coincide, retorna el usuario.

#### B. `_admClienteService.ValidateCredentialsAsync` (`AdmClienteService.cs`)
**Propósito:** Validar clientes legacy que provienen de AdminPAQ/COMPAC.
- Busca al cliente por email en el repositorio legacy (`_repository.GetByEmailAsync`).
- Si el estatus en base de datos `CEstatus` es diferente de `1` (Activo), lanza una `UnauthorizedAccessException`.
- Delega la revisión de la contraseña a `_repositoryAuth.ValidateClientCredentialsAsync(cliente.CIdClienteProveedor, contrasena)`.

#### C. `_repositoryAuth.ValidateClientCredentialsAsync` (`UsuarioAuthRepository.cs`)
**Propósito:** Consulta la tabla `Auth_Clientes` donde se guardan las contraseñas reales de los clientes COMPAC.
- Recupera `datosCliente` y los datos del `Cliente` (COMPAC).
- Aplica las siguientes validaciones:
  - Que existan ambos registros.
  - Que el cliente no tenga estatus `0` (Inactivo).
  - Que no tenga su contraseña en blanco.
- **Validación del Password:** Compara **únicamente mediante hash**. Hashea la contraseña de entrada con `ApiUtilities.GenerateSha256Hash(password)` y verifica si es igual a `datosCliente.password`. 

---

### 3.3. Otras funciones utilitarias y de seguridad

#### `GenerateTokens(User user)` (`AuthController.cs`)
- Genera el **Access Token (JWT)** usando los claims (id, email, name, role, idCliente). Lo firma con HMAC SHA-256 (`SecurityAlgorithms.HmacSha256`) usando el secret key del appsettings. 
- **Duración del JWT:** El token tiene un tiempo de expiración interno de **540 minutos** (9 horas), a pesar de que el return dice 30 minutos (hay una inconsistencia lógica allí).
- Genera el **Refresh Token** de manera rudimentaria concatenando un `Guid` con el ID del usuario (`Guid.NewGuid().ToString() + "_" + user.Id`).

#### `CookieHelper.SetSecureJwtCookie` (`CookieHelper.cs`)
- Agrega la cookie HTTP con el token generado.
- **Nivel de seguridad alto:** Aplica las banderas de seguridad recomendadas:
  - `HttpOnly = true` (Inaccesible vía Javascript - previene XSS)
  - `Secure = true` (Sólo viaja por HTTPS)
  - `SameSite = SameSiteMode.Strict` (Previene ataques CSRF)
  - `Path = "/"`
- Configura la expiración de la cookie a 30 minutos.

#### `ApiUtilities.GenerateSha256Hash(string rawData)`
- Toma el string de la contraseña, lo convierte a un arreglo de bytes usando UTF8, calcula su SHA-256 usando el namespace `System.Security.Cryptography`, y devuelve un string hexadecimal con la representación de ese hash. Este hash se usa tanto en el login interno como en el legacy.
