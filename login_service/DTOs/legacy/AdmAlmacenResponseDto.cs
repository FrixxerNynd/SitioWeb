// =====================================================================================
// DTO RESPONSE ADM ALMACEN LEGACY - AdmAlmacenResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO para respuestas de API de almacenes legacy desde adCABS2016 (tabla admAlmacenes).
// Incluye campos completos de la tabla legacy sin dependencias.
//
// PROPÓSITO:
// - Comunicación API-Frontend para almacenes legacy
// - Exponer datos completos de admAlmacenes
// - Incluir clasificaciones, textos extra y timestamps
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para almacenes legacy de admAlmacenes (adCABS2016)
    /// Contiene información completa de almacenes del sistema Adminpaq
    /// </summary>
    public class AdmAlmacenResponseDto
    {
        // ═══════════════════════════════════════════════════════════════
        // IDENTIFICACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del almacén en el sistema legacy
        /// </summary>
        public int IdAlmacen { get; set; }

        /// <summary>
        /// Código del almacén (ej: "ALM001", "BOD-01")
        /// </summary>
        public string CodigoAlmacen { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del almacén
        /// </summary>
        public string NombreAlmacen { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // INFORMACIÓN PRINCIPAL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Fecha de alta del almacén
        /// </summary>
        public DateTime FechaAltaAlmacen { get; set; }

        /// <summary>
        /// Segmento contable del almacén
        /// </summary>
        public string SegContAlmacen { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // CLASIFICACIONES (6 campos disponibles)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID de clasificación 1 (0 si no tiene)
        /// </summary>
        public int Clasificacion1 { get; set; }

        /// <summary>
        /// ID de clasificación 2 (0 si no tiene)
        /// </summary>
        public int Clasificacion2 { get; set; }

        /// <summary>
        /// ID de clasificación 3 (0 si no tiene)
        /// </summary>
        public int Clasificacion3 { get; set; }

        /// <summary>
        /// ID de clasificación 4 (0 si no tiene)
        /// </summary>
        public int Clasificacion4 { get; set; }

        /// <summary>
        /// ID de clasificación 5 (0 si no tiene)
        /// </summary>
        public int Clasificacion5 { get; set; }

        /// <summary>
        /// ID de clasificación 6 (0 si no tiene)
        /// </summary>
        public int Clasificacion6 { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CAMPOS DE TEXTO EXTRA (3 campos)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Campo de texto extra 1 (uso configurable por empresa)
        /// </summary>
        public string TextoExtra1 { get; set; } = string.Empty;

        /// <summary>
        /// Campo de texto extra 2 (uso configurable por empresa)
        /// </summary>
        public string TextoExtra2 { get; set; } = string.Empty;

        /// <summary>
        /// Campo de texto extra 3 (uso configurable por empresa)
        /// </summary>
        public string TextoExtra3 { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // CAMPOS DE FECHA E IMPORTE EXTRA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Fecha extra (uso configurable por empresa)
        /// </summary>
        public DateTime FechaExtra { get; set; }

        /// <summary>
        /// Importe extra 1 (uso configurable por empresa)
        /// </summary>
        public double ImporteExtra1 { get; set; }

        /// <summary>
        /// Importe extra 2 (uso configurable por empresa)
        /// </summary>
        public double ImporteExtra2 { get; set; }

        /// <summary>
        /// Importe extra 3 (uso configurable por empresa)
        /// </summary>
        public double ImporteExtra3 { get; set; }

        /// <summary>
        /// Importe extra 4 (uso configurable por empresa)
        /// </summary>
        public double ImporteExtra4 { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CAMPOS ADICIONALES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Bandera de domicilio (0 = no, 1 = sí)
        /// </summary>
        public int BanDomicilio { get; set; }

        /// <summary>
        /// Timestamp de última modificación
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Segundo segmento contable
        /// </summary>
        public string ScAlmac2 { get; set; } = string.Empty;

        /// <summary>
        /// Tercer segmento contable
        /// </summary>
        public string ScAlmac3 { get; set; } = string.Empty;

        /// <summary>
        /// Sistema de origen
        /// </summary>
        public int SistOrig { get; set; }
    }
}