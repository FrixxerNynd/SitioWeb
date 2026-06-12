using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admAlmacenes de Adminpaq
    /// Tabla catálogo de almacenes sin dependencias
    /// </summary>
    [Table("admAlmacenes")]
    public class AdmAlmacen
    {
        [Key]
        [Column("CIDALMACEN")]
        public int CIdAlmacen { get; set; }

        [Required]
        [Column("CCODIGOALMACEN")]
        [StringLength(30)]
        public string CCodigoAlmacen { get; set; } = string.Empty;

        [Required]
        [Column("CNOMBREALMACEN")]
        [StringLength(60)]
        public string CNombreAlmacen { get; set; } = string.Empty;

        [Column("CFECHAALTAALMACEN")]
        public DateTime CFechaAltaAlmacen { get; set; }

        [Column("CIDVALORCLASIFICACION1")]
        public int CIdValorClasificacion1 { get; set; }

        [Column("CIDVALORCLASIFICACION2")]
        public int CIdValorClasificacion2 { get; set; }

        [Column("CIDVALORCLASIFICACION3")]
        public int CIdValorClasificacion3 { get; set; }

        [Column("CIDVALORCLASIFICACION4")]
        public int CIdValorClasificacion4 { get; set; }

        [Column("CIDVALORCLASIFICACION5")]
        public int CIdValorClasificacion5 { get; set; }

        [Column("CIDVALORCLASIFICACION6")]
        public int CIdValorClasificacion6 { get; set; }

        [Required]
        [Column("CSEGCONTALMACEN")]
        [StringLength(50)]
        public string CSegContAlmacen { get; set; } = string.Empty;

        [Required]
        [Column("CTEXTOEXTRA1")]
        [StringLength(50)]
        public string CTextoExtra1 { get; set; } = string.Empty;

        [Required]
        [Column("CTEXTOEXTRA2")]
        [StringLength(50)]
        public string CTextoExtra2 { get; set; } = string.Empty;

        [Required]
        [Column("CTEXTOEXTRA3")]
        [StringLength(50)]
        public string CTextoExtra3 { get; set; } = string.Empty;

        [Column("CFECHAEXTRA")]
        public DateTime CFechaExtra { get; set; }

        [Column("CIMPORTEEXTRA1")]
        public double CImporteExtra1 { get; set; }

        [Column("CIMPORTEEXTRA2")]
        public double CImporteExtra2 { get; set; }

        [Column("CIMPORTEEXTRA3")]
        public double CImporteExtra3 { get; set; }

        [Column("CIMPORTEEXTRA4")]
        public double CImporteExtra4 { get; set; }

        [Column("CBANDOMICILIO")]
        public int CBanDomicilio { get; set; }

        [Required]
        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Required]
        [Column("CSCALMAC2")]
        [StringLength(50)]
        public string CScAlmac2 { get; set; } = string.Empty;

        [Required]
        [Column("CSCALMAC3")]
        [StringLength(50)]
        public string CScAlmac3 { get; set; } = string.Empty;

        [Column("CSISTORIG")]
        public int CSistOrig { get; set; }
    }
}
