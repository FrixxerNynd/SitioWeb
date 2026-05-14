// =====================================================================================
// ENTIDAD LEGACY ADM AGENTES - AdmAgente.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad que mapea la tabla admAgentes del sistema legacy adCABS2016.
// Representa agentes/vendedores con estructura completa de Compaq Adminpaq.
//
// PROPÓSITO:
// - Lectura de datos legacy sin modificaciones
// - Acceso a catálogo completo de agentes del sistema anterior
// - Mapeo exacto de 27 columnas de admAgentes
//
// CARACTERÍSTICAS:
// - Solo lectura (sin modificar BD legacy)
// - Tipos explícitos alineados con SQL Server
// - Nombres de propiedades C# con prefijo C (estilo Adminpaq)
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad legacy para tabla admAgentes de adCABS2016
    /// Catálogo de agentes/vendedores del sistema Compaq Adminpaq
    /// SOLO LECTURA - No modificar datos legacy
    /// </summary>
    [Table("admAgentes", Schema = "dbo")]
    public class AdmAgente
    {
        // ═══════════════════════════════════════════════════════════════
        // CLAVE PRIMARIA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del agente en el sistema legacy
        /// Corresponde a CIDAGENTE en Adminpaq
        /// </summary>
        [Key]
        [Column("CIDAGENTE")]
        public int CIdAgente { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // DATOS PRINCIPALES DEL AGENTE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Código único del agente (ej: "VEN001", "AGE-01")
        /// </summary>
        [Column("CCODIGOAGENTE")]
        [MaxLength(30)]
        public string CCodigoAgente { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del agente/vendedor
        /// </summary>
        [Column("CNOMBREAGENTE")]
        [MaxLength(60)]
        public string CNombreAgente { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de alta del agente en el sistema
        /// </summary>
        [Column("CFECHAALTAAGENTE")]
        public DateTime CFechaAltaAgente { get; set; }

        /// <summary>
        /// Tipo de agente (1=Vendedor, 2=Cobrador, 3=Ambos, etc.)
        /// </summary>
        [Column("CTIPOAGENTE")]
        public int CTipoAgente { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // COMISIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Porcentaje de comisión por ventas (ej: 5.5 = 5.5%)
        /// </summary>
        [Column("CCOMISIONVENTAAGENTE")]
        public double CComisionVentaAgente { get; set; }

        /// <summary>
        /// Porcentaje de comisión por cobros (ej: 2.0 = 2.0%)
        /// </summary>
        [Column("CCOMISIONCOBROAGENTE")]
        public double CComisionCobroAgente { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES CON CLIENTES/PROVEEDORES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del cliente asociado al agente (si aplica)
        /// 0 = Sin cliente asociado
        /// </summary>
        [Column("CIDCLIENTE")]
        public int CIdCliente { get; set; }

        /// <summary>
        /// ID del proveedor asociado al agente (si aplica)
        /// 0 = Sin proveedor asociado
        /// </summary>
        [Column("CIDPROVEEDOR")]
        public int CIdProveedor { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CLASIFICACIONES (6 NIVELES)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Clasificación 1 del agente (configurable por empresa)
        /// 0 = Sin clasificación
        /// </summary>
        [Column("CIDVALORCLASIFICACION1")]
        public int CIdValorClasificacion1 { get; set; }

        /// <summary>
        /// Clasificación 2 del agente (configurable por empresa)
        /// 0 = Sin clasificación
        /// </summary>
        [Column("CIDVALORCLASIFICACION2")]
        public int CIdValorClasificacion2 { get; set; }

        /// <summary>
        /// Clasificación 3 del agente (configurable por empresa)
        /// 0 = Sin clasificación
        /// </summary>
        [Column("CIDVALORCLASIFICACION3")]
        public int CIdValorClasificacion3 { get; set; }

        /// <summary>
        /// Clasificación 4 del agente (configurable por empresa)
        /// 0 = Sin clasificación
        /// </summary>
        [Column("CIDVALORCLASIFICACION4")]
        public int CIdValorClasificacion4 { get; set; }

        /// <summary>
        /// Clasificación 5 del agente (configurable por empresa)
        /// 0 = Sin clasificación
        /// </summary>
        [Column("CIDVALORCLASIFICACION5")]
        public int CIdValorClasificacion5 { get; set; }

        /// <summary>
        /// Clasificación 6 del agente (configurable por empresa)
        /// 0 = Sin clasificación
        /// </summary>
        [Column("CIDVALORCLASIFICACION6")]
        public int CIdValorClasificacion6 { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CAMPOS CONTABLES Y EXTRA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Segmento contable del agente para contabilidad
        /// </summary>
        [Column("CSEGCONTAGENTE")]
        [MaxLength(50)]
        public string CSegContAgente { get; set; } = string.Empty;

        /// <summary>
        /// Campo de texto extra 1 (uso libre por empresa)
        /// </summary>
        [Column("CTEXTOEXTRA1")]
        [MaxLength(50)]
        public string CTextoExtra1 { get; set; } = string.Empty;

        /// <summary>
        /// Campo de texto extra 2 (uso libre por empresa)
        /// </summary>
        [Column("CTEXTOEXTRA2")]
        [MaxLength(50)]
        public string CTextoExtra2 { get; set; } = string.Empty;

        /// <summary>
        /// Campo de texto extra 3 (uso libre por empresa)
        /// </summary>
        [Column("CTEXTOEXTRA3")]
        [MaxLength(50)]
        public string CTextoExtra3 { get; set; } = string.Empty;

        /// <summary>
        /// Campo de fecha extra (uso libre por empresa)
        /// </summary>
        [Column("CFECHAEXTRA")]
        public DateTime CFechaExtra { get; set; }

        /// <summary>
        /// Campo de importe extra 1 (uso libre por empresa)
        /// </summary>
        [Column("CIMPORTEEXTRA1")]
        public double CImporteExtra1 { get; set; }

        /// <summary>
        /// Campo de importe extra 2 (uso libre por empresa)
        /// </summary>
        [Column("CIMPORTEEXTRA2")]
        public double CImporteExtra2 { get; set; }

        /// <summary>
        /// Campo de importe extra 3 (uso libre por empresa)
        /// </summary>
        [Column("CIMPORTEEXTRA3")]
        public double CImporteExtra3 { get; set; }

        /// <summary>
        /// Campo de importe extra 4 (uso libre por empresa)
        /// </summary>
        [Column("CIMPORTEEXTRA4")]
        public double CImporteExtra4 { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CAMPOS DEL SISTEMA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Timestamp de última modificación del registro
        /// Formato: "YYYYMMDD HH:MM:SS.mmm"
        /// </summary>
        [Column("CTIMESTAMP")]
        [MaxLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        /// <summary>
        /// Subcuenta contable 2 del agente
        /// </summary>
        [Column("CSCAGENTE2")]
        [MaxLength(50)]
        public string CScAgente2 { get; set; } = string.Empty;

        /// <summary>
        /// Subcuenta contable 3 del agente
        /// </summary>
        [Column("CSCAGENTE3")]
        [MaxLength(50)]
        public string CScAgente3 { get; set; } = string.Empty;
    }
}
