// =====================================================================================
// DTO RESPONSE ADM MONEDA LEGACY - AdmMonedaResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO para respuestas de API de monedas legacy desde adCABS2016 (tabla admMonedas).
// Incluye campos completos de la tabla legacy sin dependencias.
//
// PROPÓSITO:
// - Comunicación API-Frontend para monedas legacy
// - Exponer datos completos de admMonedas
// - Incluir símbolo, posiciones y timestamps
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para monedas legacy de admMonedas (adCABS2016)
    /// Contiene información completa de monedas del sistema Adminpaq
    /// </summary>
    public class AdmMonedaResponseDto
    {
        // ═══════════════════════════════════════════════════════════════
        // IDENTIFICACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único de la moneda en el sistema legacy
        /// </summary>
        public int IdMoneda { get; set; }

        /// <summary>
        /// Nombre de la moneda (ej: "PESO MEXICANO", "DOLAR AMERICANO")
        /// </summary>
        public string NombreMoneda { get; set; } = string.Empty;

        /// <summary>
        /// Símbolo de la moneda (ej: "$", "€", "£")
        /// </summary>
        public string SimboloMoneda { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DE FORMATO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Posición del símbolo (0 = antes, 1 = después)
        /// </summary>
        public int PosicionSimbolo { get; set; }

        /// <summary>
        /// Descripción del plural de la moneda
        /// </summary>
        public string Plural { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del singular de la moneda
        /// </summary>
        public string Singular { get; set; } = string.Empty;

        /// <summary>
        /// Descripción protegida de la moneda (para reportes)
        /// </summary>
        public string DescripcionProtegida { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN ADICIONAL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID de la bandera de la moneda (para UI)
        /// </summary>
        public int IdBandera { get; set; }

        /// <summary>
        /// Número de decimales para la moneda
        /// </summary>
        public int DecimalesMoneda { get; set; }

        /// <summary>
        /// Timestamp de última modificación
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Clave SAT de la moneda (para facturación electrónica)
        /// </summary>
        public string ClaveSat { get; set; } = string.Empty;
    }
}