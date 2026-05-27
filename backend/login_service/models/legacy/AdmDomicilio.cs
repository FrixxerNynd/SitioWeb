using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admDomicilios de Adminpaq
    /// Domicilios de clientes y proveedores
    /// </summary>
    [Table("admDomicilios")]
    public class AdmDomicilio
    {
        [Key]
        [Column("CIDDIRECCION")]
        public int CIdDireccion { get; set; }

        [Column("CIDCATALOGO")]
        public int CIdCatalogo { get; set; } // FK a admClientes.CIDCLIENTEPROVEEDOR

        [Column("CTIPOCATALOGO")]
        public int CTipoCatalogo { get; set; } // 1 = Cliente, 2 = Proveedor

        [Column("CTIPODIRECCION")]
        public int CTipoDireccion { get; set; } // 1 = Fiscal, 2 = Envío, 3 = Otros

        [Column("CNOMBRECALLE")]
        [StringLength(60)]
        public string CNombreCalle { get; set; } = string.Empty;

        [Column("CNUMEROEXTERIOR")]
        [StringLength(30)]
        public string CNumeroExterior { get; set; } = string.Empty;

        [Column("CNUMEROINTERIOR")]
        [StringLength(30)]
        public string CNumeroInterior { get; set; } = string.Empty;

        [Column("CCOLONIA")]
        [StringLength(60)]
        public string CColonia { get; set; } = string.Empty;

        [Column("CCODIGOPOSTAL")]
        [StringLength(6)]
        public string CCodigoPostal { get; set; } = string.Empty;

        [Column("CTELEFONO1")]
        [StringLength(15)]
        public string CTelefono1 { get; set; } = string.Empty;

        [Column("CTELEFONO2")]
        [StringLength(15)]
        public string CTelefono2 { get; set; } = string.Empty;

        [Column("CTELEFONO3")]
        [StringLength(15)]
        public string CTelefono3 { get; set; } = string.Empty;

        [Column("CTELEFONO4")]
        [StringLength(15)]
        public string CTelefono4 { get; set; } = string.Empty;

        [Column("CEMAIL")]
        [StringLength(50)]
        public string CEmail { get; set; } = string.Empty;

        [Column("CDIRECCIONWEB")]
        [StringLength(50)]
        public string CDireccionWeb { get; set; } = string.Empty;

        [Column("CPAIS")]
        [StringLength(60)]
        public string CPais { get; set; } = string.Empty;

        [Column("CESTADO")]
        [StringLength(60)]
        public string CEstado { get; set; } = string.Empty;

        [Column("CCIUDAD")]
        [StringLength(60)]
        public string CCiudad { get; set; } = string.Empty;

        [Column("CTEXTOEXTRA")]
        [StringLength(60)]
        public string CTextoExtra { get; set; } = string.Empty;

        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Column("CMUNICIPIO")]
        [StringLength(60)]
        public string CMunicipio { get; set; } = string.Empty;

        [Column("CSUCURSAL")]
        [StringLength(60)]
        public string CSucursal { get; set; } = string.Empty;

        // Navegación
        [ForeignKey("CIdCatalogo")]
        public virtual AdmCliente? Cliente { get; set; }
    }
}
