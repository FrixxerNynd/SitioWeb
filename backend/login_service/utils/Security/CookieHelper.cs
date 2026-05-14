// =====================================================================================
// COOKIE HELPER - CookieHelper.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Proporciona métodos seguros para manejar cookies JWT con las mejores prácticas
// de seguridad: HttpOnly, Secure, SameSite=Strict.
//
// CUÁNDO USARLO:
// - Al establecer cookies JWT después de login
// - Al renovar tokens
// - Al eliminar cookies en logout
//
// SEGURIDAD:
// ✅ HttpOnly: Previene acceso desde JavaScript (XSS protection)
// ✅ Secure: Solo transmite por HTTPS
// ✅ SameSite=Strict: Previene CSRF
// ✅ Path=/: Disponible en toda la aplicación
// ✅ Domain=null: Mayor seguridad, solo dominio actual
//
// =====================================================================================

namespace back_cabs.CRM.utils.Security
{
    /// <summary>
    /// Helper para manejo seguro de cookies JWT
    /// </summary>
    public static class CookieHelper
    {
        private const string JWT_COOKIE_NAME = "AuthToken";
        private const string REFRESH_COOKIE_NAME = "RefreshToken";

        /// <summary>
        /// Establece una cookie JWT con todas las flags de seguridad habilitadas
        /// </summary>
        /// <param name="response">HttpResponse del contexto actual</param>
        /// <param name="token">Token JWT a almacenar</param>
        /// <param name="expiryMinutes">Minutos hasta la expiración (default: 60)</param>
        public static void SetSecureJwtCookie(
            HttpResponse response,
            string token,
            int expiryMinutes = 60)
        {
            var cookieOptions = new CookieOptions
            {
                //  CRÍTICO: Previene acceso desde JavaScript (protege contra XSS)
                HttpOnly = true,

                //  CRÍTICO: Solo transmite por HTTPS (forzado siempre)
                // Nota: En desarrollo local con HTTP, la cookie se enviará pero Chrome mostrará warning
                Secure = true,

                //  CRÍTICO: Previene envío en requests cross-site (protege contra CSRF)
                // Strict = cookie SOLO se envía en requests del mismo sitio
                SameSite = SameSiteMode.Strict,

                //  Expiración alineada con el token JWT
                Expires = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),

                // Disponible en todas las rutas
                Path = "/",

                //  No especificar dominio para mayor seguridad
                // Esto hace que la cookie solo funcione en el dominio exacto
                Domain = null,

                //  Indica si es esencial para el funcionamiento
                IsEssential = true
            };

            response.Cookies.Append(JWT_COOKIE_NAME, token, cookieOptions);
        }

        /// <summary>
        /// Establece una cookie de refresh token (vida más larga)
        /// </summary>
        /// <param name="response">HttpResponse del contexto actual</param>
        /// <param name="refreshToken">Refresh token a almacenar</param>
        /// <param name="expiryDays">Días hasta la expiración (default: 7)</param>
        public static void SetSecureRefreshCookie(
            HttpResponse response,
            string refreshToken,
            int expiryDays = 7)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(expiryDays),
                Path = "/",
                Domain = null,
                IsEssential = true
            };

            response.Cookies.Append(REFRESH_COOKIE_NAME, refreshToken, cookieOptions);
        }

        /// <summary>
        /// Elimina la cookie JWT de forma segura (logout)
        /// </summary>
        /// <param name="response">HttpResponse del contexto actual</param>
        public static void DeleteJwtCookie(HttpResponse response)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1) // Expira inmediatamente
            };

            response.Cookies.Delete(JWT_COOKIE_NAME, cookieOptions);
        }

        /// <summary>
        /// Elimina la cookie de refresh token
        /// </summary>
        /// <param name="response">HttpResponse del contexto actual</param>
        public static void DeleteRefreshCookie(HttpResponse response)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };

            response.Cookies.Delete(REFRESH_COOKIE_NAME, cookieOptions);
        }

        /// <summary>
        /// Elimina todas las cookies de autenticación
        /// </summary>
        /// <param name="response">HttpResponse del contexto actual</param>
        public static void DeleteAllAuthCookies(HttpResponse response)
        {
            DeleteJwtCookie(response);
            DeleteRefreshCookie(response);
        }

        /// <summary>
        /// Lee el token JWT desde la cookie
        /// </summary>
        /// <param name="request">HttpRequest del contexto actual</param>
        /// <returns>Token JWT o null si no existe</returns>
        public static string? GetJwtFromCookie(HttpRequest request)
        {
            return request.Cookies[JWT_COOKIE_NAME];
        }

        /// <summary>
        /// Lee el refresh token desde la cookie
        /// </summary>
        /// <param name="request">HttpRequest del contexto actual</param>
        /// <returns>Refresh token o null si no existe</returns>
        public static string? GetRefreshTokenFromCookie(HttpRequest request)
        {
            return request.Cookies[REFRESH_COOKIE_NAME];
        }

        /// <summary>
        /// Valida que las cookies están configuradas correctamente en desarrollo
        /// </summary>
        /// <param name="environment">Entorno de la aplicación</param>
        /// <returns>Mensaje de advertencia si hay problemas, null si está OK</returns>
        public static string? ValidateCookieConfiguration(IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                return " ADVERTENCIA: En desarrollo local sin HTTPS, las cookies Secure pueden no funcionar correctamente. " +
                       "Para testing local, considera usar HTTPS con certificado de desarrollo o ajustar temporalmente la configuración.";
            }

            return null;
        }
    }
}
