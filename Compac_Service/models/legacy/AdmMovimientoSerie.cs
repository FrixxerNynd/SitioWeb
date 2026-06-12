using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admMovimientosSerie de Adminpaq
    /// Tabla de relación N:N entre admMovimientos y admNumerosSerie
    /// Depende de: admMovimientos (CIDMOVIMIENTO)
    /// Depende de: admNumerosSerie (CIDSERIE)
    /// </summary>
    [Table("admMovimientosSerie")]
    public class AdmMovimientoSerie
    {
        /// <summary>
        /// Columna de identidad autogenerada
        /// No es parte de la clave primaria compuesta
        /// </summary>
        [Column("CIDAUTOINCSQL")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CIdAutoIncSql { get; set; }

        /// <summary>
        /// FK a admMovimientos - Parte de la clave primaria compuesta
        /// </summary>
        [Key]
        [Column("CIDMOVIMIENTO", Order = 0)]
        public int CIdMovimiento { get; set; }

        /// <summary>
        /// FK a admNumerosSerie - Parte de la clave primaria compuesta
        /// </summary>
        [Key]
        [Column("CIDSERIE", Order = 1)]
        public int CIdSerie { get; set; }

        [Column("CFECHA")]
        public DateTime CFecha { get; set; }

        // Propiedades de navegación
        [ForeignKey("CIdMovimiento")]
        public AdmMovimiento? Movimiento { get; set; }

        [ForeignKey("CIdSerie")]
        public AdmNumeroSerie? NumeroSerie { get; set; }
    }
}
