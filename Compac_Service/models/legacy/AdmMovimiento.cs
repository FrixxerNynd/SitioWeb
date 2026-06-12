using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admMovimientos de Adminpaq
    /// Depende de: admDocumentos (CIDDOCUMENTO)
    /// Depende de: admDocumentosModelo (CIDDOCUMENTODE)
    /// Depende de: admProductos (CIDPRODUCTO)
    /// Depende de: admAlmacenes (CIDALMACEN)
    /// Depende de: admUnidadesMedidaPeso (CIDUNIDAD, CIDUNIDADNC)
    /// </summary>
    [Table("admMovimientos")]
    public class AdmMovimiento
    {
        [Key]
        [Column("CIDMOVIMIENTO")]
        public int CIdMovimiento { get; set; }

        /// <summary>
        /// FK a admDocumentos
        /// </summary>
        [Column("CIDDOCUMENTO")]
        public int CIdDocumento { get; set; }

        [Column("CNUMEROMOVIMIENTO")]
        public double CNumeroMovimiento { get; set; }

        /// <summary>
        /// FK a admDocumentosModelo
        /// </summary>
        [Column("CIDDOCUMENTODE")]
        public int CIdDocumentoDe { get; set; }

        /// <summary>
        /// FK a admProductos
        /// </summary>
        [Column("CIDPRODUCTO")]
        public int CIdProducto { get; set; }

        /// <summary>
        /// FK a admAlmacenes
        /// </summary>
        [Column("CIDALMACEN")]
        public int CIdAlmacen { get; set; }

        [Column("CUNIDADES")]
        public double CUnidades { get; set; }

        [Column("CUNIDADESNC")]
        public double CUnidadesNc { get; set; }

        [Column("CUNIDADESCAPTURADAS")]
        public double CUnidadesCapturadas { get; set; }

        /// <summary>
        /// FK a admUnidadesMedidaPeso
        /// </summary>
        [Column("CIDUNIDAD")]
        public int CIdUnidad { get; set; }

        /// <summary>
        /// FK a admUnidadesMedidaPeso (unidad no convertible)
        /// </summary>
        [Column("CIDUNIDADNC")]
        public int CIdUnidadNc { get; set; }

        [Column("CPRECIO")]
        public double CPrecio { get; set; }

        [Column("CPRECIOCAPTURADO")]
        public double CPrecioCapturado { get; set; }

        [Column("CCOSTOCAPTURADO")]
        public double CCostoCapturado { get; set; }

        [Column("CCOSTOESPECIFICO")]
        public double CCostoEspecifico { get; set; }

        [Column("CNETO")]
        public double CNeto { get; set; }

        [Column("CIMPUESTO1")]
        public double CImpuesto1 { get; set; }

        [Column("CPORCENTAJEIMPUESTO1")]
        public double CPorcentajeImpuesto1 { get; set; }

        [Column("CIMPUESTO2")]
        public double CImpuesto2 { get; set; }

        [Column("CPORCENTAJEIMPUESTO2")]
        public double CPorcentajeImpuesto2 { get; set; }

        [Column("CIMPUESTO3")]
        public double CImpuesto3 { get; set; }

        [Column("CPORCENTAJEIMPUESTO3")]
        public double CPorcentajeImpuesto3 { get; set; }

        [Column("CRETENCION1")]
        public double CRetencion1 { get; set; }

        [Column("CPORCENTAJERETENCION1")]
        public double CPorcentajeRetencion1 { get; set; }

        [Column("CRETENCION2")]
        public double CRetencion2 { get; set; }

        [Column("CPORCENTAJERETENCION2")]
        public double CPorcentajeRetencion2 { get; set; }

        [Column("CDESCUENTO1")]
        public double CDescuento1 { get; set; }

        [Column("CPORCENTAJEDESCUENTO1")]
        public double CPorcentajeDescuento1 { get; set; }

        [Column("CDESCUENTO2")]
        public double CDescuento2 { get; set; }

        [Column("CPORCENTAJEDESCUENTO2")]
        public double CPorcentajeDescuento2 { get; set; }

        [Column("CDESCUENTO3")]
        public double CDescuento3 { get; set; }

        [Column("CPORCENTAJEDESCUENTO3")]
        public double CPorcentajeDescuento3 { get; set; }

        [Column("CDESCUENTO4")]
        public double CDescuento4 { get; set; }

        [Column("CPORCENTAJEDESCUENTO4")]
        public double CPorcentajeDescuento4 { get; set; }

        [Column("CDESCUENTO5")]
        public double CDescuento5 { get; set; }

        [Column("CPORCENTAJEDESCUENTO5")]
        public double CPorcentajeDescuento5 { get; set; }

        [Column("CTOTAL")]
        public double CTotal { get; set; }

        [Column("CPORCENTAJECOMISION")]
        public double CPorcentajeComision { get; set; }

        [Required]
        [Column("CREFERENCIA")]
        [StringLength(20)]
        public string CReferencia { get; set; } = string.Empty;

        [Column("COBSERVAMOV")]
        public string? CObservaMov { get; set; }

        [Column("CAFECTAEXISTENCIA")]
        public int CAfectaExistencia { get; set; }

        [Column("CAFECTADOSALDOS")]
        public int CAfectadoSaldos { get; set; }

        [Column("CAFECTADOINVENTARIO")]
        public int CAfectadoInventario { get; set; }

        [Column("CFECHA")]
        public DateTime CFecha { get; set; }

        [Column("CMOVTOOCULTO")]
        public int CMovtoOculto { get; set; }

        [Column("CIDMOVTOOWNER")]
        public int CIdMovtoOwner { get; set; }

        [Column("CIDMOVTOORIGEN")]
        public int CIdMovtoOrigen { get; set; }

        [Column("CUNIDADESPENDIENTES")]
        public double CUnidadesPendientes { get; set; }

        [Column("CUNIDADESNCPENDIENTES")]
        public double CUnidadesNcPendientes { get; set; }

        [Column("CUNIDADESORIGEN")]
        public double CUnidadesOrigen { get; set; }

        [Column("CUNIDADESNCORIGEN")]
        public double CUnidadesNcOrigen { get; set; }

        [Column("CTIPOTRASPASO")]
        public int CTipoTraspaso { get; set; }

        [Column("CIDVALORCLASIFICACION")]
        public int CIdValorClasificacion { get; set; }

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

        [Required]
        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Column("CGTOMOVTO")]
        public double CGtoMovto { get; set; }

        [Required]
        [Column("CSCMOVTO")]
        [StringLength(50)]
        public string CScMovto { get; set; } = string.Empty;

        [Column("CCOMVENTA")]
        public double CComVenta { get; set; }

        [Column("CIDMOVTODESTINO")]
        public int CIdMovtoDestino { get; set; }

        [Column("CNUMEROCONSOLIDACIONES")]
        public int CNumeroConsolidaciones { get; set; }

        [Required]
        [Column("COBJIMPU01")]
        [StringLength(20)]
        public string CObjImpu01 { get; set; } = string.Empty;

        [Column("CCONFIMP1")]
        public int CConfImp1 { get; set; } = 0;

        [Column("CCONFIMP2")]
        public int CConfImp2 { get; set; } = 0;

        [Column("CCONFIMP3")]
        public int CConfImp3 { get; set; } = 0;

        [Column("CCONFIMP4")]
        public int CConfImp4 { get; set; } = 0;

        // Propiedades de navegación
        [ForeignKey("CIdDocumento")]
        public AdmDocumento? Documento { get; set; }

        [ForeignKey("CIdProducto")]
        public AdmProducto? Producto { get; set; }

        [ForeignKey("CIdAlmacen")]
        public AdmAlmacen? Almacen { get; set; }

        [ForeignKey("CIdUnidad")]
        public AdmUnidadMedidaPeso? Unidad { get; set; }


    }
}
