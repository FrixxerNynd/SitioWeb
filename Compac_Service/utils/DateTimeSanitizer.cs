using System;
using System.Globalization;

namespace back_cabs.CRM.Utils
{
    /// <summary>
    /// Proporciona métodos de utilidad para sanitizar y manejar fechas y horas de forma segura.
    /// </summary>
    public static class DateTimeSanitizer
    {
        /// <summary>
        /// Formato estándar para la representación de fechas en todo el sistema (ISO 8601).
        /// </summary>
        public const string StandardFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// Intenta convertir una cadena de texto a un objeto DateTime en formato UTC.
        /// Es el método más seguro para parsear fechas de entrada.
        /// </summary>
        /// <param name="dateString">La cadena de texto que representa la fecha.</param>
        /// <param name="sanitizedDateTime">El resultado DateTime si la conversión es exitosa.</param>
        /// <returns>True si la conversión fue exitosa, de lo contrario False.</returns>
        public static bool TryParseToUtc(string? dateString, out DateTime sanitizedDateTime)
        {
            // Usamos TryParse con CultureInfo.InvariantCulture para evitar problemas con formatos regionales.
            // DateTimeStyles.AdjustToUniversal asegura que el resultado sea UTC.
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var result))
            {
                sanitizedDateTime = result;
                return true;
            }

            sanitizedDateTime = default;
            return false;
        }

        /// <summary>
        /// Valida si una cadena de texto representa una fecha y hora válidas.
        /// </summary>
        /// <param name="dateString">La cadena a validar.</param>
        /// <returns>True si la cadena es una fecha válida, de lo contrario False.</returns>
        public static bool IsValidDateTime(string? dateString)
        {
            return DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }


        /// <summary>
        /// Convierte un objeto DateTime a su representación de cadena en formato estándar UTC (ISO 8601).
        /// Ideal para enviar fechas a APIs o para almacenamiento.
        /// </summary>
        /// <param name="dateTime">El objeto DateTime a formatear.</param>
        /// <returns>La cadena de texto formateada.</returns>
        public static string ToStandardUtcString(DateTime dateTime)
        {
            // Asegurarse de que la fecha esté en UTC antes de formatear
            var utcDateTime = dateTime.ToUniversalTime();
            return utcDateTime.ToString(StandardFormat);
        }

        /// <summary>
        /// Devuelve la fecha y hora actual en UTC, asegurando consistencia en todo el sistema.
        /// </summary>
        public static DateTime GetCurrentUtcTime()
        {
            return DateTime.UtcNow;
        }
    }
}
