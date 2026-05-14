 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admNumerosSerie de Adminpaq
    /// Depende de: admProductos (CIDPRODUCTO)
    /// Depende de: admAlmacenes (CIDALMACEN)
    /// </summary>
    [Table("admNumerosSerie")]
    public class AdmNumeroSerie
    {
        [Key]
        [Column("CIDSERIE")]
        public int CIdSerie { get; set; }

        /// <summary>
        /// FK a admProductos
        /// </summary>
        [Column("CIDPRODUCTO")]
        public int CIdProducto { get; set; }

        [Required]
        [Column("CNUMEROSERIE")]
        [StringLength(30)]
        public string CNumeroSerie { get; set; } = string.Empty;

        /// <summary>
        /// FK a admAlmacenes
        /// </summary>
        [Column("CIDALMACEN")]
        public int CIdAlmacen { get; set; }

        [Column("CESTADO")]
        public int CEstado { get; set; }

        [Column("CESTADOANTERIOR")]
        public int CEstadoAnterior { get; set; }

        [Required]
        [Column("CNUMEROLOTE")]
        [StringLength(30)]
        public string CNumeroLote { get; set; } = string.Empty;

        [Column("CFECHACADUCIDAD")]
        public DateTime CFechaCaducidad { get; set; }

        [Column("CFECHAFABRICACION")]
        public DateTime CFechaFabricacion { get; set; }

        [Required]
        [Column("CPEDIMENTO")]
        [StringLength(30)]
        public string CPedimento { get; set; } = string.Empty;

        [Required]
        [Column("CADUANA")]
        [StringLength(60)]
        public string CAduana { get; set; } = string.Empty;

        [Column("CFECHAPEDIMENTO")]
        public DateTime CFechaPedimento { get; set; }

        [Column("CTIPOCAMBIO")]
        public double CTipoCambio { get; set; }

        [Column("CCOSTO")]
        public double CCosto { get; set; }

        [Required]
        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Column("CNUMADUANA")]
        public int CNumAduana { get; set; }

        [Required]
        [Column("CCLAVESAT")]
        [StringLength(30)]
        public string CClaveSat { get; set; } = string.Empty;

        // Propiedades de navegación
        [ForeignKey("CIdProducto")]
        public AdmProducto? Producto { get; set; }

        [ForeignKey("CIdAlmacen")]
        public AdmAlmacen? Almacen { get; set; }
    }
}
