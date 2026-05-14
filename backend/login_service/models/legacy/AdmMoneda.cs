using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admMonedas de Adminpaq
    /// Tabla catálogo de monedas sin dependencias
    /// </summary>
    [Table("admMonedas")]
    public class AdmMoneda
    {
        [Key]
        [Column("CIDMONEDA")]
        public int CIdMoneda { get; set; }

        [Required]
        [Column("CNOMBREMONEDA")]
        [StringLength(60)]
        public string CNombreMoneda { get; set; } = string.Empty;

        [Required]
        [Column("CSIMBOLOMONEDA")]
        [StringLength(1)]
        public string CSimboloMoneda { get; set; } = string.Empty;

        [Column("CPOSICIONSIMBOLO")]
        public int CPosicionSimbolo { get; set; }

        [Required]
        [Column("CPLURAL")]
        [StringLength(60)]
        public string CPlural { get; set; } = string.Empty;

        [Required]
        [Column("CSINGULAR")]
        [StringLength(60)]
        public string CSingular { get; set; } = string.Empty;

        [Required]
        [Column("CDESCRIPCIONPROTEGIDA")]
        [StringLength(60)]
        public string CDescripcionProtegida { get; set; } = string.Empty;

        [Column("CIDBANDERA")]
        public int CIdBandera { get; set; }

        [Column("CDECIMALESMONEDA")]
        public int CDecimalesMoneda { get; set; }

        [Required]
        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Required]
        [Column("CCLAVESAT")]
        [StringLength(3)]
        public string CClaveSat { get; set; } = string.Empty;
    }
}
