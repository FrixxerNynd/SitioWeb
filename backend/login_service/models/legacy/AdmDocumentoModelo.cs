using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admDocumentosModelo de Adminpaq
    /// Tabla catálogo de modelos de documentos sin dependencias
    /// </summary>
    [Table("admDocumentosModelo")]
    public class AdmDocumentoModelo
    {
        [Key]
        [Column("CIDDOCUMENTODE")]
        public int CIdDocumentoDe { get; set; }

        [Required]
        [Column("CDESCRIPCION")]
        [StringLength(50)]
        public string CDescripcion { get; set; } = string.Empty;

        [Column("CNATURALEZA")]
        public int CNaturaleza { get; set; }

        [Column("CAFECTAEXISTENCIA")]
        public int CAfectaExistencia { get; set; }

        [Column("CMODULO")]
        public int CModulo { get; set; }

        [Column("CNOFOLIO")]
        public double CNoFolio { get; set; }

        [Column("CIDCONCEPTODOCTOASUMIDO")]
        public int CIdConceptoDoctoAsumido { get; set; }

        [Column("CUSACLIENTE")]
        public int CUsaCliente { get; set; }

        [Column("CUSAPROVEEDOR")]
        public int CUsaProveedor { get; set; }

        [Column("CIDASIENTOCONTABLE")]
        public int CIdAsientoContable { get; set; }
    }
}
