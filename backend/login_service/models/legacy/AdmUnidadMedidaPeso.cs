using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admUnidadesMedidaPeso de Adminpaq
    /// Tabla catálogo de unidades de medida sin dependencias
    /// </summary>
    [Table("admUnidadesMedidaPeso")]
    public class AdmUnidadMedidaPeso
    {
        [Key]
        [Column("CIDUNIDAD")]
        public int CIdUnidad { get; set; }

        [Required]
        [Column("CNOMBREUNIDAD")]
        [StringLength(60)]
        public string CNombreUnidad { get; set; } = string.Empty;

        [Required]
        [Column("CABREVIATURA")]
        [StringLength(3)]
        public string CAbreviatura { get; set; } = string.Empty;

        [Required]
        [Column("CDESPLIEGUE")]
        [StringLength(3)]
        public string CDespliegue { get; set; } = string.Empty;

        [Required]
        [Column("CCLAVEINT")]
        [StringLength(3)]
        public string CClaveInt { get; set; } = string.Empty;

        [Required]
        [Column("CCLAVESAT")]
        [StringLength(3)]
        public string CClaveSat { get; set; } = string.Empty;
    }
}
