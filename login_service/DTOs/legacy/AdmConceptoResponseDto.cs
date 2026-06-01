// =====================================================================================
// DTO RESPONSE ADM CONCEPTO LEGACY - AdmConceptoResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO para respuestas de API de conceptos legacy desde adCABS2016 (tabla admConceptos).
// Incluye campos principales de la tabla legacy con FKs a documentos modelo y monedas.
//
// PROPÓSITO:
// - Comunicación API-Frontend para conceptos legacy
// - Exponer datos completos de admConceptos
// - Incluir naturaleza, tipo folio, serie y configuraciones
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para conceptos legacy de admConceptos (adCABS2016)
    /// Contiene información completa de conceptos del sistema Adminpaq
    /// </summary>
    public class AdmConceptoResponseDto
    {
        // ═══════════════════════════════════════════════════════════════
        // IDENTIFICACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del concepto en el sistema legacy
        /// </summary>
        public int IdConceptoDocumento { get; set; }

        /// <summary>
        /// Código del concepto (ej: "FAC", "PED", "REM")
        /// </summary>
        public string CodigoConcepto { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del concepto (ej: "FACTURA", "PEDIDO", "REMISION")
        /// </summary>
        public string NombreConcepto { get; set; } = string.Empty;

        /// <summary>
        /// Prefijo del concepto para folios
        /// </summary>
        public string PrefijoConcepto { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES (FKs)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del modelo de documento asociado (FK a admDocumentosModelo)
        /// </summary>
        public int IdDocumentoDe { get; set; }

        /// <summary>
        /// ID de la moneda asociada (FK a admMonedas)
        /// </summary>
        public int IdMoneda { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN PRINCIPAL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Naturaleza del concepto (1 = Cargo, 2 = Abono)
        /// </summary>
        public int Naturaleza { get; set; }

        /// <summary>
        /// Descripción de la naturaleza
        /// </summary>
        public string NaturalezaDescripcion => Naturaleza switch
        {
            1 => "Cargo",
            2 => "Abono",
            _ => "Otro"
        };

        /// <summary>
        /// Tipo de folio (0 = Manual, 1 = Automático)
        /// </summary>
        public int TipoFolio { get; set; }

        /// <summary>
        /// Descripción del tipo de folio
        /// </summary>
        public string TipoFolioDescripcion => TipoFolio switch
        {
            0 => "Manual",
            1 => "Automático",
            _ => "Otro"
        };

        /// <summary>
        /// Número de folio actual
        /// </summary>
        public double NoFolio { get; set; }

        /// <summary>
        /// Serie por omisión
        /// </summary>
        public string SeriePorOmision { get; set; } = string.Empty;

        /// <summary>
        /// Máximo de movimientos permitidos
        /// </summary>
        public int MaximoMovtos { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // FLAGS DE CONFIGURACIÓN (Usa... campos)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Usa nombre de cliente/proveedor (0 = No, 1 = Sí)
        /// </summary>
        public int UsaNombreCteProv { get; set; }

        /// <summary>
        /// Usa RFC (0 = No, 1 = Sí)
        /// </summary>
        public int UsaRfc { get; set; }

        /// <summary>
        /// Usa fecha de vencimiento (0 = No, 1 = Sí)
        /// </summary>
        public int UsaFechaVencimiento { get; set; }

        /// <summary>
        /// Usa moneda (0 = No, 1 = Sí)
        /// </summary>
        public int UsaMoneda { get; set; }

        /// <summary>
        /// Usa tipo de cambio (0 = No, 1 = Sí)
        /// </summary>
        public int UsaTipoCambio { get; set; }

        /// <summary>
        /// Usa código de agente (0 = No, 1 = Sí)
        /// </summary>
        public int UsaCodigoAgente { get; set; }

        /// <summary>
        /// Usa dirección (0 = No, 1 = Sí)
        /// </summary>
        public int UsaDireccion { get; set; }

        /// <summary>
        /// Usa referencia (0 = No, 1 = Sí)
        /// </summary>
        public int UsaReferencia { get; set; }

        /// <summary>
        /// Usa almacén (0 = No, 1 = Sí)
        /// </summary>
        public int UsaAlmacen { get; set; }

        /// <summary>
        /// Usa precio (0 = No, 1 = Sí)
        /// </summary>
        public int UsaPrecio { get; set; }

        /// <summary>
        /// Usa existencia (0 = No, 1 = Sí)
        /// </summary>
        public int UsaExistencia { get; set; }

        /// <summary>
        /// Usa observaciones (0 = No, 1 = Sí)
        /// </summary>
        public int UsaObservaciones { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // FACTURACIÓN ELECTRÓNICA (CFD)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Es comprobante fiscal digital (0 = No, 1 = Sí)
        /// </summary>
        public int EsCfd { get; set; }

        /// <summary>
        /// Indica si es CFD
        /// </summary>
        public bool EsCfdFlag => EsCfd == 1;

        /// <summary>
        /// Clave SAT del concepto
        /// </summary>
        public string ClaveSat { get; set; } = string.Empty;

        /// <summary>
        /// Método de pago
        /// </summary>
        public string MetodoPago { get; set; } = string.Empty;

        /// <summary>
        /// Régimen fiscal
        /// </summary>
        public string RegimenFiscal { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // FLAGS DE PRESENTACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Presenta datos fiscales (0 = No, 1 = Sí)
        /// </summary>
        public int PresentaFiscal { get; set; }

        /// <summary>
        /// Presenta referencia (0 = No, 1 = Sí)
        /// </summary>
        public int PresentaReferencia { get; set; }

        /// <summary>
        /// Presenta detalle (0 = No, 1 = Sí)
        /// </summary>
        public int PresentaDetalle { get; set; }

        /// <summary>
        /// Presenta opción de imprimir (0 = No, 1 = Sí)
        /// </summary>
        public int PresentaImprimir { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CAMPOS ADICIONALES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Orden de cálculo
        /// </summary>
        public int OrdenCalculo { get; set; }

        /// <summary>
        /// Crea cliente automáticamente (0 = No, 1 = Sí)
        /// </summary>
        public int CreaCliente { get; set; }

        /// <summary>
        /// Documento a crédito (0 = No, 1 = Sí)
        /// </summary>
        public int DoctoACredito { get; set; }

        /// <summary>
        /// Estatus del concepto (0 = Inactivo, 1 = Activo)
        /// </summary>
        public int Estatus { get; set; }

        /// <summary>
        /// Indica si el concepto está activo
        /// </summary>
        public bool EstaActivo => Estatus == 1;

        /// <summary>
        /// Timestamp de última modificación
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Segmento contable del concepto
        /// </summary>
        public string SegContConcepto { get; set; } = string.Empty;
    }
}