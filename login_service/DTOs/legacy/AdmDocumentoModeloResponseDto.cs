// =====================================================================================
// DTO RESPONSE ADM DOCUMENTO MODELO LEGACY - AdmDocumentoModeloResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO para respuestas de API de modelos de documentos legacy desde adCABS2016 (tabla admDocumentosModelo).
// Incluye campos completos de la tabla legacy sin dependencias.
//
// PROPÓSITO:
// - Comunicación API-Frontend para modelos de documentos legacy
// - Exponer datos completos de admDocumentosModelo
// - Incluir naturaleza, módulo, folios y configuraciones
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para modelos de documentos legacy de admDocumentosModelo (adCABS2016)
    /// Contiene información completa de modelos de documentos del sistema Adminpaq
    /// </summary>
    public class AdmDocumentoModeloResponseDto
    {
        // ═══════════════════════════════════════════════════════════════
        // IDENTIFICACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del modelo de documento en el sistema legacy
        /// </summary>
        public int IdDocumentoDe { get; set; }

        /// <summary>
        /// Descripción del modelo de documento (ej: "FACTURA", "PEDIDO", "REMISION")
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Naturaleza del documento (1 = Entrada, 2 = Salida)
        /// </summary>
        public int Naturaleza { get; set; }

        /// <summary>
        /// Descripción de la naturaleza del documento
        /// </summary>
        public string NaturalezaDescripcion => Naturaleza switch
        {
            1 => "Entrada",
            2 => "Salida",
            _ => "Otro"
        };

        /// <summary>
        /// Indica si afecta existencia (0 = No, 1 = Sí)
        /// </summary>
        public int AfectaExistencia { get; set; }

        /// <summary>
        /// Indica si el documento afecta existencia
        /// </summary>
        public bool AfectaExistenciaFlag => AfectaExistencia == 1;

        /// <summary>
        /// Módulo del sistema (1 = Ventas, 2 = Compras, 3 = Inventarios)
        /// </summary>
        public int Modulo { get; set; }

        /// <summary>
        /// Descripción del módulo
        /// </summary>
        public string ModuloDescripcion => Modulo switch
        {
            1 => "Ventas",
            2 => "Compras",
            3 => "Inventarios",
            _ => "Otro"
        };

        /// <summary>
        /// Número de folio actual
        /// </summary>
        public double NoFolio { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES Y CONFIGURACIONES ADICIONALES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del concepto de documento asumido
        /// </summary>
        public int IdConceptoDoctoAsumido { get; set; }

        /// <summary>
        /// Usa cliente (0 = No, 1 = Sí)
        /// </summary>
        public int UsaCliente { get; set; }

        /// <summary>
        /// Indica si el documento usa cliente
        /// </summary>
        public bool UsaClienteFlag => UsaCliente == 1;

        /// <summary>
        /// Usa proveedor (0 = No, 1 = Sí)
        /// </summary>
        public int UsaProveedor { get; set; }

        /// <summary>
        /// Indica si el documento usa proveedor
        /// </summary>
        public bool UsaProveedorFlag => UsaProveedor == 1;

        /// <summary>
        /// ID del asiento contable asociado
        /// </summary>
        public int IdAsientoContable { get; set; }
    }
}