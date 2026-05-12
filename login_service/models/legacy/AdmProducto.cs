using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admProductos de Adminpaq
    /// Depende de: admUnidadesMedidaPeso (CIDUNIDADBASE, CIDUNIDADNOCONVERTIBLE, CIDUNIDADCOMPRA, CIDUNIDADVENTA)
    /// Depende de: admMonedas (CIDMONEDA)
    /// </summary>
    [Table("admProductos")]
    public class AdmProducto
    {
        [Key]
        [Column("CIDPRODUCTO")]
        public int CIdProducto { get; set; }

        [Required]
        [Column("CCODIGOPRODUCTO")]
        [StringLength(30)]
        public string CCodigoProducto { get; set; } = string.Empty;

        [Required]
        [Column("CNOMBREPRODUCTO")]
        [StringLength(60)]
        public string CNombreProducto { get; set; } = string.Empty;

        [Column("CTIPOPRODUCTO")]
        public int CTipoProducto { get; set; }

        [Column("CFECHAALTAPRODUCTO")]
        public DateTime CFechaAltaProducto { get; set; }

        [Column("CCONTROLEXISTENCIA")]
        public int CControlExistencia { get; set; }

        [Column("CIDFOTOPRODUCTO")]
        public int CIdFotoProducto { get; set; }

        [Column("CDESCRIPCIONPRODUCTO")]
        public string? CDescripcionProducto { get; set; }

        [Column("CMETODOCOSTEO")]
        public int CMetodoCosteo { get; set; }

        [Column("CPESOPRODUCTO")]
        public double CPesoProducto { get; set; }

        [Column("CCOMVENTAEXCEPPRODUCTO")]
        public double CComVentaExcepProducto { get; set; }

        [Column("CCOMCOBROEXCEPPRODUCTO")]
        public double CComCobroExcepProducto { get; set; }

        [Column("CCOSTOESTANDAR")]
        public double CCostoEstandar { get; set; }

        [Column("CMARGENUTILIDAD")]
        public double CMargenUtilidad { get; set; }

        [Column("CSTATUSPRODUCTO")]
        public int CStatusProducto { get; set; }

        /// <summary>
        /// FK a admUnidadesMedidaPeso
        /// </summary>
        [Column("CIDUNIDADBASE")]
        public int CIdUnidadBase { get; set; }

        /// <summary>
        /// FK a admUnidadesMedidaPeso
        /// </summary>
        [Column("CIDUNIDADNOCONVERTIBLE")]
        public int CIdUnidadNoConvertible { get; set; }

        [Column("CFECHABAJA")]
        public DateTime CFechaBaja { get; set; }

        [Column("CIMPUESTO1")]
        public double CImpuesto1 { get; set; }

        [Column("CIMPUESTO2")]
        public double CImpuesto2 { get; set; }

        [Column("CIMPUESTO3")]
        public double CImpuesto3 { get; set; }

        [Column("CRETENCION1")]
        public double CRetencion1 { get; set; }

        [Column("CRETENCION2")]
        public double CRetencion2 { get; set; }

        [Column("CIDPADRECARACTERISTICA1")]
        public int CIdPadreCaracteristica1 { get; set; }

        [Column("CIDPADRECARACTERISTICA2")]
        public int CIdPadreCaracteristica2 { get; set; }

        [Column("CIDPADRECARACTERISTICA3")]
        public int CIdPadreCaracteristica3 { get; set; }

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
        [Column("CSEGCONTPRODUCTO1")]
        [StringLength(50)]
        public string CSegContProducto1 { get; set; } = string.Empty;

        [Required]
        [Column("CSEGCONTPRODUCTO2")]
        [StringLength(50)]
        public string CSegContProducto2 { get; set; } = string.Empty;

        [Required]
        [Column("CSEGCONTPRODUCTO3")]
        [StringLength(50)]
        public string CSegContProducto3 { get; set; } = string.Empty;

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

        [Column("CPRECIO1")]
        public double CPrecio1 { get; set; }

        [Column("CPRECIO2")]
        public double CPrecio2 { get; set; }

        [Column("CPRECIO3")]
        public double CPrecio3 { get; set; }

        [Column("CPRECIO4")]
        public double CPrecio4 { get; set; }

        [Column("CPRECIO5")]
        public double CPrecio5 { get; set; }

        [Column("CPRECIO6")]
        public double CPrecio6 { get; set; }

        [Column("CPRECIO7")]
        public double CPrecio7 { get; set; }

        [Column("CPRECIO8")]
        public double CPrecio8 { get; set; }

        [Column("CPRECIO9")]
        public double CPrecio9 { get; set; }

        [Column("CPRECIO10")]
        public double CPrecio10 { get; set; }

        [Column("CBANUNIDADES")]
        public int CBanUnidades { get; set; }

        [Column("CBANCARACTERISTICAS")]
        public int CBanCaracteristicas { get; set; }

        [Column("CBANMETODOCOSTEO")]
        public int CBanMetodoCosteo { get; set; }

        [Column("CBANMAXMIN")]
        public int CBanMaxMin { get; set; }

        [Column("CBANPRECIO")]
        public int CBanPrecio { get; set; }

        [Column("CBANIMPUESTO")]
        public int CBanImpuesto { get; set; }

        [Column("CBANCODIGOBARRA")]
        public int CBanCodigoBarra { get; set; }

        [Column("CBANCOMPONENTE")]
        public int CBanComponente { get; set; }

        [Required]
        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Column("CERRORCOSTO")]
        public int CErrorCosto { get; set; }

        [Column("CFECHAERRORCOSTO")]
        public DateTime CFechaErrorCosto { get; set; }

        [Column("CPRECIOCALCULADO")]
        public double CPrecioCalculado { get; set; }

        [Column("CESTADOPRECIO")]
        public int CEstadoPrecio { get; set; }

        [Column("CBANUBICACION")]
        public int CBanUbicacion { get; set; }

        [Column("CESEXENTO")]
        public int CEsExento { get; set; }

        [Column("CEXISTENCIANEGATIVA")]
        public int CExistenciaNegativa { get; set; }

        [Column("CCOSTOEXT1")]
        public double CCostoExt1 { get; set; }

        [Column("CCOSTOEXT2")]
        public double CCostoExt2 { get; set; }

        [Column("CCOSTOEXT3")]
        public double CCostoExt3 { get; set; }

        [Column("CCOSTOEXT4")]
        public double CCostoExt4 { get; set; }

        [Column("CCOSTOEXT5")]
        public double CCostoExt5 { get; set; }

        [Column("CFECCOSEX1")]
        public DateTime CFecCosEx1 { get; set; }

        [Column("CFECCOSEX2")]
        public DateTime CFecCosEx2 { get; set; }

        [Column("CFECCOSEX3")]
        public DateTime CFecCosEx3 { get; set; }

        [Column("CFECCOSEX4")]
        public DateTime CFecCosEx4 { get; set; }

        [Column("CFECCOSEX5")]
        public DateTime CFecCosEx5 { get; set; }

        [Column("CMONCOSEX1")]
        public int CMonCosEx1 { get; set; }

        [Column("CMONCOSEX2")]
        public int CMonCosEx2 { get; set; }

        [Column("CMONCOSEX3")]
        public int CMonCosEx3 { get; set; }

        [Column("CMONCOSEX4")]
        public int CMonCosEx4 { get; set; }

        [Column("CMONCOSEX5")]
        public int CMonCosEx5 { get; set; }

        [Column("CBANCOSEX")]
        public int CBanCosEx { get; set; }

        [Column("CESCUOTAI2")]
        public int CEsCuotaI2 { get; set; }

        [Column("CESCUOTAI3")]
        public int CEsCuotaI3 { get; set; }

        /// <summary>
        /// FK a admUnidadesMedidaPeso
        /// </summary>
        [Column("CIDUNIDADCOMPRA")]
        public int CIdUnidadCompra { get; set; }

        /// <summary>
        /// FK a admUnidadesMedidaPeso
        /// </summary>
        [Column("CIDUNIDADVENTA")]
        public int CIdUnidadVenta { get; set; }

        [Column("CSUBTIPO")]
        public int CSubTipo { get; set; }

        [Required]
        [Column("CCODALTERN")]
        [StringLength(30)]
        public string CCodAltern { get; set; } = string.Empty;

        [Required]
        [Column("CNOMALTERN")]
        [StringLength(60)]
        public string CNomAltern { get; set; } = string.Empty;

        [Required]
        [Column("CDESCCORTA")]
        [StringLength(30)]
        public string CDescCorta { get; set; } = string.Empty;

        /// <summary>
        /// FK a admMonedas
        /// </summary>
        [Column("CIDMONEDA")]
        public int CIdMoneda { get; set; }

        [Column("CUSABASCU")]
        public int CUsaBascu { get; set; }

        [Column("CTIPOPAQUE")]
        public int CTipoPaque { get; set; }

        [Column("CPRECSELEC")]
        public int CPrecSelec { get; set; }

        [Column("CDESGLOSAI2")]
        public int CDesglosaI2 { get; set; }

        [Required]
        [Column("CSEGCONTPRODUCTO4")]
        [StringLength(20)]
        public string CSegContProducto4 { get; set; } = string.Empty;

        [Required]
        [Column("CSEGCONTPRODUCTO5")]
        [StringLength(20)]
        public string CSegContProducto5 { get; set; } = string.Empty;

        [Required]
        [Column("CSEGCONTPRODUCTO6")]
        [StringLength(20)]
        public string CSegContProducto6 { get; set; } = string.Empty;

        [Required]
        [Column("CSEGCONTPRODUCTO7")]
        [StringLength(20)]
        public string CSegContProducto7 { get; set; } = string.Empty;

        [Required]
        [Column("CCTAPRED")]
        [StringLength(150)]
        public string CCtaPred { get; set; } = string.Empty;

        [Column("CNODESCOMP")]
        public int CNoDescomp { get; set; }

        [Column("CIDUNIXML")]
        public int CIdUniXml { get; set; }

        [Column("CNOMODCOMP")]
        public int CNoModComp { get; set; }

        [Required]
        [Column("CCLAVESAT")]
        [StringLength(8)]
        public string CClaveSat { get; set; } = string.Empty;

        [Column("CCANTIDADFISCAL")]
        public double CCantidadFiscal { get; set; }

        // Propiedades de navegación
        [ForeignKey("CIdUnidadBase")]
        public AdmUnidadMedidaPeso? UnidadBase { get; set; }

        [ForeignKey("CIdMoneda")]
        public AdmMoneda? Moneda { get; set; }
    }
}
